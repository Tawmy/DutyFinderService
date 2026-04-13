using DutyFinderService.Data;

namespace DutyFinderService.DTOs;

public record AllianceRaidDto
{
    public required string Name { get; init; }
    public required string Expansion { get; init; }
    public required string Series { get; init; }
    public required string Patch { get; init; }
    public required short Level { get; init; }
    public required AllianceRaidDifficulty Difficulty { get; init; }
    public required string ImageUrl { get; init; }
    public short? AchievementId { get; init; }
}