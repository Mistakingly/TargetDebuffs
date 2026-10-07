using System;
using System.Collections.Generic;

namespace TargetDebuffs;

// Statuses are matched by their in-game name (in your game client's language).
// If one is missing or named differently, add it under "Extra status names" in settings.
public static class DotList
{
    public static readonly (string Job, string[] Names)[] Groups =
    {
        ("WHM", new[] { "Aero", "Aero II", "Dia" }),
        ("SCH", new[] { "Bio", "Bio II", "Biolysis" }),
        ("AST", new[] { "Combust", "Combust II", "Combust III" }),
        ("SGE", new[] { "Eukrasian Dosis", "Eukrasian Dosis II", "Eukrasian Dosis III" }),
        ("DRG", new[] { "Chaos Thrust", "Chaotic Spring" }),
        ("RPR", new[] { "Death's Design" }),
        ("MNK", new[] { "Demolish" }),
        ("SAM", new[] { "Higanbana" }),
        ("BRD", new[] { "Venomous Bite", "Windbite", "Caustic Bite", "Stormbite" }),
        ("MCH", new[] { "Bioblaster" }),
        ("BLM", new[] { "Thunder", "Thunder II", "Thunder III", "Thunder IV", "High Thunder", "High Thunder II" }),
        ("SMN", new[] { "Bio III", "Miasma III", "Slipstream" }),
        ("PLD", new[] { "Goring Blade" }),
        ("GNB", new[] { "Sonic Break", "Bow Shock" }),
    };

    // Jobs that start switched off until you tick them.
    public static readonly string[] DefaultOffJobs = { "GNB" };

    public static HashSet<string> BuildSet(Configuration config)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (job, names) in Groups)
        {
            if (!config.IsJobEnabled(job))
            {
                continue;
            }

            foreach (var name in names)
            {
                set.Add(name);
            }
        }

        foreach (var custom in config.CustomNames)
        {
            if (!string.IsNullOrWhiteSpace(custom))
            {
                set.Add(custom.Trim());
            }
        }

        return set;
    }
}
