using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using static AetherLinkServer.Data.Enums;

namespace AetherLinkServer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    public SummoningBellLocations PreferredSummoningBellEnum = 0;
    public ushort Port { get; set; } = 5000;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
