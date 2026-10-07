using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace TargetDebuffs.Windows;

public sealed class ConfigWindow : Window
{
    private readonly Configuration config;
    private readonly StatusTracker tracker;
    private readonly TargetBarHider barHider;
    private string newName = string.Empty;

    public ConfigWindow(Configuration config, StatusTracker tracker, TargetBarHider barHider)
        : base("Target Debuffs Settings##TargetDebuffsConfig", ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.config = config;
        this.tracker = tracker;
        this.barHider = barHider;
        Size = new Vector2(360, 0);
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

        var onlyDots = config.OnlyDoTs;
        if (ImGui.Checkbox("Only show DoTs (from the job list)", ref onlyDots))
        {
            config.OnlyDoTs = onlyDots;
            config.Save();
        }

        if (config.OnlyDoTs)
        {
            DrawDotSettings();
        }
        else
        {
            var onlyDebuffs = config.OnlyDebuffs;
            if (ImGui.Checkbox("Only debuffs (hide buffs)", ref onlyDebuffs))
            {
                config.OnlyDebuffs = onlyDebuffs;
                config.Save();
            }
        }

        ImGui.Separator();
        DrawBossSettings();

        ImGui.Separator();
        DrawNativeBarSettings();

        ImGui.Separator();
        ImGui.Text("Text");

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

        var textScale = config.TextScale;
        if (ImGui.SliderFloat("Text size", ref textScale, 0.5f, 3f, "%.2f"))
        {
            config.TextScale = textScale;
            config.Save();
        }

        var textColor = config.TextColor;
        if (ImGui.ColorEdit4("Text color", ref textColor))
        {
            config.TextColor = textColor;
            config.Save();
        }

        var outline = config.TextOutline;
        if (ImGui.Checkbox("Text outline", ref outline))
        {
            config.TextOutline = outline;
            config.Save();
        }

        ImGui.Separator();
        ImGui.Text("Layout");

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

    private void DrawBossSettings()
    {
        var onlyBosses = config.OnlyBosses;
        if (ImGui.Checkbox("Only show on bosses (Striking Dummies count)", ref onlyBosses))
        {
            config.OnlyBosses = onlyBosses;
            config.Save();
        }

        if (!config.OnlyBosses)
        {
            return;
        }

        var minHp = config.BossMinHp;
        ImGui.SetNextItemWidth(160);
        if (ImGui.InputInt("Boss min max-HP", ref minHp, 100000, 1000000))
        {
            config.BossMinHp = minHp < 0 ? 0 : minHp;
            config.Save();
        }

        ImGui.TextDisabled("Enemies with at least this much max HP count as bosses; dummies always do.");

        var snap = tracker.Latest;
        if (snap.TargetId != 0)
        {
            ImGui.TextDisabled($"Current target: {snap.TargetName}, max HP {snap.TargetMaxHp:N0}, boss: {(snap.IsBoss ? "yes" : "no")}");
        }
    }

    private void DrawNativeBarSettings()
    {
        var hide = config.HideNativeBar;
        if (ImGui.Checkbox("Hide these statuses on the game's target bar", ref hide))
        {
            config.HideNativeBar = hide;
            config.Save();
        }

        ImGui.TextDisabled("Only the statuses this overlay shows, and only ones you applied yourself.");

        if (!config.HideNativeBar)
        {
            return;
        }

        var closeGaps = config.CloseGaps;
        if (ImGui.Checkbox("Close the gaps they leave", ref closeGaps))
        {
            config.CloseGaps = closeGaps;
            config.Save();
        }

        if (barHider.BigPlayerDebuffsLoaded)
        {
            ImGui.TextWrapped("BigPlayerDebuffs is loaded, so gaps are left open to avoid the two plugins fighting over icon positions. Since your own debuffs are hidden here, you can turn BigPlayerDebuffs off.");
        }
    }

    private void DrawDotSettings()
    {
        if (ImGui.CollapsingHeader("DoT jobs (tick the ones you want to see)"))
        {
            var i = 0;
            foreach (var group in DotList.Groups)
            {
                if (i % 4 != 0)
                {
                    ImGui.SameLine();
                }

                var enabled = config.IsJobEnabled(group.Job);
                if (ImGui.Checkbox($"{group.Job}##job{group.Job}", ref enabled))
                {
                    config.SetJobEnabled(group.Job, enabled);
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(string.Join(", ", group.Names));
                }

                i++;
            }
        }

        if (ImGui.CollapsingHeader("Extra status names"))
        {
            ImGui.TextDisabled("Exact in-game status name, e.g. a DoT missing from the list.");

            ImGui.SetNextItemWidth(200);
            ImGui.InputText("##newStatusName", ref newName, 64);
            ImGui.SameLine();
            if (ImGui.Button("Add") && !string.IsNullOrWhiteSpace(newName))
            {
                config.CustomNames.Add(newName.Trim());
                newName = string.Empty;
                config.Save();
            }

            for (var i = 0; i < config.CustomNames.Count; i++)
            {
                if (ImGui.SmallButton($"X##removeName{i}"))
                {
                    config.CustomNames.RemoveAt(i);
                    config.Save();
                    break;
                }

                ImGui.SameLine();
                ImGui.Text(config.CustomNames[i]);
            }
        }
    }
}
