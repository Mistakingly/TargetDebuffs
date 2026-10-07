using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace TargetDebuffs;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool Locked { get; set; } = false;

    public bool OnlyMine { get; set; } = true;
    public bool IncludePets { get; set; } = true;

    public bool OnlyDoTs { get; set; } = true;

    public bool OnlyBosses { get; set; } = false;

    public int BossMinHp { get; set; } = 3_000_000;

    public bool HideNativeBar { get; set; } = false;
    public bool CloseGaps { get; set; } = true;

    public HashSet<string>? EnabledJobs { get; set; } = null;
    public List<string> CustomNames { get; set; } = new();

    public bool OnlyDebuffs { get; set; } = true;

    public bool ShowTimers { get; set; } = true;
    public bool ShowStacks { get; set; } = true;

    public float TextScale { get; set; } = 1.0f;
    public Vector4 TextColor { get; set; } = new(1f, 1f, 1f, 1f);
    public bool TextOutline { get; set; } = true;

    public float IconScale { get; set; } = 1.5f;
    public int IconsPerRow { get; set; } = 8;
    public float Spacing { get; set; } = 4f;

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pi) => pluginInterface = pi;

    public void Save() => pluginInterface?.SavePluginConfig(this);

    public bool IsJobEnabled(string job)
    {
        if (EnabledJobs == null)
        {
            return !DotList.DefaultOffJobs.Contains(job);
        }

        return EnabledJobs.Contains(job);
    }

    public void SetJobEnabled(string job, bool enabled)
    {
        EnabledJobs ??= new HashSet<string>(
            DotList.Groups.Select(g => g.Job).Where(j => !DotList.DefaultOffJobs.Contains(j)));

        if (enabled)
        {
            EnabledJobs.Add(job);
        }
        else
        {
            EnabledJobs.Remove(job);
        }

        Save();
    }
}
