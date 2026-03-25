using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using DutyFinderService.Data;

namespace DutyFinderService.Services;

public class DataService
{
    private const string Directory = "Json";
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonStringEnumConverter() } };
    private ImmutableArray<Trial>? _trials;

    public async Task InitializeAsync(CT ct = default)
    {
        _trials = await LoadTrialsAsync(ct);
    }

    public ImmutableArray<Trial> GetTrials()
    {
        return _trials ?? throw new InvalidOperationException("Trials aren't loaded.");
    }

    private static async Task<ImmutableArray<Trial>> LoadTrialsAsync(CT ct)
    {
        await using var stream = File.OpenRead($"{Directory}/trials.json");
        var trials = await JsonSerializer.DeserializeAsync<Trial[]>(stream, Options, ct);

        return trials?.ToImmutableArray() ?? throw new JsonException("Failed to parse trials.");
    }
}