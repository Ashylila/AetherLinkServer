using System;
using System.Linq;
using AetherLinkServer.Data;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using ActionState = AetherLinkServer.Data.Enums.ActionState;
using AetherLinkServer.IPC;
using AetherLinkServer.Utility;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using TaskManager = ECommons.Automation.NeoTaskManager.TaskManager;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation;

namespace AetherLinkServer.Handlers;

public sealed class AutoRetainerHandler : HandlerBase, IDisposable
{
    private readonly Plugin _plugin;
    private readonly ICondition _condition;
    private readonly IClientState _clientState;
    private readonly IObjectTable _objectTable;
    private readonly ITargetManager _targetManager;
    private readonly IFramework _framework;
    private readonly Chat _chat;
    private readonly ActionScheduler _actionScheduler;

    private bool _autoretainerRunning;

    public override string[] AddonsToClose => new[] { "RetainerList", "SelectYesno", "SelectString", "RetainerTaskAsk" };
    public override bool CanBeInterrupted => false;

    public AutoRetainerHandler(
        Plugin plugin,
        Chat chat,
        ICondition condition,
        IFramework framework,
        IPluginLog logger,
        IClientState state,
        ITargetManager target,
        IObjectTable objects,
        TaskManager taskManager,
        HandlerManager handlerManager,
        ActionScheduler actionScheduler
    ) : base(logger, taskManager, handlerManager)
    {
        _plugin = plugin;
        _chat = chat;
        _condition = condition;
        _framework = framework;
        _clientState = state;
        _targetManager = target;
        _objectTable = objects;
        _actionScheduler = actionScheduler;
    }

    public void Dispose()
    {
        _framework.Update -= OnUpdate;
        _framework.Update -= HandleRetainerExit;
    }

    public long? ClosestRetainerVentureSecondsRemaining =>
        AutoRetainer_IPCSubscriber.GetClosestRetainerVentureSecondsRemaining(_clientState.LocalContentId);

    public void Invoke()
    {
        if (!AutoRetainer_IPCSubscriber.IsEnabled)
        {
            Logger.Info("AutoRetainer requires the AutoRetainer plugin.");
            return;
        }

        var secondsRemaining = ClosestRetainerVentureSecondsRemaining;
        if (secondsRemaining is > 0)
        {
            Logger.Debug($"Scheduling AutoRetainer in {secondsRemaining} seconds...");
            _actionScheduler.ScheduleAction(nameof(Enable), ()=>TaskManager.Enqueue(() =>
            {
                var result = HandlerManager.TryInterruptRunningHandlers();
                if (result.result)
                {
                    Enable();
                    return true;
                }
                Logger.Debug($"{result.reason}");   
                return false;
            },nameof(Enable)), (int)secondsRemaining);
        }
        else
        {
            TaskManager.Enqueue(() =>
            {
                var result = HandlerManager.TryInterruptRunningHandlers();
                if (result.result)
                {
                    Enable();
                    return true;
                }
                Logger.Debug($"{result.reason}");
                return false;
            },nameof(Enable));
            _actionScheduler.CancelAction(nameof(Enable));
        }
    }
    
    protected override void OnEnable()
    {
        if (!AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
        {
            Logger.Debug("No available retainers.");
            Disable();
            return;
        }

        Logger.Debug("AutoRetainer enabled.");
        _framework.Update += OnUpdate;
    }

    protected override void OnDisable()
    {
        TaskManager.Abort();
        _autoretainerRunning = false;
        _framework.Update -= OnUpdate;
        _framework.Update += HandleRetainerExit;
    }

    private void OnUpdate(IFramework framework)
    {
        if (!IsEnabled) return;

        if (!_autoretainerRunning && !AutoRetainer_IPCSubscriber.IsBusy())
        {
            _autoretainerRunning = true;
            StartTasks();
        }
        else if (_autoretainerRunning && !AutoRetainer_IPCSubscriber.IsBusy() && !AutoRetainer_IPCSubscriber.AreAnyRetainersAvailableForCurrentChara())
        {
            Logger.Info("AutoRetainer finished.");
            Disable();
        }
    }

    private unsafe void StartTasks()
    {
        TaskManager.Enqueue(() =>
        {
            if (_clientState.LocalPlayer == null) return false;

            if (Player.Territory != 628 && _plugin.state != ActionState.Teleporting && !Player.IsBusy)
            {
                if (TeleportHelper.TryFindAetheryteByName("Kugane", out var kugane, out _))
                {
                    Logger.Debug("Teleporting to Kugane...");
                    TeleportHelper.Teleport(kugane.AetheryteId, kugane.SubIndex);
                    _plugin.state = ActionState.Teleporting;
                }
                return false;
            }

            return true;
        }, new TaskManagerConfiguration(timeLimitMS:1000000));

        TaskManager.Enqueue(() =>
        {
            if (Player.Territory == 628 && !Player.IsBusy)
            {
                Logger.Debug("Arrived at Kugane, moving to Summoning Bell...");
                PlayerMovement.Move(SummoningBell.SummoningBellVector3s(628));
                _plugin.state = ActionState.MovingToBell;
                return true;
            }
            return false;
        });

        TaskManager.Enqueue(() =>
        {
            var bellPos = SummoningBell.SummoningBellVector3s(628);
            if (PlayerHelper.GetDistanceToPlayer(bellPos) <= 4 && summoningBell != null)
            {
                if (!_condition[ConditionFlag.OccupiedSummoningBell])
                {
                    Logger.Debug("Interacting with Summoning Bell...");
                    _chat.ExecuteCommand("/autoretainer e");
                    _targetManager.Target = summoningBell;
                    TargetSystem.Instance()->OpenObjectInteraction(TargetSystem.Instance()->Target);
                    _plugin.state = ActionState.InteractingWithBell;
                }
                return true;
            }
            return false;
        });

        TaskManager.Enqueue(() =>
        {
            if (AutoRetainer_IPCSubscriber.IsBusy())
            {
                Logger.Debug("AutoRetainer is now working.");
                return false;
            }
            return true;
        });

        TaskManager.Enqueue(() =>
        {
            Logger.Debug("AutoRetainer finished work, closing addons.");
            CloseAddons();
            _plugin.state = ActionState.None;
            return true;
        });
    }

    private void HandleRetainerExit(IFramework framework)
    {
        if (!_condition[ConditionFlag.OccupiedSummoningBell])
        {
            _plugin.state = ActionState.None;
            _framework.Update -= HandleRetainerExit;
            Invoke();
        }
        else
        {
            Logger.Debug("Closing remaining windows...");
            CloseAddons();
        }
    }

    private IGameObject? summoningBell =>
        _objectTable.FirstOrDefault(o => o.DataId == SummoningBell.SummoningBellDataIds(628));
}
