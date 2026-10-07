using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

namespace TargetDebuffs;

public readonly struct StatusEntry
{
    public StatusEntry(uint icon, float remaining, ushort stacks, string name)
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

public sealed class StatusSnapshot
{
    public static readonly StatusSnapshot Empty =
        new(new List<StatusEntry>(), new List<bool>(), 0, 0, 0, string.Empty, false);

    public StatusSnapshot(
        List<StatusEntry> shown,
        List<bool> ownHideFlags,
        int totalOnBar,
        uint targetId,
        uint targetMaxHp,
        string targetName,
        bool isBoss)
    {
        Shown = shown;
        OwnHideFlags = ownHideFlags;
        Total = totalOnBar;
        TargetId = targetId;
        TargetMaxHp = targetMaxHp;
        TargetName = targetName;
        IsBoss = isBoss;
    }

    // Statuses the overlay displays.
    public List<StatusEntry> Shown { get; }

    // One flag per status YOU applied (in status-list order): true = the overlay shows it.
    // The game lists a target's statuses with your own first, so this lines up with the first slots of its bar.
    public List<bool> OwnHideFlags { get; }

    // Number of statuses currently on the target that have an icon (slots in use on the game's bar).
    public int Total { get; }

    public uint TargetId { get; }
    public uint TargetMaxHp { get; }
    public string TargetName { get; }
    public bool IsBoss { get; }
}

// Works out, once per frame, which statuses the overlay shows and which of the game's bar slots to hide.
public sealed class StatusTracker
{
    private readonly Configuration config;
    private readonly ITargetManager targetManager;
    private readonly IObjectTable objectTable;

    public StatusTracker(Configuration config, ITargetManager targetManager, IObjectTable objectTable)
    {
        this.config = config;
        this.targetManager = targetManager;
        this.objectTable = objectTable;
    }

    public StatusSnapshot Latest { get; private set; } = StatusSnapshot.Empty;

    public void Update()
    {
        var me = objectTable.LocalPlayer;
        if (targetManager.Target is not IBattleChara target || me == null)
        {
            Latest = StatusSnapshot.Empty;
            return;
        }

        var shown = new List<StatusEntry>();
        var ownFlags = new List<bool>();
        var total = 0;

        var isBoss = IsBoss(target);
        var allowedNames = config.OnlyDoTs ? DotList.BuildSet(config) : null;
        var bossOk = !config.OnlyBosses || isBoss;

        foreach (var status in target.StatusList)
        {
            if (status.StatusId == 0 || !status.GameData.IsValid)
            {
                continue;
            }

            var row = status.GameData.Value;
            if (row.Icon == 0)
            {
                continue;
            }

            total++;

            var isOwn = status.SourceId == me.EntityId;
            var show = bossOk && ShouldShow(status, row, allowedNames, me.EntityId);

            if (isOwn)
            {
                ownFlags.Add(show);
            }

            if (!show)
            {
                continue;
            }

            var icon = row.Icon;
            var stacks = status.Param;

            // Stackable statuses use consecutive icon ids, one per stack count.
            if (row.MaxStacks > 1 && stacks > 1)
            {
                icon += (uint)(stacks - 1);
            }

            shown.Add(new StatusEntry(icon, status.RemainingTime, stacks, row.Name.ToString()));
        }

        Latest = new StatusSnapshot(
            shown, ownFlags, total, target.EntityId, target.MaxHp, target.Name.TextValue, isBoss);
    }

    private bool ShouldShow(
        Dalamud.Game.ClientState.Statuses.IStatus status,
        Lumina.Excel.Sheets.Status row,
        System.Collections.Generic.HashSet<string>? allowedNames,
        uint myEntityId)
    {
        if (config.OnlyMine)
        {
            var mine = status.SourceId == myEntityId
                       || (config.IncludePets && status.SourceObject != null && status.SourceObject.OwnerId == myEntityId);
            if (!mine)
            {
                return false;
            }
        }

        if (allowedNames != null)
        {
            return allowedNames.Contains(row.Name.ToString());
        }

        // StatusCategory 2 = detrimental status.
        return !config.OnlyDebuffs || row.StatusCategory == 2;
    }

    // There is no reliable "is boss" flag exposed to plugins, so this is a heuristic:
    // training dummies always count, and so does any hostile enemy whose max HP reaches the threshold.
    private bool IsBoss(IBattleChara target)
    {
        if (target.Name.TextValue.Contains("Dummy", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return target is IBattleNpc npc
               && npc.BattleNpcKind == BattleNpcSubKind.Enemy
               && target.MaxHp >= (uint)Math.Max(0, config.BossMinHp);
    }
}
