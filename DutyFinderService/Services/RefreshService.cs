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
            logger.LogInformation("All images match, no refresh necessary.");
        }
        else
        {
            logger.LogInformation("Not all images have been updated this patch, updating...");
            await xivApiService.UpdateImagesAsync(dataService.GetRaids(), ffxivPatch, ct);
            await xivApiService.UpdateImagesAsync(dataService.GetTrials(), ffxivPatch, ct);
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