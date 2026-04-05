using AspNetCoreExtensions;
using DutyFinderService.Data;
using DutyFinderService.DTOs;

namespace DutyFinderService.Extensions;

public static class TrialMappingExtensions
{
    extension(Trial source)
    {
        public TrialDto ToDto(string imageUrl)
        {
            return new TrialDto
            {
                Name = source.Name,
                Expansion = source.Expansion.GetDisplayName(),
                Patch = source.Patch,
                Level = source.Level,
                Difficulty = source.Difficulty,
                ImageUrl = imageUrl,
                AchievementId = source.AchievementId
            };
        }
    }
}