using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using DutyFinderService.Data;
using DutyFinderService.Db;
using DutyFinderService.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace DutyFinderService.Services;

internal class DataService(IDbContextFactory<DatabaseContext> dbContextFactory)
{
    private const string Directory = "Json";
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonStringEnumConverter() } };
    private ImmutableArray<Image>? _images;
    private ImmutableArray<Trial>? _trials;

    public async Task InitializeAsync(CT ct = default)
    {
        _trials = await LoadTrialsAsync(ct);
        _images = await LoadImagesAsync(ct);
    }

    public ImmutableArray<Trial> GetTrials()
    {
        return _trials ?? throw new InvalidOperationException("Trials aren't loaded.");
    }

    public ImmutableArray<Image> GetImages()
    {
        return _images ?? throw new InvalidOperationException("Images aren't loaded.");
    }

    public async Task<ImmutableArray<Image>> LoadImagesAsync(CT ct)
    {
        var ctx = await dbContextFactory.CreateDbContextAsync(ct);
        var images = await ctx.Images.ToListAsync(ct);
        return [..images];
    }

    private static async Task<ImmutableArray<Trial>> LoadTrialsAsync(CT ct)
    {
        await using var stream = File.OpenRead($"{Directory}/trials.json");
        var trials = await JsonSerializer.DeserializeAsync<Trial[]>(stream, Options, ct);

        return [..trials ?? throw new JsonException("Failed to parse trials.")];
    }
}