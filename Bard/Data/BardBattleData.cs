using Dalamud.Game.ClientState.JobGauge.Enums;

namespace WotouTC.Bard.Data;

public class BardBattleData
{
    public static BardBattleData Instance { get; } = new();

    public HashSet<uint> DotBlackList { get; } = new();

    public bool HasUseIronJawsInCurrentBursting { get; set; }

    public bool HasUseApexArrowInCurrentNonBurstingPeriod { get; set; }

    public int PitchPerfectMinEnemyCount { get; set; }

    public double TotalStopTime { get; set; }

    public double RestStopTime { get; set; }

    public int GcdCountDown { get; set; } = 3;

    public long BaseGcdHoldStartTime { get; set; }

    public double CurrentBaseGcdHoldTime { get; set; }

    public bool HasFirst120SBuff { get; set; }

    public bool HasSecond120SBuff { get; set; }

    public bool HasThird120SBuff { get; set; }

    public uint First120SBuffSpellId { get; set; } = BRDSkill.RagingStrikes;

    public uint First120SBuffId { get; set; } = BRDBuff.RagingStrikes;

    public uint Second120SBuffSpellId { get; set; } = BRDSkill.BattleVoice;

    public uint Second120SBuffId { get; set; } = BRDBuff.BattleVoice;

    public uint Third120SBuffSpellId { get; set; } = BRDSkill.RadiantFinale;

    public uint Third120SBuffId { get; set; } = BRDBuff.RadiantFinale;

    public Song LastSong { get; set; } = Song.None;

    public long LastSongTime { get; set; }

    public Dictionary<uint, long> LastActionTimes { get; } = new();

    public void ResetForBattle()
    {
        HasUseIronJawsInCurrentBursting = false;
        HasUseApexArrowInCurrentNonBurstingPeriod = false;
        TotalStopTime = 0;
        RestStopTime = 0;
        GcdCountDown = 3;
        BaseGcdHoldStartTime = 0;
        CurrentBaseGcdHoldTime = 0;
        HasFirst120SBuff = false;
        HasSecond120SBuff = false;
        HasThird120SBuff = false;
        First120SBuffSpellId = BRDSkill.RagingStrikes;
        First120SBuffId = BRDBuff.RagingStrikes;
        Second120SBuffSpellId = BRDSkill.BattleVoice;
        Second120SBuffId = BRDBuff.BattleVoice;
        Third120SBuffSpellId = BRDSkill.RadiantFinale;
        Third120SBuffId = BRDBuff.RadiantFinale;
        LastSong = Song.None;
        LastSongTime = 0;
        LastActionTimes.Clear();
    }

    public void RecordAction(uint actionId)
    {
        LastActionTimes[actionId] = Environment.TickCount64;
    }

    public void RecordGcdAction(uint actionId)
    {
        if (!IsBardGcd(actionId))
            return;

        if (GcdCountDown > 0)
            GcdCountDown--;

        BaseGcdHoldStartTime = 0;
        CurrentBaseGcdHoldTime = 0;
    }

    public void Record120SBuffAction(uint actionId)
    {
        if (actionId is not (BRDSkill.RagingStrikes or BRDSkill.BattleVoice or BRDSkill.RadiantFinale))
            return;

        if (!HasFirst120SBuff)
        {
            HasFirst120SBuff = true;
            First120SBuffSpellId = actionId;
            First120SBuffId = GetBuffIdBy120SBuffAction(actionId);
            return;
        }

        if (!HasSecond120SBuff)
        {
            HasSecond120SBuff = true;
            Second120SBuffSpellId = actionId;
            Second120SBuffId = GetBuffIdBy120SBuffAction(actionId);
            return;
        }

        if (!HasThird120SBuff)
        {
            HasThird120SBuff = true;
            Third120SBuffSpellId = actionId;
            Third120SBuffId = GetBuffIdBy120SBuffAction(actionId);
        }
    }

    public long GetLastActionTime(uint actionId)
    {
        return LastActionTimes.TryGetValue(actionId, out var time) ? time : 0;
    }

    private static bool IsBardGcd(uint actionId)
    {
        return actionId is BRDSkill.HeavyShot
            or BRDSkill.StraightShot
            or BRDSkill.BurstShot
            or BRDSkill.RefulgentArrow
            or BRDSkill.WideVolley
            or BRDSkill.QuickNock
            or BRDSkill.Ladonsbite
            or BRDSkill.Shadowbite
            or BRDSkill.ApexArrow
            or BRDSkill.BlastArrow
            or BRDSkill.ResonantArrow
            or BRDSkill.RadiantEncore
            or BRDSkill.VenomousBite
            or BRDSkill.CausticBite
            or BRDSkill.Windbite
            or BRDSkill.Stormbite
            or BRDSkill.IronJaws;
    }

    private static uint GetBuffIdBy120SBuffAction(uint actionId)
    {
        return actionId switch
        {
            BRDSkill.RagingStrikes => BRDBuff.RagingStrikes,
            BRDSkill.BattleVoice => BRDBuff.BattleVoice,
            BRDSkill.RadiantFinale => BRDBuff.RadiantFinale,
            _ => 0
        };
    }
}

