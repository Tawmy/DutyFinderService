using DutyFinderService.Extensions;
using DutyFinderService.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace DutyFinderService;

public static class Endpoints
{
    extension(WebApplication app)
    {
        public void MapEndpoints()
        {
            app.MapGet("/refresh", async (DataService dataService, XivApiService xivApiService,
                HybridCache hybridCache, CT ct) =>
            {
                var currentFfxivPatch = dataService.GetCurrentFfxivPatch();
                await xivApiService.UpdateImagesAsync(dataService.GetRaids(), currentFfxivPatch, ct);
                await xivApiService.UpdateImagesAsync(dataService.GetAllianceRaids(), currentFfxivPatch, ct);
                await xivApiService.UpdateImagesAsync(dataService.GetTrials(), currentFfxivPatch, ct);
                await dataService.LoadImagesAsync(ct);

                await hybridCache.RemoveAsync("raids", ct);
                await hybridCache.RemoveAsync("alliance-raids", ct);
                await hybridCache.RemoveAsync("trials", ct);

                return Results.NoContent();
            }).RequireAuthorization();

            app.MapGet("/raids", async (DataService dataService, HybridCache hybridCache, CT ct) =>
            {
                return await hybridCache.GetOrCreateAsync("raids", _ =>
                {
                    var images = dataService.GetImages();
                    return ValueTask.FromResult(dataService.GetRaids()
                        .Select(x =>
                            x.ToDto(images.First(y =>
                                y.Name.Equals(x.Name, StringComparison.Ordinal)).ImageUrl))
                        .ToList());
                }, cancellationToken: ct);
            });

            app.MapGet("/alliance-raids", async (DataService dataService, HybridCache hybridCache, CT ct) =>
            {
                return await hybridCache.GetOrCreateAsync("alliance-raids", async _ =>
                {
                    var images = dataService.GetImages();
                    return dataService.GetAllianceRaids()
                        .Select(x =>
                            x.ToDto(images.First(y =>
                                y.Name.Equals(x.Name, StringComparison.Ordinal)).ImageUrl))
                        .ToList();
                }, cancellationToken: ct);
            });

            app.MapGet("/trials", async (DataService dataService, HybridCache hybridCache, CT ct) =>
            {
                return await hybridCache.GetOrCreateAsync("trials", async _ =>
                {
                    var images = dataService.GetImages();
                    return dataService.GetTrials()
                        .Select(x =>
                            x.ToDto(images.First(y =>
                                y.Name.Equals(x.Name, StringComparison.Ordinal)).ImageUrl))
                        .ToList();
                }, cancellationToken: ct);
            });
        }
    }
}