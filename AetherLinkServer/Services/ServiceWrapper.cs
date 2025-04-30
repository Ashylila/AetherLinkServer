using System;
using System.Net.WebSockets;
using AetherLinkServer.Commands;
using AetherLinkServer.Handlers;
using AetherLinkServer.Models;
using AetherLinkServer.Utility;
using AetherLinkServer.Windows;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin;
using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Microsoft.Extensions.DependencyInjection;
using Dalamud.Plugin.Services;

namespace AetherLinkServer.Services;

public static class ServiceWrapper
{
    public static IServiceProvider ServiceProvider;
    
    public static void Initialize(Plugin plugin, IDalamudPluginInterface pi)
{
        IServiceCollection services = new ServiceCollection();
        Svc.Init(pi);

        
        services.AddSingleton(Svc.Chat);
        services.AddSingleton(Svc.Log);
        services.AddSingleton(Svc.PluginInterface);
        services.AddSingleton(Svc.Framework);
        services.AddSingleton(Svc.Condition);
        services.AddSingleton(Svc.GameGui);
        services.AddSingleton(Svc.Targets);
        services.AddSingleton(Svc.ClientState);
        services.AddSingleton(Svc.Objects);
        services.AddSingleton(Svc.Data);

        
        services.AddSingleton(plugin);
        services.AddSingleton(plugin.Configuration);

        
        services.AddSingleton<ClientWebSocket>();
        
        services.AddTransient<TaskManager>();
        
        services.AddSingleton<Chat>();
        services.AddSingleton<ActionScheduler>();
        services.AddSingleton<ChatHandler>();

        
        services.AddSingleton<HandlerManager>();

        
        services.AddTransient<AutoRetainerHandler>();
        services.AddTransient<HandlerBase>(dp => dp.GetRequiredService<AutoRetainerHandler>());
        services.AddTransient<AutogatherHandler>();
        services.AddTransient<HandlerBase>(dp => dp.GetRequiredService<AutogatherHandler>());

        
        services.AddSingleton<AutoretainerCommand>();
        services.AddSingleton<TeleportCommand>();
        services.AddSingleton<GetCurrentTargetCommand>();
        services.AddSingleton<AutogatherCommand>();

        
        services.AddSingleton<MainWindow>();

        
        services.AddSingleton<WebSocketServer>();
        services.AddSingleton<CommandHandler>();
        
        ServiceProvider = services.BuildServiceProvider();

    
        var handlerManager = ServiceProvider.GetRequiredService<HandlerManager>();

        foreach (var handler in ServiceProvider
                     .GetServices<HandlerBase>())
        {
            handlerManager.Register(handler);
        }
}


    public static T Get<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();
    public static object Get(Type type) => ServiceProvider.GetRequiredService(type);
}
