using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ECommons.DalamudServices;

namespace AetherLinkServer.Utility;
public static class ActionScheduler
{
    private static readonly ConcurrentDictionary<string, (Action action, int delaySeconds, DateTime scheduledTime)> 
        _scheduledActions = new();
    
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> 
        _cancellationTokens = new();
    
    public static void Dispose()
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
    public static void ScheduleAction(string name, Action action, int delaySeconds)
    {
        // Cancel existing if already scheduled
        if (_cancellationTokens.TryRemove(name, out var existingCts))
        {
            Svc.Log.Debug($"Action {name} has already been scheduled, replacing");
            existingCts.Cancel();
            existingCts.Dispose();
        }
        Svc.Log.Debug($"Scheduling action {name} with delay {delaySeconds} seconds");
        var cts = new CancellationTokenSource();
        _cancellationTokens[name] = cts;
        var scheduledTime = DateTime.UtcNow.AddSeconds(delaySeconds);
        _scheduledActions[name] = (action, delaySeconds, scheduledTime);

        _ = RunScheduledAction(name, delaySeconds, cts.Token);
    }

    private static async Task RunScheduledAction(string name, int delaySeconds, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delaySeconds * 1000, ct);
            if (!ct.IsCancellationRequested && _scheduledActions.TryGetValue(name, out var actionInfo))
            {
                Svc.Log.Debug($"Executing action {name}");
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

    public static bool DelayAction(string name, int additionalDelaySeconds)
    {
        if (!_scheduledActions.TryGetValue(name, out var actionInfo) ||
            !_cancellationTokens.TryGetValue(name, out var cts))
        {
            return false;
        }
        Svc.Log.Debug($"Delaying action {name} by {additionalDelaySeconds} seconds");
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

    public static bool CancelAction(string name)
    {
        if (_cancellationTokens.TryRemove(name, out var cts))
        {
            Svc.Log.Debug($"Cancelling action {name}");
            cts.Cancel();
            cts.Dispose();
            _scheduledActions.TryRemove(name, out _);
            return true;
        }
        return false;
    }

    public static void ClearAll()
    {
        Svc.Log.Debug("Clearing all scheduled actions");
        foreach (var cts in _cancellationTokens.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _cancellationTokens.Clear();
        _scheduledActions.Clear();
    }

    public static bool IsActionScheduled(string name)
    {
        return _scheduledActions.ContainsKey(name);
    }

    public static double GetRemainingSeconds(string name)
    {
        if (_scheduledActions.TryGetValue(name, out var scheduledItem))
        {
            var remaining = (scheduledItem.scheduledTime - DateTime.UtcNow).TotalSeconds;
            return Math.Max(0, remaining);
        }
        return -1; // Indicates action not found
    }
}
