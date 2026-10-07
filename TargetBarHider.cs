using System;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TargetDebuffs;

// Hides the statuses the overlay is showing from the game's own target status bar.
//
// Layout (same one BigPlayerDebuffs relies on): the target's 30 status slots are image nodes in the addon's node list.
//   Split target frame  "_TargetInfoBuffDebuff": slot k = NodeList[31 - k]
//   Merged target frame "_TargetInfo":            slot k = NodeList[32 - k]
// Slots 0-14 are the first row (25px apart), 15-29 the second row.
// The game places statuses applied by you first, so your own statuses occupy the first slots.
//
// The game re-shows icons whenever it refreshes them (for example as timers tick), so the hiding is applied in the
// addon's PreDraw event: after the game has updated the bar, immediately before it is drawn. That avoids flicker.
public sealed unsafe class TargetBarHider : IDisposable
{
    private const int MaxSlots = 30;
    private const float SlotWidth = 25f;
    private const int RowLength = 15;

    private static readonly (string Name, int BaseIndex, int MinNodes)[] Addons =
    {
        ("_TargetInfoBuffDebuff", 31, 32),
        ("_TargetInfo", 32, 53),
    };

    private readonly Configuration config;
    private readonly StatusTracker tracker;
    private readonly IGameGui gameGui;
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly IPluginLog log;

    private readonly bool[][] hidden = { new bool[MaxSlots], new bool[MaxSlots] };
    private readonly bool[] moved = new bool[2];
    private int frame;
    private bool errorLogged;

    public TargetBarHider(
        Configuration config,
        StatusTracker tracker,
        IGameGui gameGui,
        IAddonLifecycle addonLifecycle,
        IDalamudPluginInterface pluginInterface,
        IPluginLog log)
    {
        this.config = config;
        this.tracker = tracker;
        this.gameGui = gameGui;
        this.addonLifecycle = addonLifecycle;
        this.pluginInterface = pluginInterface;
        this.log = log;

        addonLifecycle.RegisterListener(
            AddonEvent.PreDraw,
            Addons.Select(a => a.Name).ToArray(),
            OnPreDraw);
    }

    public bool BigPlayerDebuffsLoaded { get; private set; }

    // Called once per frame from the framework update; only refreshes the BigPlayerDebuffs check.
    public void Update()
    {
        if (frame++ % 120 != 0)
        {
            return;
        }

        try
        {
            BigPlayerDebuffsLoaded = pluginInterface.InstalledPlugins
                .Any(p => p.InternalName == "BigPlayerDebuffs" && p.IsLoaded);
        }
        catch (Exception ex)
        {
            LogOnce(ex, "Failed to check for BigPlayerDebuffs");
        }
    }

    private void OnPreDraw(AddonEvent type, AddonArgs args)
    {
        try
        {
            var a = Array.FindIndex(Addons, x => x.Name == args.AddonName);
            if (a < 0)
            {
                return;
            }

            ApplyToAddon(a, restoreOnly: false);
        }
        catch (Exception ex)
        {
            LogOnce(ex, "Failed to update the target status bar");
        }
    }

    private void ApplyToAddon(int a, bool restoreOnly)
    {
        var unit = (AtkUnitBase*)gameGui.GetAddonByName(Addons[a].Name, 1).Address;
        if (unit == null || unit->UldManager.NodeList == null || unit->UldManager.NodeListCount < Addons[a].MinNodes)
        {
            return;
        }

        var snap = tracker.Latest;
        var want = new bool[MaxSlots];

        if (config.HideNativeBar && !restoreOnly)
        {
            for (var k = 0; k < snap.OwnHideFlags.Count && k < MaxSlots; k++)
            {
                want[k] = snap.OwnHideFlags[k];
            }
        }

        // Closing gaps moves icons around, which would fight BigPlayerDebuffs (it also repositions them).
        var compact = config.HideNativeBar && config.CloseGaps && !BigPlayerDebuffsLoaded && !restoreOnly;

        var nodes = unit->UldManager.NodeList;
        var hid = hidden[a];
        var column = new int[2];

        for (var k = 0; k < MaxSlots; k++)
        {
            var node = nodes[Addons[a].BaseIndex - k];
            if (node == null)
            {
                continue;
            }

            var occupied = k < snap.Total;

            if (want[k] && occupied)
            {
                node->ToggleVisibility(false);
                hid[k] = true;
                continue;
            }

            if (hid[k])
            {
                // Only show it again if a status really sits in this slot; empty slots must stay hidden.
                if (occupied)
                {
                    node->ToggleVisibility(true);
                }

                hid[k] = false;
            }

            if (!occupied)
            {
                continue;
            }

            var row = k / RowLength;
            float? x = null;
            if (compact)
            {
                x = column[row] * SlotWidth;
                column[row]++;
                moved[a] = true;
            }
            else if (moved[a])
            {
                x = (k % RowLength) * SlotWidth;
            }

            if (x.HasValue && MathF.Abs(node->X - x.Value) > 0.01f)
            {
                node->X = x.Value;
                node->DrawFlags |= 0x1;
            }
        }

        if (!compact && moved[a])
        {
            moved[a] = false;
        }
    }

    private void LogOnce(Exception ex, string message)
    {
        if (errorLogged)
        {
            return;
        }

        log.Error(ex, message);
        errorLogged = true;
    }

    public void Dispose()
    {
        addonLifecycle.UnregisterListener(OnPreDraw);

        try
        {
            // Put everything back the way the game had it.
            for (var a = 0; a < Addons.Length; a++)
            {
                ApplyToAddon(a, restoreOnly: true);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to restore the target status bar");
        }
    }
}
