using System;
using ActionState = AetherLinkServer.Data.Enums.ActionState;
using AetherLinkServer.IPC;
using Dalamud.Plugin.Services;
using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;

namespace AetherLinkServer.Handlers;

public class AutogatherHandler : HandlerBase, IDisposable
{
    private Plugin _plugin;
    
    public override string[] AddonsToClose => new string[0];
    public override bool CanBeInterrupted => true;

    public AutogatherHandler(IPluginLog logger, TaskManager taskManager, HandlerManager handlerManager)
        : base(logger, taskManager, handlerManager)
    {

    }

    public bool Invoke()
    {
        if (IsEnabled) return true;
        if (!GatherBuddyReborn_IPCSubscriber.IsEnabled)
        {
            Logger.Info("AutoRetainer requires the AutoRetainer plugin. Visit  for more info.");
            return false;
        }
        base.Enable();
        return true;
    }

    protected override void OnEnable()
    {
        TaskManager.Enqueue(() =>
        {
            if (!Player.IsBusy)
            {
                try
                {
                    GatherBuddyReborn_IPCSubscriber.SetAutoGatherEnabled(true);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    return false;
                }
            }

            return false;
        });
    }

    protected override void OnDisable()
    {
        GatherBuddyReborn_IPCSubscriber.SetAutoGatherEnabled(false);
        TaskManager.Abort();
    }

    public void Dispose()
    {
        
    }
}
