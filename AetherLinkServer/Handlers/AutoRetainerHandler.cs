using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using AetherLinkServer.Data;
using AetherLinkServer.IPC;
using AetherLinkServer.Utility;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation;
using AetherLinkServer.DalamudServices;
using ECommons.GameHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;


namespace AetherLinkServer.Handlers;

public class AutoRetainerHandler : IDisposable
{
    private IFramework Framework => Svc.Framework;
    public bool IsEnabled { get; private set; } = false;
    private bool _autoretainerEnabled = false;

    public long? ClosestRetainerVentureSecondsRemaining =>
        AutoRetainer_IPCSubscriber.GetClosestRetainerVentureSecondsRemaining(Svc.ClientState.LocalContentId);
    
    public void Dispose()
    {
        
    }

    internal static unsafe void CloseRetainerWindows()
    {
        if (Svc.Targets.Target != null)
            Svc.Targets.Target = null;
        else if (GenericHelpers.TryGetAddonByName("SelectYesno", out AtkUnitBase* addonSelectYesno))
            addonSelectYesno->Close(true);
        else if (GenericHelpers.TryGetAddonByName("SelectString", out AtkUnitBase* addonSelectString))
            addonSelectString->Close(true);
        else if (GenericHelpers.TryGetAddonByName("RetainerList", out AtkUnitBase* addonRetainerList))
            addonRetainerList->Close(true);
        else if (GenericHelpers.TryGetAddonByName("RetainerTaskAsk", out AtkUnitBase* addonRetainerSell))
            addonRetainerSell->Close(true);
    }

    public void Enable()
    { 
        if(!AutoRetainer_IPCSubscriber.IsEnabled ||
            !AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
            return; 

        if (!IsEnabled)
        {
            IsEnabled = true;
            Svc.Log.Debug("Enabling");
            Framework.Update += HandleAutoretainer;
        }
    }

    public void Invoke()
    {
        if (!AutoRetainer_IPCSubscriber.IsEnabled)
        {
            Svc.Log.Info("AutoRetainer requires a plugin, visit https://puni.sh/plugin/AutoRetainer for more info");
            return;
        }
        if (ClosestRetainerVentureSecondsRemaining is not null && ClosestRetainerVentureSecondsRemaining > 0)
        {
            ActionScheduler.ScheduleAction("InvokeAutoretainer", Enable, (int)ClosestRetainerVentureSecondsRemaining);
        }
        else if (ClosestRetainerVentureSecondsRemaining is not null && ClosestRetainerVentureSecondsRemaining <= 0)
        {
            Enable();
            ActionScheduler.CancelAction("InvokeAutoretainer");
        }
    }
    private IGameObject? summoningBell => Svc.Objects.FirstOrDefault(o => o.DataId == SummoningBell.SummoningBellDataIds(628));
    private unsafe void HandleAutoretainer(IFramework framework)
    {
        if (!IsEnabled) return;
        if (!_autoretainerEnabled && !AutoRetainer_IPCSubscriber.IsBusy())
        {
            Svc.Log.Debug("AutoRetainer enabled");
            _autoretainerEnabled = true;
        }
        if (_autoretainerEnabled && !AutoRetainer_IPCSubscriber.IsBusy() && !AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
        {
            _autoretainerEnabled = false;
            Svc.Log.Info("AutoRetainer finished");
            Stop();
            return;
        }
        if (Svc.ClientState.LocalPlayer == null) return;
        //TODO: Make Teleporting and Summoning bell selection Dynamic
        if (Player.Territory != 628 && PlayerHelper.state != Enums.ActionState.Teleporting)
        {
            var Location = TeleportHelper.TryFindAetheryteByName("Kugane", out var kugane, out var name);
            TeleportHelper.Teleport(kugane.AetheryteId, kugane.SubIndex);
            Svc.Log.Debug("Teleporting...");
            PlayerHelper.state = Enums.ActionState.Teleporting;
        }
        if (Player.Territory == 628 && PlayerHelper.state != Enums.ActionState.Running)
        {
            Svc.Log.Debug("Player is:" + Player.IsBusy.ToString());
            if (!Player.IsBusy)
            {
                PlayerMovement.Move(SummoningBell.SummoningBellVector3s(628));
                Svc.Log.Debug("Moving to summoning bell..");
                PlayerHelper.state = Enums.ActionState.Running;
            }
        }
        else if (PlayerHelper.GetDistanceToPlayer(SummoningBell.SummoningBellVector3s(628)) <= 4 && _autoretainerEnabled && summoningBell != null)
        {
            if (!Svc.Condition[ConditionFlag.OccupiedSummoningBell])
            {
                if (VNavmesh_IPCSubscriber.Path_IsRunning())
                    VNavmesh_IPCSubscriber.Path_Stop();
                Svc.Log.Debug("Waiting for AutoRetainer to Start");
                Chat.Instance.ExecuteCommand("/autoretainer e");
                Svc.Targets.Target = summoningBell;
                TargetSystem.Instance()->OpenObjectInteraction(TargetSystem.Instance()->Target);
                
            }
            else
            {
                Svc.Log.Debug("Interacting with bell");
            }

        }
    }

    private void HandleAutoretainerStop(IFramework framework)
    {
        if (!Svc.Condition[ConditionFlag.OccupiedSummoningBell])
        {
            PlayerHelper.state = Enums.ActionState.None;
            Framework.Update -= HandleAutoretainerStop;
            Invoke();
            
        }
        else
        {
            CloseRetainerWindows();
        }
    }

    private void Stop()
    {
        Svc.Log.Debug("AutoRetainer finished");
        Svc.Framework.Update -= HandleAutoretainer;
        Svc.Framework.Update += HandleAutoretainerStop;
    }
    
    
    
}
