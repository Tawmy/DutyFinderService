using System.Text.Json;

namespace XivApiClient.Models;

public record SearchResult<TFields>
{
    public required double Score { get; init; }
    public required string Sheet { get; init; }
    public required uint RowId { get; init; }
    public uint? SubrowId { get; init; }
    public required TFields Fields { get; init; }
    public JsonElement? Transient { get; init; }
}

public record SearchResponse<TFields>
{
    public Guid? Next { get; init; }
    public required SearchResult<TFields>[] Results { get; init; }
    public required string Schema { get; init; }
    public required string Version { get; init; }
}