using System;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using AetherLinkServer.Data;
using AetherLinkServer.IPC;
using AetherLinkServer.Utility;
using ECommons;
using ECommons.Automation;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager;
using TaskManager = ECommons.Automation.NeoTaskManager.TaskManager;

namespace AetherLinkServer.Handlers;

public sealed class AutoRetainerHandler(Chat chat, ICondition condition, Plugin plugin, ActionScheduler scheduler, IFramework framework, IPluginLog logger, IClientState state, ITargetManager target, IObjectTable objects, TaskManager taskmanager) : IDisposable
{
    private readonly IFramework _framework = framework;
    private readonly IPluginLog _logger = logger;
    private readonly IClientState _clientState = state;
    private readonly ITargetManager _targetManager = target;
    private readonly IObjectTable _objectTable = objects;
    private readonly ICondition _condition = condition;
    private readonly Chat _chat = chat;
    private readonly Plugin _plugin = plugin;
    private readonly ActionScheduler _actionScheduler = scheduler;
    
    
    private readonly string[] _addonsToClose = ["RetainerList", "SelectYesno", "SelectString", "RetainerTaskAsk"];
    
    private readonly TaskManager _taskManager = taskmanager;
    private bool _autoretainerRunning;

    public bool IsEnabled { get; private set; }

    public long? ClosestRetainerVentureSecondsRemaining =>
        AutoRetainer_IPCSubscriber.GetClosestRetainerVentureSecondsRemaining(_clientState.LocalContentId);

    public void Dispose()
    {
        _framework.Update -= OnUpdate;
    }

    public void Invoke()
    {
        if (!AutoRetainer_IPCSubscriber.IsEnabled)
        {
            _logger.Info("AutoRetainer requires the AutoRetainer plugin. Visit https://puni.sh/plugin/AutoRetainer for more info.");
            return;
        }

        var secondsRemaining = ClosestRetainerVentureSecondsRemaining;
        if (secondsRemaining is > 0)
        {
            _logger.Debug($"Scheduling AutoRetainer in {secondsRemaining} seconds...");
            _actionScheduler.ScheduleAction(nameof(Invoke), Enable, (int)secondsRemaining);
        }
        else
        {
            Enable();
            _actionScheduler.CancelAction(nameof(Invoke));
        }
    }

    public void Enable()
    {
        if (!AutoRetainer_IPCSubscriber.IsEnabled || !AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
            return;

        if (IsEnabled)
            return;

        _logger.Debug("AutoRetainer enabled.");
        IsEnabled = true;
        _framework.Update += OnUpdate;
    }

    private void OnUpdate(IFramework framework)
    {
        if (!IsEnabled)
            return;

        if (!_autoretainerRunning && !AutoRetainer_IPCSubscriber.IsBusy())
        {
            _autoretainerRunning = true;
            StartTasks();
        }
        else if (_autoretainerRunning && !AutoRetainer_IPCSubscriber.IsBusy() && !AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
        {
            _logger.Info("AutoRetainer finished.");
            Stop();
        }
    }

    private unsafe void StartTasks()
    {
        _taskManager.Enqueue(() =>
        {
            if (_clientState.LocalPlayer == null) return false;

            // Teleport to Kugane if not there
            if (Player.Territory != 628 && _plugin.state != Enums.ActionState.Teleporting)
            {
                if (TeleportHelper.TryFindAetheryteByName("Kugane", out var kugane, out _))
                {
                    _logger.Debug("Teleporting to Kugane...");
                    TeleportHelper.Teleport(kugane.AetheryteId, kugane.SubIndex);
                    _plugin.state = Enums.ActionState.Teleporting;
                }
                return false; // wait for teleport
            }

            return true;
        });

        _taskManager.Enqueue(() =>
        {
            // After teleport: wait for zone loaded
            if (Player.Territory == 628 && !Player.IsBusy)
            {
                _logger.Debug("Arrived at Kugane, moving to Summoning Bell...");
                PlayerMovement.Move(SummoningBell.SummoningBellVector3s(628));
                _plugin.state = Enums.ActionState.MovingToBell;
                return true;
            }
            return false;
        });

        _taskManager.Enqueue(() =>
        {
            // Walked to bell
            var bellPos = SummoningBell.SummoningBellVector3s(628);
            if (PlayerHelper.GetDistanceToPlayer(bellPos) <= 4 && summoningBell != null)
            {
                if (!_condition[ConditionFlag.OccupiedSummoningBell])
                {
                    _logger.Debug("Interacting with Summoning Bell...");
                    _chat.ExecuteCommand("/autoretainer e");
                    _targetManager.Target = summoningBell;
                    TargetSystem.Instance()->OpenObjectInteraction(TargetSystem.Instance()->Target);
                    _plugin.state = Enums.ActionState.InteractingWithBell;
                }
                return true;
            }
            return false;
        });

        _taskManager.Enqueue(() =>
        {
            // Wait until AutoRetainer work is done
            if (AutoRetainer_IPCSubscriber.IsBusy())
            {
                _logger.Debug("AutoRetainer is now working.");
                return false;
            }
            return true;
        });

        _taskManager.Enqueue(() =>
        {
            // Cleanup
            _logger.Debug("AutoRetainer finished work, closing addons.");
            CloseAddons();
            _plugin.state = Enums.ActionState.InteractingWithBell;
            return true;
        });
    }

    private void Stop()
    {
        _logger.Debug("Stopping AutoRetainerHandler...");
        IsEnabled = false;
        _autoretainerRunning = false;
        _framework.Update -= OnUpdate;

        _framework.Update += HandleRetainerExit;
    }

    private void HandleRetainerExit(IFramework framework)
    {
        if (!_condition[ConditionFlag.OccupiedSummoningBell])
        {
            _plugin.state = Enums.ActionState.None;
            _framework.Update -= HandleRetainerExit;
            Invoke(); // Check again if another venture is ready soon
        }
        else
        {
            _logger.Debug("Closing remaining windows...");
            CloseAddons();
        }
    }

    private unsafe bool CloseAddons()
    {
        foreach (var name in _addonsToClose)
        {
            if (GenericHelpers.TryGetAddonByName(name, out AtkUnitBase* addon) && addon->IsVisible)
            {
                _logger.Debug($"Closing addon: {name}");
                addon->Close(true);
                return false;
            }
        }
        return true;
    }

    private IGameObject? summoningBell =>
        _objectTable.FirstOrDefault(o => o.DataId == SummoningBell.SummoningBellDataIds(628));
}
