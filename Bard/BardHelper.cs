using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;
using PromeRotation.Data;
using PromeRotation.Extensions;
using WotouTC.Bard.Data;

namespace WotouTC.Bard;

public class BardHelper
{
    public static readonly uint[] PoisonDotBuffs = [BRDBuff.VenomousBite, BRDBuff.CausticBite];
    public static readonly uint[] WindDotBuffs = [BRDBuff.Windbite, BRDBuff.Stormbite];

    private const uint BurstShot = BRDSkill.BurstShot;
    private const uint RefulgentArrow = BRDSkill.RefulgentArrow;
    private const uint Ladonsbite = BRDSkill.Ladonsbite;
    private const uint Shadowbite = BRDSkill.Shadowbite;

    private const uint HawkEyeBuff = BRDBuff.HawksEye;
    private const uint BarrageBuff = BRDBuff.Barrage;

    public static PAction GetBaseGcd(ActionTargetType target = ActionTargetType.Target)
    {
        var me = Core.Core.Me;
        if (me != null && me.HasStatus(HawkEyeBuff))
        {
            return PromeSettings.Instance.GetQt(BRDQt.AOE)
                       ? new PAction(Shadowbite, ActionType.Gcd, target)
                       : new PAction(RefulgentArrow, ActionType.Gcd, target);
        }


        if (me != null && me.HasStatus(BarrageBuff))
        {
            return new PAction(RefulgentArrow, ActionType.Gcd, target);
        }

        return PromeSettings.Instance.GetQt(BRDQt.AOE) ?
                   new PAction(Ladonsbite, ActionType.Gcd, target) :
                   new PAction(BurstShot, ActionType.Gcd, target);
    }

    public static bool HasAnyDot(IBattleChara? target, uint[] dots)
    {
        return target != null && dots.Any(dot => HasOwnStatus(target, dot));
    }

    public static bool HasOwnStatus(IBattleChara target, uint statusId)
    {
        var me = Core.Core.Me;
        if (me == null) return false;

        return target.StatusList.Any(status =>
            status.StatusId == statusId &&
            status.SourceId == me.EntityId &&
            status.RemainingTime > 0);
    }

    public static bool HasOwnStatusWithTimeLeft(IBattleChara target, uint statusId, int milliseconds)
    {
        var me = Core.Core.Me;
        if (me == null) return false;

        var seconds = milliseconds / 1000f;
        return target.StatusList.Any(status =>
            status.StatusId == statusId &&
            status.SourceId == me.EntityId &&
            status.RemainingTime > seconds);
    }

    public static bool HasSelfStatus(uint statusId)
    {
        return Core.Core.Me?.HasStatus(statusId) == true;
    }

    public static bool HasSelfStatusWithTimeLeft(uint statusId, int milliseconds)
    {
        var me = Core.Core.Me;
        if (me == null) return false;

        var seconds = milliseconds / 1000f;
        return me.StatusList.Any(status =>
            status.StatusId == statusId &&
            status.SourceId == me.EntityId &&
            status.RemainingTime > seconds);
    }

    public static bool IsBoss(IBattleChara target)
    {
        if (target == null)
            return false;
        if (IsDummy(target))
            return true;

        var isBossRank = false;
        try
        {
            if (Svc.Data.GetExcelSheet<BNpcBase>().TryGetRow(target.DataId, out var dataRow))
            {
                isBossRank = dataRow.Rank is 2 or 6;
            }
        }
        catch
        {
            isBossRank = false;
        }

        var me = Core.Core.Me;
        var hpThreshold = me == null ? 0UL : (ulong)me.MaxHp * 20UL;
        return isBossRank || (hpThreshold > 0 && target.MaxHp >= hpThreshold);
    }

    private static bool IsDummy(IBattleChara target)
    {
        try
        {
            var name = target.Name.ToString();
            return name.Contains("木人") ||
                   name.Contains("训练假人") ||
                   name.Contains("Striking Dummy", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsUnlocked(uint actionId)
    {
        var me = Core.Core.Me;
        if (me == null) return false;

        return actionId switch
        {
            BRDSkill.VenomousBite => me.Level >= 6,
            BRDSkill.Windbite => me.Level >= 30,
            BRDSkill.IronJaws => me.Level >= 56,
            BRDSkill.Barrage => me.Level >= 38,
            BRDSkill.Bloodletter => me.Level >= 12,
            BRDSkill.RainofDeath => me.Level >= 45,
            BRDSkill.RefulgentArrow => me.Level >= 70,
            BRDSkill.Shadowbite => me.Level >= 72,
            BRDSkill.RagingStrikes => me.Level >= 4,
            BRDSkill.BattleVoice => me.Level >= 50,
            BRDSkill.ApexArrow => me.Level >= 80,
            BRDSkill.EmpyrealArrow => me.Level >= 54,
            BRDSkill.Sidewinder => me.Level >= 60,
            BRDSkill.PitchPerfect => me.Level >= 52,
            BRDSkill.HeartBreak => me.Level >= 92,
            BRDSkill.MagesBallad => me.Level >= 30,
            BRDSkill.ArmysPaeon => me.Level >= 40,
            BRDSkill.TheWanderersMinuet => me.Level >= 52,
            BRDSkill.RadiantFinale => me.Level >= 90,
            BRDSkill.RadiantEncore => me.Level >= 100,
            BRDSkill.ResonantArrow => me.Level >= 100,
            BRDSkill.BlastArrow => me.Level >= 86,
            _ => true
        };
    }

    public static bool IsUnlockedWithCdCheck(uint actionId)
    {
        return IsUnlocked(actionId) && actionId.GetActionCooldown() <= 0.05f;
    }

    public static bool RecentlyUsed(uint actionId, int span = 1000)
    {
        if (actionId == 0)
            return false;

        var lastActionTime = BardBattleData.Instance.GetLastActionTime(actionId);
        return lastActionTime != 0 && Environment.TickCount64 - lastActionTime < span;
    }

    public static Song CurrentSong => BardSongHelper.CurrentSong;

    public static float SongTimerMs => BardSongHelper.SongTimerMs;

    public static bool IsSongOrderNormal() => BardSongHelper.IsSongOrderNormal();

    public static uint GetSpellBySong(Song song) => BardSongHelper.GetSpellBySong(song);

    public static Song GetSongBySpell(uint actionId) => BardSongHelper.GetSongBySpell(actionId);

    public static float GetSongDuration(Song song) => BardSongHelper.GetSongDuration(song);

    public static bool IsSongReady(Song song) => BardSongHelper.IsSongReady(song);

    public static bool RecentlyUsedAnySong(int span = 1000) => BardSongHelper.RecentlyUsedAnySong(span);

    public static bool CanSwitchFromFirstToSecond() => BardSongHelper.CanSwitchFromFirstToSecond();

    public static bool CanSwitchFromSecondToThird() => BardSongHelper.CanSwitchFromSecondToThird();

    public static bool CanSwitchFromThirdToFirst() => BardSongHelper.CanSwitchFromThirdToFirst();

    public static bool CanSwitchFromNone() => BardSongHelper.CanSwitchFromNone();

    public static uint GetNextSongAction() => BardSongHelper.GetNextSongAction();

    public static bool ShouldSwitchSong(out uint songActionId, bool allowNoTarget = false)
    {
        return BardSongHelper.ShouldSwitchSong(out songActionId, allowNoTarget);
    }

    public static bool ShouldSwitchSong(out uint songActionId, out string reason, bool allowNoTarget = false)
    {
        return BardSongHelper.ShouldSwitchSong(out songActionId, out reason, allowNoTarget);
    }

    public static uint Adjust(uint actionId)
    {
        var adjusted = actionId.GetAdjustedActionId();
        return adjusted == 0 ? actionId : adjusted;
    }

    public static bool HasAllPartyBuff()
    {
        return IsBattleVoiceConditionMet() && IsRagingStrikesConditionMet() && IsRadiantFinaleConditionMet();
    }

    public static bool HasAnyPartyBuff()
    {
        return HasSelfStatus(BRDBuff.BattleVoice) ||
               HasSelfStatus(BRDBuff.RagingStrikes) ||
               HasSelfStatus(BRDBuff.RadiantFinale);
    }

    public static bool HasNoPartyBuff()
    {
        return !HasAnyPartyBuff();
    }

    public static bool PartyBuffWillBeReadyIn(float milliseconds)
    {
        return BRDSkill.BattleVoice.GetActionCooldown() * 1000f <= milliseconds &&
               BRDSkill.RagingStrikes.GetActionCooldown() * 1000f <= milliseconds;
    }

    private static bool IsRadiantFinaleConditionMet()
    {
        return Core.Core.Me?.Level < 90 || HasSelfStatus(BRDBuff.RadiantFinale);
    }

    private static bool IsBattleVoiceConditionMet()
    {
        return !IsUnlocked(BRDSkill.BattleVoice) || HasSelfStatus(BRDBuff.BattleVoice);
    }

    private static bool IsRagingStrikesConditionMet()
    {
        return !IsUnlocked(BRDSkill.RagingStrikes) || HasSelfStatus(BRDBuff.RagingStrikes);
    }
}

