namespace DutyFinderService.Data;

public record AllianceRaid
{
    public required string Name { get; init; }
    public required Expansion Expansion { get; init; }
    public required string Series { get; init; }
    public required string Patch { get; init; }
    public required short Level { get; init; }
    public required AllianceRaidDifficulty Difficulty { get; init; }
    public short? AchievementId { get; init; }
}