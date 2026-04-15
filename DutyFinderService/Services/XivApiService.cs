using DutyFinderService.Data;
using DutyFinderService.Db;
using DutyFinderService.Db.Models;
using Microsoft.EntityFrameworkCore;
using XivApiClient.Abstractions;

namespace DutyFinderService.Services;

internal class XivApiService(IXivApiClient xivApi, DatabaseContext ctx, ILogger<XivApiService> logger)
{
    private const string Sheet = "ContentFinderCondition";

    public async Task<ContentUpdateResult> UpdateImagesAsync(IEnumerable<Raid> raids, string ffxivPatch,
        CT ct = default)
    {
        var result = await UpdateImagesAsync(raids.Select(x => new Content(x.Name, x.NameFallback)), ffxivPatch, ct);
        result.Log(logger, "raids");
        return result;
    }

    public async Task<ContentUpdateResult> UpdateImagesAsync(IEnumerable<AllianceRaid> allianceRaids, string ffxivPatch,
        CT ct = default)
    {
        var result = await UpdateImagesAsync(allianceRaids.Select(x => new Content(x.Name)), ffxivPatch, ct);
        result.Log(logger, "alliance raids");
        return result;
    }

    public async Task<ContentUpdateResult> UpdateImagesAsync(IEnumerable<Trial> trials, string ffxivPatch,
        CT ct = default)
    {
        var result = await UpdateImagesAsync(trials.Select(x => new Content(x.Name)), ffxivPatch, ct);
        result.Log(logger, "trials");
        return result;
    }

    private async Task<ContentUpdateResult> UpdateImagesAsync(IEnumerable<Content> contents, string ffxivPatch, CT ct)
    {
        var contentDatas = await RetrieveContentDatasAsync(contents, ct);

        var nameFallbackUsed = contentDatas
            .Where(x => x.NameFallbackUsed)
            .Select(x => x.Content.Name)
            .ToList();

        var count = await AddContentDatasToDbAsync(contentDatas, ffxivPatch, ct);
        return new ContentUpdateResult(count, nameFallbackUsed);
    }

    private async Task<IEnumerable<ContentData>> RetrieveContentDatasAsync(IEnumerable<Content> contents, CT ct)
    {
        return await Task.WhenAll(contents.Select(x => RetrieveContentDataAsync(x, ct)));
    }

    private async Task<ContentData> RetrieveContentDataAsync(Content content, CT ct)
    {
        uint? rowId = null;
        var usedNameFallback = false;

        try
        {
            rowId = await GetRowIdAsync(content.Name, ct);
        }
        catch
        {
            // do nothing
        }

        if (rowId is null && !string.IsNullOrEmpty(content.NameFallback))
        {
            rowId = await GetRowIdAsync(content.NameFallback, ct);
            usedNameFallback = true;
        }

        if (rowId is null)
        {
            throw new XivApiException($"Failed to retrieve RowId for {content.Name}");
        }

        if (await GetImageUrlAsync(rowId.Value, ct) is not { } imageUrl)
        {
            throw new XivApiException($"Failed to retrieve image url for rowId {rowId}");
        }

        return new ContentData(content, imageUrl, usedNameFallback);
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

    private async Task<int> AddContentDatasToDbAsync(IEnumerable<ContentData> contentDatas, string ffxivPatch, CT ct)
    {
        var dbEntries = await ctx.Images.Where(x => contentDatas.Select(y => y.Content.Name).Contains(x.Name))
            .ToListAsync(ct);

        var count = 0;

        foreach (var contentData in contentDatas)
        {
            if (dbEntries.FirstOrDefault(x => x.Name == contentData.Content.Name) is { } dbEntry)
            {
                if (!dbEntry.ImageUrl.Equals(contentData.ImageUrl) ||
                    !dbEntry.LastUpdatedPatch.Equals(ffxivPatch, StringComparison.OrdinalIgnoreCase))
                {
                    // update url if xivapi returned different url
                    dbEntry.ImageUrl = contentData.ImageUrl;
                    dbEntry.LastUpdatedPatch = ffxivPatch;

                    count++;
                }
            }
            else
            {
                // data not saved to database yet, create new entry
                ctx.Images.Add(new Image
                {
                    Name = contentData.Content.Name,
                    ImageUrl = contentData.ImageUrl,
                    LastUpdatedPatch = ffxivPatch
                });

                count++;
            }
        }

        await ctx.SaveChangesAsync(ct);
        return count;
    }

    private record Content(string Name, string? NameFallback = null);

    private record ContentData(Content Content, string ImageUrl, bool NameFallbackUsed = false);

    internal record ContentUpdateResult(int Count, IReadOnlyList<string> NameFallbackUsed)
    {
        public void Log(ILogger logger, string label)
        {
            if (!logger.IsEnabled(LogLevel.Information)) return;

            logger.LogInformation("Updated images for {n} {label}.", Count, label);

            if (NameFallbackUsed.Count > 0)
            {
                logger.LogInformation("NameFallback used for {label}: {Names}", label,
                    string.Join(", ", NameFallbackUsed));
            }
        }
    }

    private class XivApiException(string message) : Exception(message);
}