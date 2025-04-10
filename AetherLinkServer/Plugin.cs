using System;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Data;
using AetherLinkServer.Handlers;
using AetherLinkServer.IPC;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;
using AetherLinkServer.Windows;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using Serilog;

namespace AetherLinkServer;

#nullable disable
public sealed class Plugin : IDalamudPlugin
{
    
    public Enums.ActionState state = Enums.ActionState.None;
    private const string CommandName = "/aetherlinkserver";
    public readonly WindowSystem WindowSystem = new("AetherLinkServer");

    public Plugin()
    {
        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);
        ServiceWrapper.Initialize(this, PluginInterface);
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        MainWindow = new MainWindow(this);
        

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
        if (ServiceWrapper.ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        WindowSystem.RemoveAllWindows();
        server.Dispose();
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
