using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using AetherLinkServer.Data;
using AetherLinkServer.IPC;
using AetherLinkServer.Utility;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace AetherLinkServer.Handlers;

public class AutoRetainerHandler : IDisposable
{
    private IGameObject? summoningBell;
    private IFramework Framework => Svc.Framework;
    public bool IsEnabled { get; private set; } = false;
    private bool _autoretainerEnabled = false;
    
    public void Dispose()
    {
        
    }

    internal static unsafe void CloseRetainerWindows()
    {
        if (Svc.Targets.Target != null)
            Svc.Targets.Target = null;
    }

    public void Enable()
    {
        summoningBell = Svc.Objects.FirstOrDefault(x => x.DataId == 2000441);
        if (!AutoRetainer_IPCSubscriber.IsEnabled ||
            !AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
            return;
        if (!AutoRetainer_IPCSubscriber.IsEnabled)
        {
            Svc.Log.Info("AutoRetainer requires a plugin, visit https://puni.sh/plugin/AutoRetainer for more info");
            return;
        }
        if (!IsEnabled)
        {
            IsEnabled = true;
            Svc.Log.Debug((summoningBell == null).ToString());
            Svc.Log.Debug("Enabling");
            Framework.Update += HandleAutoretainer;
        }
        
    }

    private void HandleAutoretainer(IFramework framework)
    {
        if (!IsEnabled) return;
        if (!_autoretainerEnabled && !AutoRetainer_IPCSubscriber.IsBusy())
        {
            Svc.Log.Debug("AutoRetainer enabled");
            _autoretainerEnabled = true;
        }
        /*if (_autoretainerEnabled && !AutoRetainer_IPCSubscriber.IsBusy() && !AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
        {
            _autoretainerEnabled = false;
            Svc.Log.Info("AutoRetainer finished");
            Stop();
            return;
        }*/
        if (Svc.ClientState.LocalPlayer == null) return;
        //TODO: Make Teleporting Dynamic
        if (Player.Territory != 628 && PlayerHelper.state != Enums.ActionState.Teleporting)
        {
            var Location = TeleportHelper.TryFindAetheryteByName("Kugane", out var kugane, out var name);
            TeleportHelper.Teleport(kugane.AetheryteId, kugane.SubIndex);
            Svc.Log.Debug("Teleporting...");
            PlayerHelper.state = Enums.ActionState.Teleporting;
        }
        if (summoningBell != null && Player.Territory == 628 && PlayerHelper.state != Enums.ActionState.Running)
        {
            Svc.Log.Debug(Player.IsBusy.ToString());
            if (!Player.IsBusy)
            {
                PlayerMovement.Move(summoningBell);
                Svc.Log.Debug("Moving to summoning bell..");
                PlayerHelper.state = Enums.ActionState.Running;
            }
        }
        else if (PlayerHelper.GetDistanceToPlayer(summoningBell) <= 4 && _autoretainerEnabled)
        {
            if (Svc.Condition[ConditionFlag.OccupiedSummoningBell])
            {
                if (VNavmesh_IPCSubscriber.Path_IsRunning())
                    VNavmesh_IPCSubscriber.Path_Stop();
                Svc.Log.Debug("Waiting for AutoRetainer to Start");
                Chat.Instance.ExecuteCommand("/autoretainer e");
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
