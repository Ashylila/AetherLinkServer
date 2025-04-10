using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;

namespace AetherLinkServer.Utility;
public class ActionScheduler(IPluginLog log) : IDisposable
{
    private readonly ConcurrentDictionary<string, (Action action, int delaySeconds, DateTime scheduledTime)> 
        _scheduledActions = new();
    
    private readonly ConcurrentDictionary<string, CancellationTokenSource> 
        _cancellationTokens = new();
    private readonly IPluginLog _log = log;
    
    public void Dispose()
    {

        try
        {
            // Cancel and dispose all cancellation tokens
            foreach (var cts in _cancellationTokens.Values)
            {
                try
                {
                    if (!cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }

                    cts.Dispose();
                }
                catch
                {
                    // Suppress disposal errors
                }
            }

            // Clear all collections
            _cancellationTokens.Clear();
            _scheduledActions.Clear();
        }
        catch
        {
            
        }
    }
    public void ScheduleAction(string name, Action action, int delaySeconds)
    {
        // Cancel existing if already scheduled
        if (_cancellationTokens.TryRemove(name, out var existingCts))
        {
            _log.Debug($"Action {name} has already been scheduled, replacing");
            existingCts.Cancel();
            existingCts.Dispose();
        }
        _log.Debug($"Scheduling action {name} with delay {delaySeconds} seconds");
        var cts = new CancellationTokenSource();
        _cancellationTokens[name] = cts;
        var scheduledTime = DateTime.UtcNow.AddSeconds(delaySeconds);
        _scheduledActions[name] = (action, delaySeconds, scheduledTime);

        _ = RunScheduledAction(name, delaySeconds, cts.Token);
    }

    private async Task RunScheduledAction(string name, int delaySeconds, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delaySeconds * 1000, ct);
            if (!ct.IsCancellationRequested && _scheduledActions.TryGetValue(name, out var actionInfo))
            {
                _log.Debug($"Executing action {name}");
                actionInfo.action();
                _scheduledActions.TryRemove(name, out _);
                _cancellationTokens.TryRemove(name, out _);
            }
        }
        catch (TaskCanceledException)
        {
            // Action was cancelled - ignore
        }
    }

    public bool DelayAction(string name, int additionalDelaySeconds)
    {
        if (!_scheduledActions.TryGetValue(name, out var actionInfo) ||
            !_cancellationTokens.TryGetValue(name, out var cts))
        {
            return false;
        }
        _log.Debug($"Delaying action {name} by {additionalDelaySeconds} seconds");
        // Cancel current execution
        cts.Cancel();
        cts.Dispose();

        // Calculate new delay
        var remaining = (actionInfo.scheduledTime - DateTime.UtcNow).TotalSeconds;
        var newDelay = Math.Max(0, remaining) + additionalDelaySeconds;

        // Reschedule with new delay
        var newCts = new CancellationTokenSource();
        _cancellationTokens[name] = newCts;
        var newScheduledTime = DateTime.UtcNow.AddSeconds(newDelay);
        _scheduledActions[name] = (actionInfo.action, (int)newDelay, newScheduledTime);

        _ = RunScheduledAction(name, (int)newDelay, newCts.Token);
        return true;
    }

    public bool CancelAction(string name)
    {
        if (_cancellationTokens.TryRemove(name, out var cts))
        {
            _log.Debug($"Cancelling action {name}");
            cts.Cancel();
            cts.Dispose();
            _scheduledActions.TryRemove(name, out _);
            return true;
        }
        return false;
    }

    public void ClearAll()
    {
        _log.Debug("Clearing all scheduled actions");
        foreach (var cts in _cancellationTokens.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _cancellationTokens.Clear();
        _scheduledActions.Clear();
    }

    public bool IsActionScheduled(string name)
    {
        return _scheduledActions.ContainsKey(name);
    }

    public double GetRemainingSeconds(string name)
    {
        if (_scheduledActions.TryGetValue(name, out var scheduledItem))
        {
            var remaining = (scheduledItem.scheduledTime - DateTime.UtcNow).TotalSeconds;
            return Math.Max(0, remaining);
        }
        return -1; // Indicates action not found
    }
}
