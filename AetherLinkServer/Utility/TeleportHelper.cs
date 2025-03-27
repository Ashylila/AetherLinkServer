using System;
using System.Linq;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.IPC;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace AetherLinkServer.Utility;

public static class TeleportHelper
{
    public static unsafe bool TryFindAetheryteByName(string name, bool matchPartial, out TeleportInfo info) {
        info = new TeleportInfo();
        foreach(var tpInfo in Telepo.Instance()->TeleportList) {
            var aetheryteName = Svc.Data.GetExcelSheet<Aetheryte>().FirstOrDefault(x => x.RowId == tpInfo.AetheryteId).PlaceName.ValueNullable?.Name.ToString();

            var result = matchPartial && aetheryteName.Contains(name, StringComparison.OrdinalIgnoreCase);
            if (!result && !aetheryteName.Equals(name, StringComparison.OrdinalIgnoreCase))
                continue;
            info = tpInfo;
            return true;
        }
        return false;
    }
}
