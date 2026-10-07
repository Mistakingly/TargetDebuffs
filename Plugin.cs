using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using TargetDebuffs.Windows;

namespace TargetDebuffs;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/tdebuffs";

    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commandManager;
    private readonly WindowSystem windowSystem = new("TargetDebuffs");
    private readonly ConfigWindow configWindow;
    private readonly DebuffWindow debuffWindow;
    private readonly IFramework framework;
    private readonly StatusTracker tracker;
    private readonly TargetBarHider barHider;

    public Configuration Config { get; }

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        ITargetManager targetManager,
        IObjectTable objectTable,
        ITextureProvider textureProvider,
        IFramework framework,
        IGameGui gameGui,
        IAddonLifecycle addonLifecycle,
        IPluginLog log)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;

        Config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Initialize(pluginInterface);

        this.framework = framework;
        tracker = new StatusTracker(Config, targetManager, objectTable);
        barHider = new TargetBarHider(Config, tracker, gameGui, addonLifecycle, pluginInterface, log);

        configWindow = new ConfigWindow(Config, tracker, barHider);
        debuffWindow = new DebuffWindow(Config, tracker, textureProvider);

        windowSystem.AddWindow(configWindow);
        windowSystem.AddWindow(debuffWindow);

        commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "/tdebuffs: open settings. /tdebuffs lock | unlock | toggle: control the overlay lock.",
        });

        framework.Update += OnFrameworkUpdate;
        pluginInterface.UiBuilder.Draw += windowSystem.Draw;
        pluginInterface.UiBuilder.OpenConfigUi += configWindow.Toggle;
        pluginInterface.UiBuilder.OpenMainUi += configWindow.Toggle;
    }

    private void OnFrameworkUpdate(IFramework _)
    {
        tracker.Update();
        barHider.Update();
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "lock":
                Config.Locked = true;
                Config.Save();
                break;
            case "unlock":
                Config.Locked = false;
                Config.Save();
                break;
            case "toggle":
                Config.Locked = !Config.Locked;
                Config.Save();
                break;
            default:
                configWindow.Toggle();
                break;
        }
    }

    public void Dispose()
    {
        framework.Update -= OnFrameworkUpdate;
        barHider.Dispose();
        pluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        pluginInterface.UiBuilder.OpenConfigUi -= configWindow.Toggle;
        pluginInterface.UiBuilder.OpenMainUi -= configWindow.Toggle;

        windowSystem.RemoveAllWindows();
        commandManager.RemoveHandler(CommandName);
    }
}
