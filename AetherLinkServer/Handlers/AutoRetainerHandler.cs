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
using Dalamud.Game.ClientState.Objects;
using ECommons.GameHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;


namespace AetherLinkServer.Handlers;

public class AutoRetainerHandler(Chat chat, ICondition condition, Plugin plugin, ActionScheduler scheduler, IFramework framework, IPluginLog Logger, IClientState state, ITargetManager target, IObjectTable objects) : IDisposable
{
    private readonly IFramework _framework = framework;
    private readonly IPluginLog _logger = Logger;
    private readonly IClientState _clientState = state;
    private readonly ITargetManager _targetManager = target;
    private readonly IObjectTable _objectTable = objects;
    private readonly ICondition _condition = condition;
    private readonly Chat _chat = chat;

    private Plugin _plugin = plugin;
    
    private readonly ActionScheduler _actionScheduler = scheduler;
    public bool IsEnabled { get; private set; } = false;
    private bool _autoretainerEnabled = false;

    public long? ClosestRetainerVentureSecondsRemaining =>
        AutoRetainer_IPCSubscriber.GetClosestRetainerVentureSecondsRemaining(_clientState.LocalContentId);
    
    public void Dispose()
    {
        
    }

    internal unsafe void CloseRetainerWindows()
    {
        if (_targetManager.Target != null)
            _targetManager.Target = null;
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
            _logger.Debug("Enabling");
            _framework.Update += HandleAutoretainer;
        }
    }

    public void Invoke()
    {
        if (!AutoRetainer_IPCSubscriber.IsEnabled)
        {
            _logger.Info("AutoRetainer requires a plugin, visit https://puni.sh/plugin/AutoRetainer for more info");
            return;
        }
        if (ClosestRetainerVentureSecondsRemaining is not null && ClosestRetainerVentureSecondsRemaining > 0)
        {
            _actionScheduler.ScheduleAction("InvokeAutoretainer", Enable, (int)ClosestRetainerVentureSecondsRemaining);
        }
        else if (ClosestRetainerVentureSecondsRemaining is not null && ClosestRetainerVentureSecondsRemaining <= 0)
        {
            Enable();
            _actionScheduler.CancelAction("InvokeAutoretainer");
        }
    }
    private IGameObject? summoningBell => _objectTable.FirstOrDefault(o => o.DataId == SummoningBell.SummoningBellDataIds(628));
    private unsafe void HandleAutoretainer(IFramework framework)
    {
        if (!IsEnabled) return;
        if (!_autoretainerEnabled && !AutoRetainer_IPCSubscriber.IsBusy())
        {
            _logger.Debug("AutoRetainer enabled");
            _autoretainerEnabled = true;
        }
        if (_autoretainerEnabled && !AutoRetainer_IPCSubscriber.IsBusy() && !AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
        {
            _autoretainerEnabled = false;
            _logger.Info("AutoRetainer finished");
            Stop();
            return;
        }
        if (_clientState.LocalPlayer == null) return;
        //TODO: Make Teleporting and Summoning bell selection Dynamic
        if (Player.Territory != 628 && _plugin.state != Enums.ActionState.Teleporting)
        {
            var Location = TeleportHelper.TryFindAetheryteByName("Kugane", out var kugane, out var name);
            TeleportHelper.Teleport(kugane.AetheryteId, kugane.SubIndex);
            _logger.Debug("Teleporting...");
            _plugin.state = Enums.ActionState.Teleporting;
        }
        if (Player.Territory == 628 && _plugin.state != Enums.ActionState.Running)
        {
            _logger.Debug("Player is:" + Player.IsBusy.ToString());
            if (!Player.IsBusy)
            {
                PlayerMovement.Move(SummoningBell.SummoningBellVector3s(628));
                _logger.Debug("Moving to summoning bell..");
                _plugin.state = Enums.ActionState.Running;
            }
        }
        else if (PlayerHelper.GetDistanceToPlayer(SummoningBell.SummoningBellVector3s(628)) <= 4 && _autoretainerEnabled && summoningBell != null)
        {
            if (!_condition[ConditionFlag.OccupiedSummoningBell])
            {
                if (VNavmesh_IPCSubscriber.Path_IsRunning())
                    VNavmesh_IPCSubscriber.Path_Stop();
                _logger.Debug("Waiting for AutoRetainer to Start");
                _chat.ExecuteCommand("/autoretainer e");
                _targetManager.Target = summoningBell;
                TargetSystem.Instance()->OpenObjectInteraction(TargetSystem.Instance()->Target);
                
            }
            else
            {
                _logger.Debug("Interacting with bell");
            }

        }
    }

    private void HandleAutoretainerStop(IFramework framework)
    {
        if (!_condition[ConditionFlag.OccupiedSummoningBell])
        {
            PlayerHelper.state = Enums.ActionState.None;
            _framework.Update -= HandleAutoretainerStop;
            Invoke();
            
        }
        else
        {
            _logger.Debug("Closing windows");
            CloseRetainerWindows();
        }
    }

    private void Stop()
    {
        _logger.Debug("AutoRetainer finished");
        _framework.Update -= HandleAutoretainer;
        _framework.Update += HandleAutoretainerStop;
    }
    
    
    
}
