using System.Text.Json.Serialization;
using AspNetCoreExtensions.EfCore;
using DutyFinderService;
using DutyFinderService.Db;
using DutyFinderService.Extensions;
using DutyFinderService.Services;
using XivApiClient.DepdendencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(x => x.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContextFactory<DatabaseContext>();
builder.Services.AddXivApiClient();
builder.Services.AddSingleton<DataService>();
builder.Services.AddScoped<XivApiService>();
builder.Services.AddScoped<RefreshService>();

builder.AddCustomOpenApi();
builder.AddCustomAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "Duty Finder API"));

app.MapEndpoints();

await app.Services.MigrateDatabaseAsync<DatabaseContext>();

await app.Services.GetRequiredService<DataService>().InitializeAsync();
var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<RefreshService>().UpdateImagesIfNecessaryAsync();

app.Run();