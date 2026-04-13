using DutyFinderService.Data;

namespace DutyFinderService.DTOs;

public record RaidDto
{
    public required string Name { get; init; }
    public required string Expansion { get; init; }
    public required string Series { get; init; }
    public required string Section { get; init; }
    public required string Patch { get; init; }
    public required short Level { get; init; }
    public required RaidDifficulty Difficulty { get; init; }
    public required string ImageUrl { get; init; }
    public short? AchievementId { get; init; }
}