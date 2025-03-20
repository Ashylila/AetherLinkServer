using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using AetherLinkServer.Windows;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Services;
using ECommons;
using AetherLinkServer.IPC;
using AetherLinkServer.Utility;
using System.Linq;
using System.Numerics;

namespace AetherLinkServer;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    private const string CommandName = "/pmycommand";

    public static WebSocketServer server { get; private set; }
    private readonly CommandHandler _commandHandler;
    private readonly ChatHandler _chatHandler;
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);
        Svc.Init(PluginInterface);
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        MainWindow = new MainWindow(this);
        
        server = new WebSocketServer(this);
        _commandHandler = new CommandHandler(this);
        _chatHandler = new ChatHandler(this);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        //PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        server.Dispose();
        _chatHandler.Dispose();
        MainWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}
