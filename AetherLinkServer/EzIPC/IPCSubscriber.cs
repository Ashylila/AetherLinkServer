#nullable disable warnings

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ECommons.DalamudServices;
using ECommons.EzIpcManager;
using ECommons.Reflection;

namespace AetherLinkServer.IPC;

internal static class AutoRetainer_IPCSubscriber
{
    private static readonly EzIPCDisposalToken[] _disposalTokens =
        EzIPC.Init(typeof(AutoRetainer_IPCSubscriber), "AutoRetainer.PluginState", SafeWrapper.IPCException);

    [EzIPC]
    internal static readonly Func<bool> IsBusy;

    [EzIPC]
    internal static readonly Func<ulong ,long?> GetClosestRetainerVentureSecondsRemaining;

    [EzIPC]
    internal static readonly Func<Dictionary<ulong, HashSet<string>>> GetEnabledRetainers;

    [EzIPC]
    internal static readonly Func<bool> AreAnyRetainersAvailableForCurrentChara;

    [EzIPC]
    internal static readonly Action AbortAllTasks;

    [EzIPC]
    internal static readonly Action DisableAllFunctions;

    [EzIPC]
    internal static readonly Action EnableMultiMode;

    [EzIPC]
    internal static readonly Func<int> GetInventoryFreeSlotCount;

    [EzIPC]
    internal static readonly Action EnqueueHET;

    internal static bool IsEnabled => IPCSubscriber_Common.IsReady("AutoRetainer");

    internal static void Dispose()
    {
        IPCSubscriber_Common.DisposeAll(_disposalTokens);
    }
}

internal static class VNavmesh_IPCSubscriber
{
    private static readonly EzIPCDisposalToken[] _disposalTokens =
        EzIPC.Init(typeof(VNavmesh_IPCSubscriber), "vnavmesh", SafeWrapper.IPCException);

    [EzIPC("Nav.IsReady")]
    internal static readonly Func<bool> Nav_IsReady;

    [EzIPC("Nav.BuildProgress")]
    internal static readonly Func<float> Nav_BuildProgress;

    [EzIPC("Nav.Reload")]
    internal static readonly Func<bool> Nav_Reload;

    [EzIPC("Nav.Rebuild")]
    internal static readonly Func<bool> Nav_Rebuild;

    [EzIPC("Nav.Pathfind")]
    internal static readonly Func<Vector3, Vector3, bool, Task<List<Vector3>>> Nav_Pathfind;

    [EzIPC("Nav.PathfindCancelable")]
    internal static readonly Func<Vector3, Vector3, bool, CancellationToken, Task<List<Vector3>>>
        Nav_PathfindCancelable;

    [EzIPC("Nav.PathfindCancelAll")]
    internal static readonly Action Nav_PathfindCancelAll;

    [EzIPC("Nav.PathfindInProgress")]
    internal static readonly Func<bool> Nav_PathfindInProgress;

    [EzIPC("Nav.PathfindNumQueued")]
    internal static readonly Func<int> Nav_PathfindNumQueued;

    [EzIPC("Nav.IsAutoLoad")]
    internal static readonly Func<bool> Nav_IsAutoLoad;

    [EzIPC("Nav.SetAutoLoad")]
    internal static readonly Action<bool> Nav_SetAutoLoad;

    [EzIPC("Query.Mesh.NearestPoint")]
    internal static readonly Func<Vector3, float, float, Vector3> Query_Mesh_NearestPoint;

    [EzIPC("Query.Mesh.PointOnFloor")]
    internal static readonly Func<Vector3, bool, float, Vector3> Query_Mesh_PointOnFloor;

    [EzIPC("Path.MoveTo")]
    internal static readonly Action<List<Vector3>, bool> Path_MoveTo;

    [EzIPC("Path.Stop")]
    internal static readonly Action Path_Stop;

    [EzIPC("Path.IsRunning")]
    internal static readonly Func<bool> Path_IsRunning;

    [EzIPC("Path.NumWaypoints")]
    internal static readonly Func<int> Path_NumWaypoints;

    [EzIPC("Path.GetMovementAllowed")]
    internal static readonly Func<bool> Path_GetMovementAllowed;

    [EzIPC("Path.SetMovementAllowed")]
    internal static readonly Action<bool> Path_SetMovementAllowed;

    [EzIPC("Path.GetAlignCamera")]
    internal static readonly Func<bool> Path_GetAlignCamera;

    [EzIPC("Path.SetAlignCamera")]
    internal static readonly Action<bool> Path_SetAlignCamera;

    [EzIPC("Path.GetTolerance")]
    internal static readonly Func<float> Path_GetTolerance;

    [EzIPC("Path.SetTolerance")]
    internal static readonly Action<float> Path_SetTolerance;

    [EzIPC("SimpleMove.PathfindAndMoveTo")]
    internal static readonly Func<Vector3, bool, bool> SimpleMove_PathfindAndMoveTo;

    [EzIPC("SimpleMove.PathfindInProgress")]
    internal static readonly Func<bool> SimpleMove_PathfindInProgress;

    [EzIPC("Window.IsOpen")]
    internal static readonly Func<bool> Window_IsOpen;

    [EzIPC("Window.SetOpen")]
    internal static readonly Action<bool> Window_SetOpen;

    [EzIPC("DTR.IsShown")]
    internal static readonly Func<bool> DTR_IsShown;

    [EzIPC("DTR.SetShown")]
    internal static readonly Action<bool> DTR_SetShown;

    internal static bool IsEnabled => IPCSubscriber_Common.IsReady("vnavmesh");

    internal static void Dispose()
    {
        IPCSubscriber_Common.DisposeAll(_disposalTokens);
    }
}

internal static class GatherBuddyReborn_IPCSubscriber
{
    public static event Action<bool> OnAutoGatherStatusChanged;
    private static readonly EzIPCDisposalToken[] _disposalTokens;
    static GatherBuddyReborn_IPCSubscriber()
    {
        _disposalTokens = EzIPC.Init(typeof(GatherBuddyReborn_IPCSubscriber),"GatherBuddyReborn");
    }


    [EzIPC]
    internal static readonly Func<bool> IsAutoGatherEnabled;
    [EzIPC]
    internal static readonly Action<bool> SetAutoGatherEnabled;
    [EzIPC]
    internal static readonly Func<bool> IsAutoGatherWaiting;

    [EzIPCEvent]
    public static void AutoGatherEnabledChanged(bool enabled)
    {
        OnAutoGatherStatusChanged.Invoke(enabled);
    }
}

internal class IPCSubscriber_Common
{
    internal static bool IsReady(string pluginName)
    {
        return DalamudReflector.TryGetDalamudPlugin(pluginName, out _, false, true);
    }

    internal static Version Version(string pluginName)
    {
        return DalamudReflector.TryGetDalamudPlugin(pluginName, out var dalamudPlugin, false, true)
                   ? dalamudPlugin.GetType().Assembly.GetName().Version
                   : new Version(0, 0, 0, 0);
    }

    internal static void DisposeAll(EzIPCDisposalToken[] disposalTokens)
    {
        foreach (var disposalToken in disposalTokens)
            try
            {
                disposalToken.Dispose();
            }
            catch (Exception ex)
            {
                Svc.Log.Debug($"Error while unregistering IPC: {ex}");
            }
    }
}
