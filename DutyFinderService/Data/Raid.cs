namespace DutyFinderService.Data;

public record Raid
{
    public required string Name { get; init; }
    public string? NameFallback { get; init; }
    public required string Abbreviation { get; init; }
    public required Expansion Expansion { get; init; }
    public required string Series { get; init; }
    public required string Section { get; init; }
    public required string Patch { get; init; }
    public required short Level { get; init; }
    public required RaidDifficulty Difficulty { get; init; }
    public short? AchievementId { get; init; }
}