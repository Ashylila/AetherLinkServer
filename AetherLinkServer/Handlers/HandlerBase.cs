
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AetherLinkServer.Handlers;

public abstract class HandlerBase
{
    protected readonly IPluginLog Logger;
    protected readonly TaskManager TaskManager;
    protected readonly HandlerManager HandlerManager;

    public abstract string[] AddonsToClose { get; }
    public abstract bool CanBeInterrupted { get; }
    
    public bool IsEnabled { get; private set; }
    public string HandlerName => GetType().Name;

    protected HandlerBase(IPluginLog logger, TaskManager taskManager, HandlerManager handlerManager)
    {
        Logger = logger;
        TaskManager = taskManager;
        HandlerManager = handlerManager;
    }

    public void Enable()
    {
        if (IsEnabled) return;
        
        Logger.Debug($"{HandlerName} Enabling.");
        IsEnabled = true;
        OnEnable();
    }

    public void Disable()
    {
        if (!IsEnabled) return;
        
        Logger.Debug($"{HandlerName} Disabling.");
        IsEnabled = false;
        OnDisable();
    }
    
    protected abstract void OnEnable();
    protected abstract void OnDisable();

    public bool TryInterrupt()
    {
        if (CanBeInterrupted && IsEnabled)
        {
            Logger.Debug($"{HandlerName} Interrupting.");
            Disable();
            return true;
        }
        Logger.Debug($"{HandlerName} cannot be Interrupted.");
        return false;
    }
    
    public unsafe bool CloseAddons()
    {
        foreach (var name in AddonsToClose)
        {
            if (GenericHelpers.TryGetAddonByName(name, out AtkUnitBase* addon) && addon->IsVisible)
            {
                Logger.Debug($"Closing addon: {name}");
                addon->Close(true);
                return false;
            }
        }
        return true;
    }
}
