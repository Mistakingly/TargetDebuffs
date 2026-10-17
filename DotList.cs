using System;
using System.Collections.Generic;

namespace TargetDebuffs;

public sealed class DotFilter
{
    public DotFilter(HashSet<uint> ids, HashSet<string> names)
    {
        Ids = ids;
        Names = names;
    }

    public HashSet<uint> Ids { get; }
    public HashSet<string> Names { get; }

    public bool Contains(uint statusId, string statusName)
    {
        return Ids.Contains(statusId) || Names.Contains(statusName);
    }
}

public static class DotList
{
    public static readonly (string Job, uint[] StatusIds)[] Groups =
    {
        ("WHM", new uint[] { 143, 144, 1871, 2035 }),
        ("SCH", new uint[] { 179, 189, 1895, 2039, 3089 }),
        ("AST", new uint[] { 838, 843, 1881, 2041 }),
        ("SGE", new uint[] { 2614, 2615, 2616, 2864, 3108, 3976 }),
        ("DRG", new uint[] { 118, 1312, 2719 }),
        ("RPR", new uint[] { 2586 }),
        ("MNK", new uint[] { 246, 1309 }),
        ("SAM", new uint[] { 1228, 1319 }),
        ("BRD", new uint[] { 124, 129, 1200, 1201, 1321, 1322 }),
        ("MCH", new uint[] { 1866, 2019 }),
        ("BLM", new uint[] { 161, 162, 163, 1210, 1324, 2075, 3871, 3872 }),
        ("SMN", new uint[] { 1214, 1215, 1326, 1327 }),
        ("PLD", new uint[] { 725 }),
        ("GNB", new uint[] { 1837, 1838 }),
    };

    public static readonly string[] DefaultOffJobs = { "GNB" };

    public static DotFilter Build(Configuration config)
    {
        var ids = new HashSet<uint>();

        foreach (var (job, statusIds) in Groups)
        {
            if (!config.IsJobEnabled(job))
            {
                continue;
            }

            foreach (var id in statusIds)
            {
                ids.Add(id);
            }
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var custom in config.CustomNames)
        {
            if (!string.IsNullOrWhiteSpace(custom))
            {
                names.Add(custom.Trim());
            }
        }

        return new DotFilter(ids, names);
    }
}
