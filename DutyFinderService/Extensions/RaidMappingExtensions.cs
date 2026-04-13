using AspNetCoreExtensions;
using DutyFinderService.Data;
using DutyFinderService.DTOs;

namespace DutyFinderService.Extensions;

public static class RaidMappingExtensions
{
    extension(Raid source)
    {
        public RaidDto ToDto(string imageUrl)
        {
            return new RaidDto
            {
                Name = source.Name,
                Expansion = source.Expansion.GetDisplayName(),
                Series = source.Series,
                Section = source.Section,
                Patch = source.Patch,
                Level = source.Level,
                Difficulty = source.Difficulty,
                ImageUrl = imageUrl,
                AchievementId = source.AchievementId
            };
        }
    }
}