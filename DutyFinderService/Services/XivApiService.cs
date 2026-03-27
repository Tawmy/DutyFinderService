using DutyFinderService.Data;
using DutyFinderService.Db;
using DutyFinderService.Db.Models;
using Microsoft.EntityFrameworkCore;
using XivApiClient.Abstractions;

namespace DutyFinderService.Services;

internal class XivApiService(IXivApiClient xivApi, DatabaseContext ctx)
{
    private const string Sheet = "ContentFinderCondition";

    public async Task UpdateImagesAsync(IEnumerable<Trial> trials, CT ct = default)
    {
        await UpdateImagesAsync(trials.Select(x => x.Name), ct);
    }

    private async Task UpdateImagesAsync(IEnumerable<string> names, CT ct)
    {
        var contentDatas = await RetrieveContentDatasAsync(names, ct);
        await AddContentDatasToDbAsync(contentDatas, ct);
    }

    private async Task<IEnumerable<ContentData>> RetrieveContentDatasAsync(IEnumerable<string> names, CT ct)
    {
        List<ContentData> contentDatas = [];
        foreach (var name in names)
        {
            var rowId = await GetRowIdAsync(name, ct);

            if (await GetImageUrlAsync(rowId, ct) is not { } imageUrl)
            {
                throw new XivApiException($"Failed to retrieve image url for rowId {rowId}");
            }

            contentDatas.Add(new ContentData(name, imageUrl));
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

    private async Task AddContentDatasToDbAsync(IEnumerable<ContentData> contentDatas, CT ct)
    {
        var dbEntries = await ctx.Images.Where(x => contentDatas.Select(y => y.Name).Contains(x.Name)).ToListAsync(ct);

        foreach (var contentData in contentDatas)
        {
            if (dbEntries.FirstOrDefault(x => x.Name == contentData.Name) is { } dbEntry)
            {
                if (!dbEntry.ImageUrl.Equals(contentData.ImageUrl))
                {
                    // update url if xivapi returned different url
                    dbEntry.ImageUrl = contentData.ImageUrl;
                }
            }
            else
            {
                // data not saved to database yet, create new entry
                ctx.Images.Add(new Image
                {
                    Name = contentData.Name,
                    ImageUrl = contentData.ImageUrl
                });
            }
        }

        await ctx.SaveChangesAsync(ct);
    }

    private record ContentData(string Name, string ImageUrl);

    public class XivApiException(string message) : Exception(message);
}