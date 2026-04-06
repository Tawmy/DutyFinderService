using DutyFinderService.Data;
using DutyFinderService.Db;
using DutyFinderService.Db.Models;
using Microsoft.EntityFrameworkCore;
using XivApiClient.Abstractions;

namespace DutyFinderService.Services;

internal class XivApiService(IXivApiClient xivApi, DatabaseContext ctx)
{
    private const string Sheet = "ContentFinderCondition";

    public async Task UpdateImagesAsync(IEnumerable<Raid> raids, string ffxivPatch, CT ct = default)
    {
        await UpdateImagesAsync(raids.Select(x => new Content(x.Name, x.NameSuffix)), ffxivPatch, ct);
    }

    public async Task UpdateImagesAsync(IEnumerable<Trial> trials, string ffxivPatch, CT ct = default)
    {
        await UpdateImagesAsync(trials.Select(x => new Content(x.Name, x.NameSuffix)), ffxivPatch, ct);
    }

    private async Task UpdateImagesAsync(IEnumerable<Content> contents, string ffxivPatch, CT ct)
    {
        var contentDatas = await RetrieveContentDatasAsync(contents, ct);
        await AddContentDatasToDbAsync(contentDatas, ffxivPatch, ct);
    }

    private async Task<IEnumerable<ContentData>> RetrieveContentDatasAsync(IEnumerable<Content> contents, CT ct)
    {
        List<ContentData> contentDatas = [];
        foreach (var content in contents)
        {
            uint? rowId = null;
            try
            {
                rowId = await GetRowIdAsync(content.FullName, ct);
            }
            catch
            {
                // do nothing
            }

            rowId ??= await GetRowIdAsync(content.Name, ct);

            if (await GetImageUrlAsync(rowId.Value, ct) is not { } imageUrl)
            {
                throw new XivApiException($"Failed to retrieve image url for rowId {rowId}");
            }

            contentDatas.Add(new ContentData(content, imageUrl));
        }

        return contentDatas;
    }

    private async Task<uint> GetRowIdAsync(string name, CT ct)
    {
        var search = await xivApi.SearchAsync($"Name~\"{name}\"", Sheet, cancellationToken: ct);

        if (search.Results.FirstOrDefault() is not { } firstResult)
        {
            throw new XivApiException($"No results for name {name}");
        }

        if (firstResult.Sheet is not Sheet)
        {
            throw new XivApiException($"First result for {name} is not of type {Sheet}");
        }

        if (!firstResult.Fields.TryGetProperty("Name", out var sheetNameJson) ||
            sheetNameJson.GetString() is not { } sheetName)
        {
            throw new XivApiException($"First result for {name} does not have a name itself");
        }

        if (!sheetName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new XivApiException($"Sheet name {sheetName} does not match {name}");
        }

        return firstResult.RowId;
    }

    private async Task<string> GetImageUrlAsync(uint rowId, CT ct)
    {
        var row = await xivApi.GetRowAsync(Sheet, rowId.ToString(), cancellationToken: ct);

        if (!row.Fields.TryGetProperty("Image", out var image))
        {
            throw new XivApiException($"Sheet row {rowId} does not have 'Image' property");
        }

        if (!image.TryGetProperty("path", out var imagePathJson) || imagePathJson.GetString() is not { } imagePath)
        {
            throw new XivApiException($"Sheet row {rowId} does not have an image path");
        }

        return $"https://v2.xivapi.com/api/asset?path={imagePath}&format=png";
    }

    private async Task AddContentDatasToDbAsync(IEnumerable<ContentData> contentDatas, string ffxivPatch, CT ct)
    {
        var dbEntries = await ctx.Images.Where(x => contentDatas.Select(y => y.Content.FullName).Contains(x.Name))
            .ToListAsync(ct);

        foreach (var contentData in contentDatas)
        {
            if (dbEntries.FirstOrDefault(x => x.Name == contentData.Content.FullName) is { } dbEntry)
            {
                if (!dbEntry.ImageUrl.Equals(contentData.ImageUrl) ||
                    !dbEntry.LastUpdatedPatch.Equals(ffxivPatch, StringComparison.OrdinalIgnoreCase))
                {
                    // update url if xivapi returned different url
                    dbEntry.ImageUrl = contentData.ImageUrl;
                    dbEntry.LastUpdatedPatch = ffxivPatch;
                }
            }
            else
            {
                // data not saved to database yet, create new entry
                ctx.Images.Add(new Image
                {
                    Name = contentData.Content.FullName,
                    ImageUrl = contentData.ImageUrl,
                    LastUpdatedPatch = ffxivPatch
                });
            }
        }

        await ctx.SaveChangesAsync(ct);
    }

    private record Content(string Name, string? NameSuffix)
    {
        public string FullName => !string.IsNullOrEmpty(NameSuffix) ? $"{Name} {NameSuffix}" : Name;
    }

    private record ContentData(Content Content, string ImageUrl);

    public class XivApiException(string message) : Exception(message);
}