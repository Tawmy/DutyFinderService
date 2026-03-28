using System.Text.Json.Serialization;
using AspNetCoreExtensions.EfCore;
using DutyFinderService.Db;
using DutyFinderService.Extensions;
using DutyFinderService.Services;
using XivApiClient.DepdendencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(x => x.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddXivApiClient();
builder.Services.AddSingleton<DataService>();
builder.Services.AddScoped<XivApiService>();

builder.AddCustomOpenApi();
builder.AddCustomAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "Duty Finder API"));

app.MapGet("/trials", (DataService dataService) => dataService.GetTrials().Select(x => x.ToDto()));
app.MapGet("/refresh", async (DataService dataService, XivApiService service, CT ct) =>
{
    await service.UpdateImagesAsync(dataService.GetTrials(), ct);
    return Results.NoContent();
}).RequireAuthorization();

await app.Services.GetRequiredService<DataService>().InitializeAsync();
await app.Services.MigrateDatabaseAsync<DatabaseContext>();

app.Run();