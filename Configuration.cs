using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace TargetDebuffs;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // Locked = click-through, no background, cannot be moved.
    public bool Locked { get; set; } = false;

    // Only show statuses applied by you (and optionally your pet/summon).
    public bool OnlyMine { get; set; } = true;
    public bool IncludePets { get; set; } = true;

    // Only show detrimental statuses (debuffs). Turn off to also see your buffs on friendly targets.
    public bool OnlyDebuffs { get; set; } = true;

    public bool ShowTimers { get; set; } = true;
    public bool ShowStacks { get; set; } = true;

    public float IconScale { get; set; } = 1.5f;
    public int IconsPerRow { get; set; } = 8;
    public float Spacing { get; set; } = 4f;

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pi) => pluginInterface = pi;

    public void Save() => pluginInterface?.SavePluginConfig(this);
}
