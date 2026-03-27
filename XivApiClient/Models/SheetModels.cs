using System.Text.Json;

namespace XivApiClient.Models;

public record SheetMetadata
{
    public required string Name { get; init; }
}

public record ListResponse
{
    public required SheetMetadata[] Sheets { get; init; }
}

public record RowResult<TFields>
{
    public required uint RowId { get; init; }
    public uint? SubrowId { get; init; }
    public required TFields Fields { get; init; }
    public JsonElement? Transient { get; init; }
}

public record SheetResponse<TFields>
{
    public required RowResult<TFields>[] Rows { get; init; }
    public required string Schema { get; init; }
    public required string Version { get; init; }
}

public record RowResponse<TFields>
{
    public required string Schema { get; init; }
    public required string Version { get; init; }
    public required uint RowId { get; init; }
    public uint? SubrowId { get; init; }
    public required TFields Fields { get; init; }
    public JsonElement? Transient { get; init; }
}