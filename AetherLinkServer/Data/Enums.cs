using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;
using ImGuiNET;

namespace AetherLinkServer.Data;

public static class Enums
{
    public enum ActionState
    {
        None = 0,
        Running = 1,
        Teleporting = 2
    }

    [Flags]
    public enum ActionTag
    {
        None = 0,
        Synced = 1 << 0,
        Unsynced = 1 << 1,
        Comment = 1 << 2,
        Revival = 1 << 3,
        Treasure = 1 << 4,
        W2W = 1 << 5
    }

    //AutoRetainer
    public enum ApartmentHousingMarker : uint
    {
        PartiallyFilled = 60790,
        Full = 60792
    }

    public enum ClassJobType
    {
        Adventurer = 0,
        Gladiator = 1,
        Pugilist = 2,
        Marauder = 3,
        Lancer = 4,
        Archer = 5,
        Conjurer = 6,
        Thaumaturge = 7,
        Carpenter = 8,
        Blacksmith = 9,
        Armorer = 10,
        Goldsmith = 11,
        Leatherworker = 12,
        Weaver = 13,
        Alchemist = 14,
        Culinarian = 15,
        Miner = 16,
        Botanist = 17,
        Fisher = 18,
        Paladin = 19,
        Monk = 20,
        Warrior = 21,
        Dragoon = 22,
        Bard = 23,
        White_Mage = 24,
        Black_Mage = 25,
        Arcanist = 26,
        Summoner = 27,
        Scholar = 28,
        Rogue = 29,
        Ninja = 30,
        Machinist = 31,
        Dark_Knight = 32,
        Astrologian = 33,
        Samurai = 34,
        RedMage = 35,
        BlueMage = 36,
        Gunbreaker = 37,
        Dancer = 38,
        Reaper = 39,
        Sage = 40,
        Pictomancer = 42
    }

    [Flags]
    public enum DutyMode
    {
        None = 0,
        Support = 1,
        Trust = 2,
        Squadron = 4,
        Regular = 8,
        Trial = 16,
        Raid = 32,
        Variant = 64
    }

    public enum DutyState
    {
        None = 0,
        DutyStarted = 1,
        DutyWiped = 2,
        DutyRecommenced = 3,
        DutyComplete = 4
    }

    //AutoRetainer
    public enum FCHousingMarker : uint
    {
        SmallYellow = 60761,
        MediumYellow = 60762,
        LargeYellow = 60763,
        SmallBlue = 60764,
        MediumBlue = 60765,
        LargeBlue = 60766
    }

    public enum Housing
    {
        Apartment = 1,
        Personal_Home = 2,
        FC_Estate = 3
    }

    public enum JobRole
    {
        None = 0,
        Tank = 1,
        Melee = 2,
        Ranged = 3,
        Healer = 4,
        Ranged_Physical = 5,
        Ranged_Magical = 6,
        Disciple_Of_The_Land = 7,
        Disciple_Of_The_Hand = 8
    }

    [Flags]
    public enum JobWithRole
    {
        None = 0,
        Paladin = 1 << 0,
        Warrior = 1 << 1,
        Dark_Knight = 1 << 2,
        Gunbreaker = 1 << 3,
        Tanks = Paladin | Warrior | Dark_Knight | Gunbreaker,
        White_Mage = 1 << 4,
        Scholar = 1 << 5,
        Astrologian = 1 << 6,
        Sage = 1 << 7,
        Healers = White_Mage | Scholar | Astrologian | Sage,
        Monk = 1 << 8,
        Dragoon = 1 << 9,
        Ninja = 1 << 10,
        Samurai = 1 << 11,
        Reaper = 1 << 12,
        Viper = 1 << 13,
        Striking = Monk | Samurai,
        Maiming = Dragoon | Reaper,
        Scouting = Ninja | Viper,
        Melee = Striking | Maiming | Scouting,
        Bard = 1 << 14,
        Machinist = 1 << 15,
        Dancer = 1 << 16,
        Aiming = Bard | Machinist | Dancer,
        Black_Mage = 1 << 17,
        Summoner = 1 << 18,
        Red_Mage = 1 << 19,
        Pictomancer = 1 << 20,
        Casters = Black_Mage | Summoner | Red_Mage | Pictomancer,
        DPS = Melee | Aiming | Casters,
        All = Tanks | Healers | DPS
    }

    public enum LevelingMode
    {
        None = 0,
        Support = 1,
        Trust = 2
    }

    public enum LootMethod
    {
        AutoDuty = 0,
        RotationSolver = 1,
        Pandora = 2,
        All = 3
    }

    public enum PlayerLifeState
    {
        Alive = 0,
        Dead = 1,
        Revived = 2
    }

    [Flags]
    public enum PluginState
    {
        None = 0,
        Looping = 1,
        Navigating = 2,
        Paused = 4,
        Other = 8
    }

    public enum Positional
    {
        Any = 0,
        Flank = 1,
        Rear = 2,
        Front = 3
    }

    //AutoRetainer
    public enum PrivateHousingMarker : uint
    {
        SmallYellow = 60776,
        MediumYellow = 60777,
        LargeYellow = 60778,
        SmallBlue = 60779,
        MediumBlue = 60780,
        LargeBlue = 60781
    }

    public enum RetireLocation
    {
        Inn = 0,
        Apartment = 1,
        Personal_Home = 2,
        FC_Estate = 3,
        GC_Barracks = 4
    }

    public enum Role
    {
        Tank = 0,
        Healer = 1,
        Ranged_DPS = 2,
        Melee_DPS = 3
    }

    [Flags]
    public enum SettingsActive
    {
        None = 0,
        Vnav_Align_Camera_Off = 1,
        Pandora_Interact_Objects = 2,
        YesAlready = 4,
        PreLoop_Enabled = 8,
        BetweenLoop_Enabled = 16,
        TerminationActions_Enabled = 32,
        BareMode_Active = 64
    }

    public enum Sounds : byte
    {
        None = 0x00,
        Unknown = 0x01,
        Sound01 = 0x25,
        Sound02 = 0x26,
        Sound03 = 0x27,
        Sound04 = 0x28,
        Sound05 = 0x29,
        Sound06 = 0x2A,
        Sound07 = 0x2B,
        Sound08 = 0x2C,
        Sound09 = 0x2D,
        Sound10 = 0x2E,
        Sound11 = 0x2F,
        Sound12 = 0x30,
        Sound13 = 0x31,
        Sound14 = 0x32,
        Sound15 = 0x33,
        Sound16 = 0x34
    }

    public enum Stage
    {
        Stopped = 0,
        Reading_Path = 1,
        Action = 2,
        Looping = 3,
        Condition = 4,
        Moving = 5,
        Waiting_For_Combat = 6,
        Paused = 7,
        Dead = 8,
        Revived = 9,
        Interactable = 10
    }

    public enum SummoningBellLocations : uint
    {
        Inn = 0,
        Apartment = 1,
        Personal_Home = 2,
        FC_Estate = 3,
        Limsa_Lominsa_Lower_Decks = 129,
        Old_Gridania = 133,
        Uldah_Steps_of_Thal = 131,
        The_Pillars = 419,
        Rhalgrs_Reach = 635,
        Kugane = 628,
        The_Doman_Enclave = 759,
        The_Crystarium = 819,
        Eulmore = 820,
        Old_Sharlayan = 962,
        Radz_at_Han = 963,
        Tuliyollal = 1185,
        Nexus_Arcade = 1186
    }

    public enum TerminationMode
    {
        Do_Nothing = 0,
        Logout = 1,
        Start_AR_Multi_Mode = 2,
        Kill_Client = 3,
        Kill_PC = 4
    }

    public enum TrustMemberName : byte
    {
        Alphinaud = 1,
        Alisaie = 2,
        Thancred = 3,
        Urianger = 5,
        Yshtola = 6,
        Ryne = 7,
        Estinien = 12,
        Graha = 10,
        Zero = 41,
        Krile = 60
    }

    public enum TrustRole : byte
    {
        DPS = 0,
        Healer = 1,
        Tank = 2,
        AllRounder = 3
    }

    public static bool HasAnyFlag<T>(this T instance, params T[] parameter) where T : Enum
    {
        return parameter.Any(enu => instance.HasFlag(enu));
    }
}

public static class JobWithRoleHelper
{
    private static readonly List<Enums.JobWithRole> enumVals = Enum.GetValues<Enums.JobWithRole>().Skip(1).ToList();

    public static Dictionary<Enums.JobWithRole, IEnumerable<Enums.JobWithRole>> categories = enumVals
        .Select(jwr => (jwr, enumVals.Where(jwrr => jwr != jwrr && jwr.HasFlag(jwrr)))).Where(j => j.Item2.Any())
        .ToDictionary(j => j.jwr, j => j.Item2);

    public static Dictionary<Enums.JobWithRole, IEnumerable<Enums.JobWithRole>> values = enumVals
        .Select(jwr => (jwr, enumVals.Where(jwrr => jwr != jwrr && jwrr.HasFlag(jwr)))).Where(j => j.Item2.Any())
        .ToDictionary(j => j.jwr, j => j.Item2);

    public static bool HasJobFlagFast(this Enums.JobWithRole value, Enums.JobWithRole flag)
    {
        return (value & flag) == flag;
    }

    public static bool HasJob(this Enums.JobWithRole jwr, Job job)
    {
        var jw = job.JobToJobWithRole();
        return jwr.HasJobFlagFast(jw);
    }

    public static Enums.JobWithRole JobToJobWithRole(this Job job)
    {
        return job switch
        {
            Job.GLA or Job.PLD => Enums.JobWithRole.Paladin,
            Job.MRD or Job.WAR => Enums.JobWithRole.Warrior,
            Job.DRK => Enums.JobWithRole.Dark_Knight,
            Job.GNB => Enums.JobWithRole.Gunbreaker,
            Job.CNJ or Job.WHM => Enums.JobWithRole.White_Mage,
            Job.SCH => Enums.JobWithRole.Scholar,
            Job.SGE => Enums.JobWithRole.Sage,
            Job.AST => Enums.JobWithRole.Astrologian,
            Job.PGL or Job.MNK => Enums.JobWithRole.Monk,
            Job.LNC or Job.DRG => Enums.JobWithRole.Dragoon,
            Job.ROG or Job.NIN => Enums.JobWithRole.Ninja,
            Job.SAM => Enums.JobWithRole.Samurai,
            Job.RPR => Enums.JobWithRole.Reaper,
            Job.VPR => Enums.JobWithRole.Viper,
            Job.ARC or Job.BRD => Enums.JobWithRole.Bard,
            Job.MCH => Enums.JobWithRole.Machinist,
            Job.DNC => Enums.JobWithRole.Dancer,
            Job.THM or Job.BLM => Enums.JobWithRole.Black_Mage,
            Job.ACN or Job.SMN => Enums.JobWithRole.Summoner,
            Job.RDM => Enums.JobWithRole.Red_Mage,
            Job.PCT => Enums.JobWithRole.Pictomancer,
            _ => Enums.JobWithRole.None
        };
    }

    public static IEnumerable<Job> ContainedJobs(this Enums.JobWithRole jwr)
    {
        return Enum.GetValuesAsUnderlyingType<Job>().Cast<Job>().Where(job => jwr.HasJob(job));
    }

    public static void DrawSelectable(Enums.JobWithRole jwr, ref Enums.JobWithRole config, bool allowRemoval = true)
    {
        var flag = (int)config;

        using (ImRaii.Disabled(!allowRemoval && config.HasFlag(jwr)))
        {
            if (ImGui.CheckboxFlags(jwr.ToString().Replace("_", " "), ref flag, (int)jwr))
                config = (Enums.JobWithRole)flag;
        }
    }

    public static void DrawCategory(Enums.JobWithRole category, ref Enums.JobWithRole config, bool allowRemoval = true)
    {
        ImGui.PushStyleColor(ImGuiCol.Header, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.2f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.3f));
        var collapse = ImGui.CollapsingHeader("##" + category, ImGuiTreeNodeFlags.AllowItemOverlap);
        ImGui.PopStyleColor(3);
        ImGui.SameLine();
        DrawSelectable(category, ref config, allowRemoval);
        if (collapse)
        {
            ImGui.Indent();
            foreach (var jobW in categories[category])
                if (values[jobW].MinBy(jwr => categories[jwr].Count()) == category)
                {
                    if (categories.ContainsKey(jobW))
                        DrawCategory(jobW, ref config, allowRemoval);
                    else
                    {
                        ImGui.Indent();
                        DrawSelectable(jobW, ref config, allowRemoval);
                        ImGui.Unindent();
                    }
                }

            ImGui.Unindent();
        }
    }
}
