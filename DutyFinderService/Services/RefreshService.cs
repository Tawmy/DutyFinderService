using DutyFinderService.Db;
using Microsoft.EntityFrameworkCore;

namespace DutyFinderService.Services;

internal class RefreshService(
    ILogger<RefreshService> logger,
    DataService dataService,
    XivApiService xivApiService,
    IDbContextFactory<DatabaseContext> dbContextFactory)
{
    public async Task UpdateImagesIfNecessaryAsync(CT ct = default)
    {
        var ffxivPatch = dataService.GetCurrentFfxivPatch();

        var allImagesMatchPatch = await AllImagesMatchPatch(ffxivPatch, ct);
        if (allImagesMatchPatch)
        {
            logger.LogInformation("All images match, no update necessary.");
        }
        else
        {
            logger.LogInformation("Not all images have been updated this patch, updating...");

            var raids = await xivApiService.UpdateImagesAsync(dataService.GetRaids(), ffxivPatch, ct);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Updated images for {n} raids.", raids.Count);

                if (raids.NameFallbackUsed.Count > 0)
                {
                    logger.LogInformation("NameFallback used for raids: {Names}",
                        string.Join(", ", raids.NameFallbackUsed));
                }
            }

            var allianceRaids = await xivApiService.UpdateImagesAsync(dataService.GetAllianceRaids(), ffxivPatch, ct);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Updated images for {n} alliance raids.", allianceRaids.Count);

                if (allianceRaids.NameFallbackUsed.Count > 0)
                {
                    logger.LogInformation("NameFallback used for alliance raids: {Names}",
                        string.Join(", ", allianceRaids.NameFallbackUsed));
                }
            }

            var trials = await xivApiService.UpdateImagesAsync(dataService.GetTrials(), ffxivPatch, ct);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Updated images for {n} trials.", trials.Count);

                if (trials.NameFallbackUsed.Count > 0)
                {
                    logger.LogInformation("NameFallback used for trials: {Names}",
                        string.Join(", ", trials.NameFallbackUsed));
                }
            }

            await dataService.LoadImagesAsync(ct);
            logger.LogInformation("Images for duties have been updated.");
        }
    }

    private async Task<bool> AllImagesMatchPatch(string ffxivPatch, CT ct)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var images = await ctx.Images.Select(x => x.LastUpdatedPatch)
            .GroupBy(x => x)
            .ToListAsync(ct);

        return images.Count == 1 && images[0].Key.Equals(ffxivPatch, StringComparison.Ordinal);
    }
}