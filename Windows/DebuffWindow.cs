using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace TargetDebuffs.Windows;

public sealed class DebuffWindow : Window
{
    private readonly Configuration config;
    private readonly ITargetManager targetManager;
    private readonly IObjectTable objectTable;
    private readonly ITextureProvider textureProvider;

    private readonly struct Entry
    {
        public Entry(uint icon, float remaining, ushort stacks, string name)
        {
            Icon = icon;
            Remaining = remaining;
            Stacks = stacks;
            Name = name;
        }

        public uint Icon { get; }
        public float Remaining { get; }
        public ushort Stacks { get; }
        public string Name { get; }
    }

    public DebuffWindow(
        Configuration config,
        ITargetManager targetManager,
        IObjectTable objectTable,
        ITextureProvider textureProvider)
        : base("Target Debuffs##TargetDebuffsOverlay")
    {
        this.config = config;
        this.targetManager = targetManager;
        this.objectTable = objectTable;
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
        var allowed = config.OnlyDoTs ? DotList.BuildSet(config) : null;
        var entries = Collect(allowed);

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

    private List<Entry> Collect(HashSet<string>? allowed)
    {
        var list = new List<Entry>();

        if (targetManager.Target is not IBattleChara target)
        {
            return list;
        }

        var me = objectTable.LocalPlayer;
        if (me == null)
        {
            return list;
        }

        foreach (var status in target.StatusList)
        {
            if (status.StatusId == 0 || !status.GameData.IsValid)
            {
                continue;
            }

            var row = status.GameData.Value;

            if (config.OnlyMine && !IsMine(status.SourceId, status.SourceObject, me.EntityId))
            {
                continue;
            }

            if (allowed != null)
            {
                // DoT filter: match by status name.
                if (!allowed.Contains(row.Name.ToString()))
                {
                    continue;
                }
            }
            else if (config.OnlyDebuffs && row.StatusCategory != 2)
            {
                // StatusCategory 2 = detrimental status.
                continue;
            }

            var icon = row.Icon;
            var stacks = status.Param;

            // Stackable statuses use consecutive icon ids, one per stack count.
            if (row.MaxStacks > 1 && stacks > 1)
            {
                icon += (uint)(stacks - 1);
            }

            list.Add(new Entry(icon, status.RemainingTime, stacks, row.Name.ToString()));
        }

        return list;
    }

    private bool IsMine(uint sourceId, IGameObject? sourceObject, uint myEntityId)
    {
        if (sourceId == myEntityId)
        {
            return true;
        }

        return config.IncludePets && sourceObject != null && sourceObject.OwnerId == myEntityId;
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
