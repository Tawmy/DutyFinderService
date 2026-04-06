using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCoreExtensions;
using DutyFinderService.Data;
using DutyFinderService.Db;
using DutyFinderService.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace DutyFinderService.Services;

internal class DataService(IConfiguration configuration, IDbContextFactory<DatabaseContext> dbContextFactory)
{
    private const string Directory = "Json";
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonStringEnumConverter() } };
    private readonly string _ffxivPatch = configuration.GetGuardedConfiguration(EnvironmentVariables.FfxivPatch);
    private ImmutableArray<Image>? _images;
    private ImmutableArray<Raid>? _raids;
    private ImmutableArray<Trial>? _trials;

    public async Task InitializeAsync(CT ct = default)
    {
        await LoadRaidsAsync(ct);
        await LoadTrialsAsync(ct);
        await LoadImagesAsync(ct);
    }

    public string GetCurrentFfxivPatch()
    {
        return _ffxivPatch;
    }

    public ImmutableArray<Raid> GetRaids()
    {
        return _raids ?? throw new InvalidOperationException("Trials aren't loaded.");
    }

    public ImmutableArray<Trial> GetTrials()
    {
        return _trials ?? throw new InvalidOperationException("Trials aren't loaded.");
    }

    public ImmutableArray<Image> GetImages()
    {
        return _images ?? throw new InvalidOperationException("Images aren't loaded.");
    }

    public async Task LoadImagesAsync(CT ct)
    {
        var ctx = await dbContextFactory.CreateDbContextAsync(ct);
        var images = await ctx.Images.ToListAsync(ct);
        _images = [..images];
    }

    private async Task LoadRaidsAsync(CT ct)
    {
        await using var stream = File.OpenRead($"{Directory}/raids.json");
        var raids = await JsonSerializer.DeserializeAsync<Raid[]>(stream, Options, ct);

        _raids = [..raids ?? throw new JsonException("Failed to parse raids.")];
    }

    private async Task LoadTrialsAsync(CT ct)
    {
        await using var stream = File.OpenRead($"{Directory}/trials.json");
        var trials = await JsonSerializer.DeserializeAsync<Trial[]>(stream, Options, ct);

        _trials = [..trials ?? throw new JsonException("Failed to parse trials.")];
    }
}