using DutyFinderService.Extensions;
using DutyFinderService.Services;

namespace DutyFinderService;

public static class Endpoints
{
    extension(WebApplication app)
    {
        public void MapEndpoints()
        {
            app.MapGet("/refresh", async (DataService dataService, XivApiService xivApiService, CT ct) =>
            {
                var currentFfxivPatch = dataService.GetCurrentFfxivPatch();
                await xivApiService.UpdateImagesAsync(dataService.GetRaids(), currentFfxivPatch, ct);
                await xivApiService.UpdateImagesAsync(dataService.GetTrials(), currentFfxivPatch, ct);
                await dataService.LoadImagesAsync(ct);
                return Results.NoContent();
            }).RequireAuthorization();

            app.MapGet("/raids", (DataService dataService) =>
            {
                var images = dataService.GetImages();
                return dataService.GetRaids()
                    .Select(x =>
                        x.ToDto(images.First(y =>
                            y.Name.Equals(x.Name, StringComparison.Ordinal)).ImageUrl));
            });

            app.MapGet("/trials", (DataService dataService) =>
            {
                var images = dataService.GetImages();
                return dataService.GetTrials()
                    .Select(x =>
                        x.ToDto(images.First(y =>
                            y.Name.Equals(x.Name, StringComparison.Ordinal)).ImageUrl));
            });
        }
    }
}