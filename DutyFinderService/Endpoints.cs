using AspNetCoreExtensions;
using DutyFinderService.Data;
using DutyFinderService.Db;
using DutyFinderService.Extensions;
using DutyFinderService.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Hybrid;

namespace DutyFinderService;

public static class Endpoints
{
    extension(WebApplication app)
    {
        public void MapEndpoints()
        {
            app.MapHealthChecks("/health").RequireAuthorization();

            app.MapHealthChecks("/health/db", new HealthCheckOptions
            {
                Predicate = check => check.Name.Equals(nameof(DatabaseContext), StringComparison.OrdinalIgnoreCase)
            }).RequireAuthorization();

            app.MapGet("/refresh", async (DataService dataService, XivApiService xivApiService,
                HybridCache hybridCache, CT ct) =>
            {
                var currentFfxivPatch = dataService.GetCurrentFfxivPatch();
                var raids = await xivApiService.UpdateImagesAsync(dataService.GetRaids(), currentFfxivPatch, ct);
                var allianceRaids = await xivApiService.UpdateImagesAsync(dataService.GetAllianceRaids(),
                    currentFfxivPatch, ct);
                var trials = await xivApiService.UpdateImagesAsync(dataService.GetTrials(), currentFfxivPatch, ct);
                await dataService.LoadImagesAsync(ct);

                await hybridCache.RemoveAsync("raids", ct);
                await hybridCache.RemoveAsync("alliance-raids", ct);
                await hybridCache.RemoveAsync("trials", ct);

                return Results.Ok(raids.NameFallbackUsed
                    .Concat(allianceRaids.NameFallbackUsed)
                    .Concat(trials.NameFallbackUsed));
            }).RequireAuthorization();

            app.MapGet("/raids", async (DataService dataService, HybridCache hybridCache,
                Expansion? expansion, RaidDifficulty? difficulty, CT ct) =>
            {
                var raids = await hybridCache.GetOrCreateAsync("raids", _ =>
                {
                    var images = dataService.GetImages();
                    return ValueTask.FromResult(dataService.GetRaids()
                        .Select(x =>
                            x.ToDto(images.First(y =>
                                y.Name.Equals(x.Name, StringComparison.Ordinal)).ImageUrl))
                        .ToList());
                }, cancellationToken: ct);

                return raids
                    .Where(x => expansion is null || x.Expansion == expansion.Value.GetDisplayName())
                    .Where(x => difficulty is null || x.Difficulty == difficulty.Value);
            });

            app.MapGet("/alliance-raids", async (DataService dataService, HybridCache hybridCache,
                Expansion? expansion, AllianceRaidDifficulty? difficulty, CT ct) =>
            {
                var allianceRaids = await hybridCache.GetOrCreateAsync("alliance-raids", async _ =>
                {
                    var images = dataService.GetImages();
                    return dataService.GetAllianceRaids()
                        .Select(x =>
                            x.ToDto(images.First(y =>
                                y.Name.Equals(x.Name, StringComparison.Ordinal)).ImageUrl))
                        .ToList();
                }, cancellationToken: ct);

                return allianceRaids
                    .Where(x => expansion is null || x.Expansion == expansion.Value.GetDisplayName())
                    .Where(x => difficulty is null || x.Difficulty == difficulty.Value);
            });

            app.MapGet("/trials", async (DataService dataService, HybridCache hybridCache,
                Expansion? expansion, TrialDifficulty? difficulty, CT ct) =>
            {
                var trials = await hybridCache.GetOrCreateAsync("trials", async _ =>
                {
                    var images = dataService.GetImages();
                    return dataService.GetTrials()
                        .Select(x =>
                            x.ToDto(images.First(y =>
                                y.Name.Equals(x.Name, StringComparison.Ordinal)).ImageUrl))
                        .ToList();
                }, cancellationToken: ct);

                return trials
                    .Where(x => expansion is null || x.Expansion == expansion.Value.GetDisplayName())
                    .Where(x => difficulty is null || x.Difficulty == difficulty.Value);
            });
        }
    }
}