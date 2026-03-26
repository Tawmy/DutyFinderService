namespace DutyFinderService.Data;

public record Trial
{
    public required string Name { get; init; }
    public required Expansion Expansion { get; init; }
    public required string Patch { get; init; }
    public required short Level { get; init; }
    public required TrialDifficulty Difficulty { get; init; }
    public short? AchievementId { get; init; }
}