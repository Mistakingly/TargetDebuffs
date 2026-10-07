using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace TargetDebuffs.Windows;

public sealed class DebuffWindow : Window
{
    private readonly Configuration config;
    private readonly StatusTracker tracker;
    private readonly ITextureProvider textureProvider;

    public DebuffWindow(Configuration config, StatusTracker tracker, ITextureProvider textureProvider)
        : base("Target Debuffs##TargetDebuffsOverlay")
    {
        this.config = config;
        this.tracker = tracker;
        this.textureProvider = textureProvider;

        IsOpen = true;
        RespectCloseHotkey = false;
        Size = new Vector2(200, 80);
        SizeCondition = ImGuiCond.FirstUseEver;
        Position = new Vector2(100, 100);
        PositionCondition = ImGuiCond.FirstUseEver;
    }

    public override void PreDraw()
    {
        // The window position is remembered between sessions by ImGui, so dragging it once is enough.
        Flags = ImGuiWindowFlags.NoTitleBar
              | ImGuiWindowFlags.NoScrollbar
              | ImGuiWindowFlags.NoScrollWithMouse
              | ImGuiWindowFlags.NoCollapse
              | ImGuiWindowFlags.NoFocusOnAppearing
              | ImGuiWindowFlags.NoNav
              | ImGuiWindowFlags.AlwaysAutoResize;

        if (config.Locked)
        {
            // Locked: invisible frame, cannot be moved, clicks pass through to the game.
            Flags |= ImGuiWindowFlags.NoMove
                   | ImGuiWindowFlags.NoBackground
                   | ImGuiWindowFlags.NoInputs;
        }
    }

    public override void Draw()
    {
        var entries = tracker.Latest.Shown;

        if (!config.Locked)
        {
            ImGui.TextDisabled("Target Debuffs - drag to move, lock in settings (/tdebuffs)");
        }

        if (entries.Count == 0)
        {
            if (!config.Locked)
            {
                ImGui.TextDisabled("(nothing to show - target something you've debuffed)");
            }
            return;
        }

        var iconSize = new Vector2(24f, 32f) * config.IconScale;
        var perRow = config.IconsPerRow < 1 ? 1 : config.IconsPerRow;

        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (i % perRow != 0)
            {
                ImGui.SameLine(0, config.Spacing);
            }

            var tex = textureProvider.GetFromGameIcon(new GameIconLookup(e.Icon)).GetWrapOrEmpty();
            ImGui.Image(tex.Handle, iconSize);

            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var drawList = ImGui.GetWindowDrawList();

            if (config.ShowTimers && e.Remaining > 0f)
            {
                var text = e.Remaining >= 10f ? ((int)e.Remaining).ToString() : e.Remaining.ToString("0.0");
                DrawText(drawList, text, new Vector2((min.X + max.X) / 2f, max.Y - 2f), bottomCenter: true);
            }

            if (config.ShowStacks && e.Stacks > 1)
            {
                DrawText(drawList, e.Stacks.ToString(), new Vector2(max.X - 2f, min.Y + 1f), topRight: true);
            }

            if (!config.Locked && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(e.Name);
            }
        }
    }

    private void DrawText(
        ImDrawListPtr drawList,
        string text,
        Vector2 anchor,
        bool bottomCenter = false,
        bool topRight = false)
    {
        var scale = config.TextScale;
        var font = ImGui.GetFont();
        var fontSize = ImGui.GetFontSize() * scale;
        var size = ImGui.CalcTextSize(text) * scale;
        var pos = anchor;

        if (bottomCenter)
        {
            pos = new Vector2(anchor.X - size.X / 2f, anchor.Y - size.Y);
        }
        else if (topRight)
        {
            pos = new Vector2(anchor.X - size.X, anchor.Y);
        }

        if (config.TextOutline)
        {
            var shadow = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 1f));
            drawList.AddText(font, fontSize, pos + new Vector2(1f, 1f), shadow, text);
            drawList.AddText(font, fontSize, pos + new Vector2(-1f, -1f), shadow, text);
            drawList.AddText(font, fontSize, pos + new Vector2(1f, -1f), shadow, text);
            drawList.AddText(font, fontSize, pos + new Vector2(-1f, 1f), shadow, text);
        }

        drawList.AddText(font, fontSize, pos, ImGui.GetColorU32(config.TextColor), text);
    }
}
