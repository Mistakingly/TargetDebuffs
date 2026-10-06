using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace TargetDebuffs.Windows;

public sealed class ConfigWindow : Window
{
    private readonly Configuration config;

    public ConfigWindow(Configuration config)
        : base("Target Debuffs Settings##TargetDebuffsConfig", ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.config = config;
        Size = new Vector2(320, 0);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var locked = config.Locked;
        if (ImGui.Checkbox("Lock overlay", ref locked))
        {
            config.Locked = locked;
            config.Save();
        }
        ImGui.TextDisabled("Unlocked: drag the overlay anywhere. Locked: click-through and fixed.");

        ImGui.Separator();

        var onlyMine = config.OnlyMine;
        if (ImGui.Checkbox("Only statuses I applied", ref onlyMine))
        {
            config.OnlyMine = onlyMine;
            config.Save();
        }

        var pets = config.IncludePets;
        if (ImGui.Checkbox("Include statuses from my pet / summon", ref pets))
        {
            config.IncludePets = pets;
            config.Save();
        }

        var onlyDebuffs = config.OnlyDebuffs;
        if (ImGui.Checkbox("Only debuffs (hide buffs)", ref onlyDebuffs))
        {
            config.OnlyDebuffs = onlyDebuffs;
            config.Save();
        }

        ImGui.Separator();

        var timers = config.ShowTimers;
        if (ImGui.Checkbox("Show timers", ref timers))
        {
            config.ShowTimers = timers;
            config.Save();
        }

        var stacks = config.ShowStacks;
        if (ImGui.Checkbox("Show stack counts", ref stacks))
        {
            config.ShowStacks = stacks;
            config.Save();
        }

        var scale = config.IconScale;
        if (ImGui.SliderFloat("Icon scale", ref scale, 0.5f, 4f, "%.2f"))
        {
            config.IconScale = scale;
            config.Save();
        }

        var perRow = config.IconsPerRow;
        if (ImGui.SliderInt("Icons per row", ref perRow, 1, 20))
        {
            config.IconsPerRow = perRow;
            config.Save();
        }

        var spacing = config.Spacing;
        if (ImGui.SliderFloat("Spacing", ref spacing, 0f, 20f, "%.0f"))
        {
            config.Spacing = spacing;
            config.Save();
        }
    }
}
