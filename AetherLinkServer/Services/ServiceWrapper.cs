using System;
using System.Net.WebSockets;
using AetherLinkServer.Commands;
using AetherLinkServer.Handlers;
using AetherLinkServer.Models;
using AetherLinkServer.Utility;
using AetherLinkServer.Windows;
using Dalamud.Plugin;
using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Microsoft.Extensions.DependencyInjection;

namespace AetherLinkServer.Services;

public static class ServiceWrapper
{
    public static IServiceProvider ServiceProvider;
    
    public static void Initialize(Plugin plugin, IDalamudPluginInterface pi)
    {
        IServiceCollection _serviceCollection = new ServiceCollection();
        Svc.Init(pi);

        _serviceCollection.AddSingleton(Svc.Chat);
        _serviceCollection.AddSingleton(Svc.Log);
        _serviceCollection.AddSingleton(Svc.PluginInterface);
        _serviceCollection.AddSingleton(Svc.Framework);
        _serviceCollection.AddSingleton(Svc.Condition);
        _serviceCollection.AddSingleton(Svc.GameGui);
        _serviceCollection.AddSingleton(Svc.Targets);
        _serviceCollection.AddSingleton(Svc.ClientState);
        _serviceCollection.AddSingleton(Svc.Objects);
        _serviceCollection.AddSingleton(Svc.Data);
        
        _serviceCollection.AddSingleton(plugin);
        _serviceCollection.AddSingleton(plugin.Configuration);
        
        _serviceCollection.AddSingleton<ClientWebSocket>();
        _serviceCollection.AddSingleton<TaskManager>();
        _serviceCollection.AddSingleton<Chat>();
        
        _serviceCollection.AddSingleton<CommandHandler>();
        _serviceCollection.AddSingleton<WebSocketServer>();
        _serviceCollection.AddSingleton<ChatHandler>();
        _serviceCollection.AddSingleton<ActionScheduler>();
        _serviceCollection.AddSingleton<AutoRetainerHandler>();

        _serviceCollection.AddSingleton<AutoretainerCommand>();
        _serviceCollection.AddSingleton<TeleportCommand>();
        _serviceCollection.AddSingleton<GetCurrentTargetCommand>();
        
        _serviceCollection.AddSingleton<MainWindow>();
        
        _serviceCollection.AddSingleton<TaskManager>();
        
        ServiceProvider = _serviceCollection.BuildServiceProvider();
        
    }

    public static T Get<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();
    public static object Get(Type type) => ServiceProvider.GetRequiredService(type);
}
