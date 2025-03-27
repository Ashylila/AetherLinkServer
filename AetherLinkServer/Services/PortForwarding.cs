using System;
using System.Net.Http;
using System.Threading.Tasks;
using ECommons.DalamudServices;
using Open.Nat;

namespace AetherLinkServer.Services;
public class PortForwarding
{
    public static async Task<bool> EnableUpnpPortForwarding(int port)
    {
        var discoverer = new NatDiscoverer();
        var cts = new System.Threading.CancellationTokenSource(5000);

        try
        {
            var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, "AetherLink WebSocket"));
            Console.WriteLine($"Port {port} forwarded successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex,"UPnP failed");
            return false;
        }
    }
    internal static async Task<string> GetPublicIpAddress()
    {
        using (var client = new HttpClient())
        {
            string ip = await client.GetStringAsync("https://api.ipify.org");
            return ip;
        }
    }
}

