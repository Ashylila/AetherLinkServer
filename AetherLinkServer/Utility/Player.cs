using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Common.Math;
using Lumina.Excel.Sheets;
using static AetherLinkServer.Data.Enums;

namespace AetherLinkServer.Utility;

internal static class PlayerHelper
{
    /*internal static unsafe short GetCurrentLevelFromSheet(Job? job = null)
    {
        PlayerState* playerState = PlayerState.Instance();
        return playerState->ClassJobLevels[Svc.Data.GetExcelSheet<ClassJob>().GetRowOrDefault((uint)(job ?? GetJob()))?.ExpArrayIndex ?? 0];
    }
*/
    internal static unsafe float GetDistanceToPlayer(IGameObject gameObject) => GetDistanceToPlayer(gameObject.Position);

    internal static unsafe float GetDistanceToPlayer(Vector3 v3) => Vector3.Distance(v3, Player.GameObject->Position);
    public static ActionState state = ActionState.None;

    public static bool CanAct
    {
        get
        {
            if (Svc.ClientState.LocalPlayer == null)
                return false;
            if (Svc.Condition[ConditionFlag.BetweenAreas]
                || Svc.Condition[ConditionFlag.BetweenAreas51]
                || Svc.Condition[ConditionFlag.OccupiedInQuestEvent]
                || Svc.Condition[ConditionFlag.OccupiedSummoningBell]
                || Svc.Condition[ConditionFlag.BeingMoved]
                || Svc.Condition[ConditionFlag.Casting]
                || Svc.Condition[ConditionFlag.Casting87]
                || Svc.Condition[ConditionFlag.Jumping]
                || Svc.Condition[ConditionFlag.Jumping61]
                || Svc.Condition[ConditionFlag.LoggingOut]
                || Svc.Condition[ConditionFlag.Occupied]
                || Svc.Condition[ConditionFlag.Occupied39]
                || Svc.Condition[ConditionFlag.Unconscious]
                || Svc.Condition[ConditionFlag.Gathering42]
                || Svc.Condition[ConditionFlag.MountOrOrnamentTransition] // Mounting up
                //Node is open? Fades off shortly after closing the node, can't use items (but can mount) while it's set
                || Svc.Condition[85] && !Svc.Condition[ConditionFlag.Gathering]
                || Svc.ClientState.LocalPlayer.IsDead
                || Player.IsAnimationLocked)
                return false;

            return true;
        }
    }

    internal static float JobRange
    {
        get
        {
            float radius = 25;
            if (!Player.Available)
                return radius;
            radius = (Svc.Data.GetExcelSheet<ClassJob>().GetRowOrDefault(Player.Object.ClassJob.RowId)?.GetJobRole() ??
                      JobRole.None) switch
            {
                JobRole.Tank or JobRole.Melee => 2.6f,
                _ => radius
            };
            return radius;
        }
    }

    internal static float AoEJobRange
    {
        get
        {
            float radius = 10;
            if (!Player.Available) return radius;
            radius = (Svc.Data.GetExcelSheet<ClassJob>().GetRowOrDefault(Player.Object.ClassJob.RowId)?.GetJobRole() ??
                      JobRole.None) switch
            {
                JobRole.Tank or JobRole.Melee => 2.6f,
                _ => radius
            };

            if (Player.Object.ClassJob.RowId == 38)
                radius = 3;
            return radius;
        }
    }

    internal static bool IsValid => Svc.Condition.Any()
                                    && !Svc.Condition[ConditionFlag.BetweenAreas]
                                    && !Svc.Condition[ConditionFlag.BetweenAreas51]
                                    && Player.Available
                                    && Player.Interactable;

    internal static bool IsJumping => Svc.Condition.Any()
                                      && (Svc.Condition[ConditionFlag.Jumping]
                                          || Svc.Condition[ConditionFlag.Jumping61]);

    internal static unsafe bool IsAnimationLocked => ActionManager.Instance()->AnimationLock > 0;

    internal static bool IsReady => IsValid && !IsOccupied;

    internal static bool IsOccupied => GenericHelpers.IsOccupied() || Svc.Condition[ConditionFlag.Jumping61];

    internal static bool IsReadyFull => IsValid && !IsOccupiedFull;

    internal static bool IsOccupiedFull => IsOccupied || IsAnimationLocked;

    internal static unsafe bool IsCasting => Player.Character->IsCasting;

    internal static unsafe bool IsMoving => AgentMap.Instance()->IsPlayerMoving;

    internal static bool InCombat => Svc.Condition[ConditionFlag.InCombat];

    internal static uint GetGrandCompanyTerritoryType(uint grandCompany)
    {
        return grandCompany switch
        {
            1 => 128u,
            2 => 132u,
            _ => 130u
        };
    }

    internal static unsafe uint GetGrandCompany()
    {
        return UIState.Instance()->PlayerState.GrandCompany;
    }

    internal static unsafe uint GetGrandCompanyRank()
    {
        return UIState.Instance()->PlayerState.GetGrandCompanyRank();
    }

    internal static uint GetMaxDesynthLevel()
    {
        return Svc.Data.Excel.GetSheet<Item>().Where(x => x.Desynth > 0).OrderBy(x => x.LevelItem.RowId).LastOrDefault()
                  .LevelItem.RowId;
    }

    internal static unsafe float GetDesynthLevel(uint classJobId)
    {
        return PlayerState.Instance()->GetDesynthesisLevel(classJobId);
    }

    internal static JobRole GetJobRole(this ClassJob job)
    {
        var role = (JobRole)job.Role;

        if (role is JobRole.Ranged or JobRole.None)
        {
            role = job.ClassJobCategory.RowId switch
            {
                30 => JobRole.Ranged_Physical,
                31 => JobRole.Ranged_Magical,
                32 => JobRole.Disciple_Of_The_Land,
                33 => JobRole.Disciple_Of_The_Hand,
                _ => JobRole.None
            };
        }

        return role;
    }

    internal static unsafe short GetCurrentItemLevelFromGearSet(
        int gearsetId = -1, bool updateGearsetBeforeCheck = true)
    {
        var gearsetModule = RaptureGearsetModule.Instance();
        if (gearsetId < 0)
            gearsetId = gearsetModule->CurrentGearsetIndex;
        if (updateGearsetBeforeCheck)
            gearsetModule->UpdateGearset(gearsetId);
        return gearsetModule->GetGearset(gearsetId)->ItemLevel;
    }

    //internal static Job GetJob() => Player.Available ? Player.Job : Plugin.JobLastKnown;

    internal static CombatRole GetCombatRole(this Job? job)
    {
        return job != null ? GetCombatRole((Job)job) : CombatRole.NonCombat;
    }

    internal static CombatRole GetCombatRole(this Job job)
    {
        return job switch
        {
            Job.GLA or Job.PLD or Job.MRD or Job.WAR or Job.DRK or Job.GNB => CombatRole.Tank,
            Job.CNJ or Job.WHM or Job.SGE or Job.SCH or Job.AST => CombatRole.Healer,
            Job.PGL or Job.MNK or Job.LNC or Job.DRG or Job.ROG or Job.NIN or Job.SAM or Job.RPR or Job.VPR or
                Job.ARC or Job.BRD or Job.DNC or Job.MCH or
                Job.THM or Job.BLM or Job.ACN or Job.SMN or Job.RDM or Job.PCT or Job.BLU => CombatRole.DPS,
            _ => CombatRole.NonCombat
        };
    }

    internal static bool HasStatus(uint statusID)
    {
        return Svc.ClientState.LocalPlayer != null && Player.Object.StatusList.Any(x => x.StatusId == statusID);
    }
}
