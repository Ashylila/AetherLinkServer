using System;
using System.Collections.Generic;
using ECommons.DalamudServices;
using ECommons.EzIpcManager;
using ECommons.Reflection;
using Lumina.Excel.Sheets;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Threading.Tasks;
using System.Threading;

#nullable disable

namespace AetherLinkServer.IPC;

internal static class AutoRetainer_IPCSubscriber
{
    private static EzIPCDisposalToken[] _disposalTokens = EzIPC.Init(typeof(AutoRetainer_IPCSubscriber), "AutoRetainer.PluginState", SafeWrapper.IPCException);

    internal static bool IsEnabled => IPCSubscriber_Common.IsReady("AutoRetainer");

    [EzIPC] internal static readonly Func<bool> IsBusy;
    [EzIPC] internal static readonly Func<Dictionary<ulong, HashSet<string>>> GetEnabledRetainers;
    [EzIPC] internal static readonly Func<bool> AreAnyRetainersAvailableForCurrentChara;
    [EzIPC] internal static readonly System.Action AbortAllTasks;
    [EzIPC] internal static readonly System.Action DisableAllFunctions;
    [EzIPC] internal static readonly System.Action EnableMultiMode;
    [EzIPC] internal static readonly Func<int> GetInventoryFreeSlotCount;
    [EzIPC] internal static readonly System.Action EnqueueHET;

    internal static void Dispose() => IPCSubscriber_Common.DisposeAll(_disposalTokens);

}
 internal static class VNavmesh_IPCSubscriber
    {
        private static EzIPCDisposalToken[] _disposalTokens = EzIPC.Init(typeof(VNavmesh_IPCSubscriber), "vnavmesh", SafeWrapper.IPCException);

        internal static bool IsEnabled => IPCSubscriber_Common.IsReady("vnavmesh");

        [EzIPC("Nav.IsReady",            true)] internal static readonly Func<bool>                                                           Nav_IsReady;
        [EzIPC("Nav.BuildProgress",      true)] internal static readonly Func<float>                                                          Nav_BuildProgress;
        [EzIPC("Nav.Reload",             true)] internal static readonly Func<bool>                                                           Nav_Reload;
        [EzIPC("Nav.Rebuild",            true)] internal static readonly Func<bool>                                                           Nav_Rebuild;
        [EzIPC("Nav.Pathfind",           true)] internal static readonly Func<Vector3, Vector3, bool, Task<List<Vector3>>>                    Nav_Pathfind;
        [EzIPC("Nav.PathfindCancelable", true)] internal static readonly Func<Vector3, Vector3, bool, CancellationToken, Task<List<Vector3>>> Nav_PathfindCancelable;
        [EzIPC("Nav.PathfindCancelAll",  true)] internal static readonly System.Action                                                               Nav_PathfindCancelAll;
        [EzIPC("Nav.PathfindInProgress", true)] internal static readonly Func<bool>                                                           Nav_PathfindInProgress;
        [EzIPC("Nav.PathfindNumQueued",  true)] internal static readonly Func<int>                                                            Nav_PathfindNumQueued;
        [EzIPC("Nav.IsAutoLoad",         true)] internal static readonly Func<bool>                                                           Nav_IsAutoLoad;
        [EzIPC("Nav.SetAutoLoad",        true)] internal static readonly Action<bool>                                                         Nav_SetAutoLoad;

        [EzIPC("Query.Mesh.NearestPoint", true)] internal static readonly Func<Vector3, float, float, Vector3> Query_Mesh_NearestPoint;
        [EzIPC("Query.Mesh.PointOnFloor", true)] internal static readonly Func<Vector3, bool, float, Vector3> Query_Mesh_PointOnFloor;

        [EzIPC("Path.MoveTo", true)] internal static readonly Action<List<Vector3>, bool> Path_MoveTo;
        [EzIPC("Path.Stop", true)] internal static readonly System.Action Path_Stop;
        [EzIPC("Path.IsRunning", true)] internal static readonly Func<bool> Path_IsRunning;
        [EzIPC("Path.NumWaypoints", true)] internal static readonly Func<int> Path_NumWaypoints;
        [EzIPC("Path.GetMovementAllowed", true)] internal static readonly Func<bool> Path_GetMovementAllowed;
        [EzIPC("Path.SetMovementAllowed", true)] internal static readonly Action<bool> Path_SetMovementAllowed;
        [EzIPC("Path.GetAlignCamera", true)] internal static readonly Func<bool> Path_GetAlignCamera;
        [EzIPC("Path.SetAlignCamera", true)] internal static readonly Action<bool> Path_SetAlignCamera;
        [EzIPC("Path.GetTolerance", true)] internal static readonly Func<float> Path_GetTolerance;
        [EzIPC("Path.SetTolerance", true)] internal static readonly Action<float> Path_SetTolerance;

        [EzIPC("SimpleMove.PathfindAndMoveTo", true)] internal static readonly Func<Vector3, bool, bool> SimpleMove_PathfindAndMoveTo;
        [EzIPC("SimpleMove.PathfindInProgress", true)] internal static readonly Func<bool> SimpleMove_PathfindInProgress;

        [EzIPC("Window.IsOpen", true)] internal static readonly Func<bool> Window_IsOpen;
        [EzIPC("Window.SetOpen", true)] internal static readonly Action<bool> Window_SetOpen;

        [EzIPC("DTR.IsShown", true)] internal static readonly Func<bool> DTR_IsShown;
        [EzIPC("DTR.SetShown", true)] internal static readonly Action<bool> DTR_SetShown;

        internal static void Dispose() => IPCSubscriber_Common.DisposeAll(_disposalTokens);
    }

    internal static class GatherBuddyReborn_IPCSubscriber
    {
         private static EzIPCDisposalToken[] _disposalTokens = EzIPC.Init(typeof(AutoRetainer_IPCSubscriber), "GatherBuddy", SafeWrapper.IPCException);
    }
    
    internal class IPCSubscriber_Common
{
    internal static bool IsReady(string pluginName) => DalamudReflector.TryGetDalamudPlugin(pluginName, out _, false, true);
    
    internal static Version Version(string pluginName) => DalamudReflector.TryGetDalamudPlugin(pluginName, out var dalamudPlugin, false, true) ? dalamudPlugin.GetType().Assembly.GetName().Version : new Version(0, 0, 0, 0);

    internal static void DisposeAll(EzIPCDisposalToken[] disposalTokens)
    {
        foreach (var disposalToken in disposalTokens)
        {
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
}