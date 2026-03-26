using DutyFinderService.Data;

namespace DutyFinderService.DTOs;

public record TrialDto
{
    public required string Name { get; init; }
    public required string Expansion { get; init; }
    public required string Patch { get; init; }
    public required short Level { get; init; }
    public required TrialDifficulty Difficulty { get; init; }
    public short? AchievementId { get; init; }
}