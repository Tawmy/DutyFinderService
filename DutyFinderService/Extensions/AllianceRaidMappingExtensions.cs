using AspNetCoreExtensions;
using DutyFinderService.Data;
using DutyFinderService.DTOs;

namespace DutyFinderService.Extensions;

public static class AllianceRaidMappingExtensions
{
    extension(AllianceRaid source)
    {
        public AllianceRaidDto ToDto(string imageUrl)
        {
            return new AllianceRaidDto
            {
                Name = source.Name,
                Expansion = source.Expansion.GetDisplayName(),
                Series = source.Series,
                Patch = source.Patch,
                Level = source.Level,
                Difficulty = source.Difficulty,
                ImageUrl = imageUrl,
                AchievementId = source.AchievementId
            };
        }
    }
}