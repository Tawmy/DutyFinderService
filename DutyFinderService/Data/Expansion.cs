using System.ComponentModel.DataAnnotations;

namespace DutyFinderService.Data;

public enum Expansion
{
    [Display(Name = "A Realm Reborn")] ARealmReborn,
    [Display(Name = "Heavensward")] Heavensward,
    [Display(Name = "Stormblood")] Stormblood,
    [Display(Name = "Shadowbringers")] Shadowbringers,
    [Display(Name = "Endwalker")] Endwalker
}