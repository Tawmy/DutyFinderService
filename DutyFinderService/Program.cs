using System.Text.Json.Serialization;
using DutyFinderService.Extensions;
using DutyFinderService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<DataService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "Duty Finder API"));

app.MapGet("/trials", (DataService dataService) => dataService.GetTrials().Select(x => x.ToDto()));

await app.Services.GetRequiredService<DataService>().InitializeAsync();

app.Run();