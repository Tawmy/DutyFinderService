namespace XivApiClient.Models;

public record VersionMetadata
{
    public required string Key { get; init; }
    public required string[] Names { get; init; }
}

public record VersionsResponse
{
    public required VersionMetadata[] Versions { get; init; }
}