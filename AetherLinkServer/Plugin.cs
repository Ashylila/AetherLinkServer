using AetherLinkServer.DalamudServices;
using AetherLinkServer.Handlers;
using AetherLinkServer.IPC;
using AetherLinkServer.Services;
using AetherLinkServer.Windows;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;

namespace AetherLinkServer;

#nullable disable
public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/aetherlinkserver";
    private readonly ChatHandler _chatHandler;
    private readonly CommandHandler _commandHandler;
    public readonly AutoRetainerHandler _autoRetainerHandler;
    public readonly WindowSystem WindowSystem = new("AetherLinkServer");

    public Plugin()
    {
        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);
        Svc.Init(PluginInterface);
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        MainWindow = new MainWindow(this);
        
        _autoRetainerHandler = new();
        server = new WebSocketServer(this);
        _commandHandler = new CommandHandler(this);
        
        _chatHandler = new ChatHandler(this);
        

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle the main Window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        //PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    public static WebSocketServer server { get; set; }
    public Configuration Configuration { get; init; }
    private MainWindow MainWindow { get; init; }

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
        ToggleMainUI();
        var random = AutoRetainer_IPCSubscriber.IsBusy();
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void ToggleMainUI()
    {
        MainWindow.Toggle();
    }
}
