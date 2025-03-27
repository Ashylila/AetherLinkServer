using System;
using AetherLinkServer.DalamudServices;
using Dalamud.Plugin.Ipc;
using System.Linq;
using Lumina.Excel.Sheets;

namespace AetherLinkServer.IPC;

public static class Teleport
{
    private static ICallGateSubscriber<uint, byte, bool>? _teleportIpc;
    private static ICallGateSubscriber<bool>? _teleportChatMessageIpc;
    public static bool IsReady => Svc.PluginInterface.InstalledPlugins.Any(x => x.Name == "Teleport" && x.IsLoaded);

    static Teleport()
    {
        _teleportIpc = Svc.PluginInterface.GetIpcSubscriber<uint, byte, bool>("Teleport");
        _teleportChatMessageIpc = Svc.PluginInterface.GetIpcSubscriber<bool>("Teleport.ChatMessage");
    }
    public static bool TeleportToAetheryte(uint aetheryteId, byte subIndex)
    {
        if (!IsReady)return false;
        try
        {
            return _teleportIpc.InvokeFunc(aetheryteId, subIndex);
        }catch(Exception ex)
        {
            Svc.Log.Error(ex, "Error while invoking Teleport IPC");
            return false;
        }
    }

    public static bool GetTeleportChatMessageSetting()
    {
        if(!IsReady)return false;
        try
        {
            return _teleportChatMessageIpc?.InvokeFunc() ?? false;
        }
        catch (Exception e)
        {
            Svc.Log.Error(e, "Error while invoking Teleport ChatMessage");
            return false;
        }
    }
    
}
