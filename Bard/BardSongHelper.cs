using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard;

public static class BardSongHelper
{
    private const uint WanderersMinuet = BRDSkill.TheWanderersMinuet;
    private const uint MagesBallad = BRDSkill.MagesBallad;
    private const uint ArmysPaeon = BRDSkill.ArmysPaeon;

    public static Song CurrentSong => JobGaugeHelper.BRD.GetCurrentSong;

    public static float SongTimerMs => JobGaugeHelper.BRD.GetCurrentSongTimer;

    public static bool IsSongOrderNormal()
    {
        return BardSettings.Instance.FirstSong == Song.WanderersMinuet &&
               BardSettings.Instance.SecondSong == Song.MagesBallad &&
               BardSettings.Instance.ThirdSong == Song.ArmysPaeon;
    }

    public static uint GetSpellBySong(Song song)
    {
        return song switch
        {
            Song.WanderersMinuet => WanderersMinuet,
            Song.MagesBallad => MagesBallad,
            Song.ArmysPaeon => ArmysPaeon,
            _ => 0
        };
    }

    public static Song GetSongBySpell(uint actionId)
    {
        return actionId switch
        {
            WanderersMinuet => Song.WanderersMinuet,
            MagesBallad => Song.MagesBallad,
            ArmysPaeon => Song.ArmysPaeon,
            _ => Song.None
        };
    }

    public static float GetSongDuration(Song song)
    {
        return song switch
        {
            Song.WanderersMinuet => BardSettings.Instance.WandererSongDuration,
            Song.MagesBallad => BardSettings.Instance.MageSongDuration,
            Song.ArmysPaeon => BardSettings.Instance.ArmySongDuration,
            _ => 0
        };
    }

    public static bool IsSongReady(Song song)
    {
        var actionId = GetSpellBySong(song);
        return actionId != 0 && BardHelper.IsUnlocked(actionId) && actionId.GetActionCooldown() <= 0.05f;
    }

    public static bool RecentlyUsedAnySong(int span = 1000)
    {
        return BardHelper.RecentlyUsed(WanderersMinuet, span) ||
               BardHelper.RecentlyUsed(MagesBallad, span) ||
               BardHelper.RecentlyUsed(ArmysPaeon, span);
    }

    public static bool CanSwitchFromFirstToSecond()
    {
        return CurrentSong == BardSettings.Instance.FirstSong &&
               PromeSettings.Instance.GetQt(BRDQt.Song) &&
               SongTimerMs <= 45000f - GetSongDuration(BardSettings.Instance.FirstSong) * 1000f &&
               IsSongReady(BardSettings.Instance.SecondSong);
    }

    public static bool CanSwitchFromSecondToThird()
    {
        return CurrentSong == BardSettings.Instance.SecondSong &&
               PromeSettings.Instance.GetQt(BRDQt.Song) &&
               SongTimerMs <= 45000f - GetSongDuration(BardSettings.Instance.SecondSong) * 1000f &&
               IsSongReady(BardSettings.Instance.ThirdSong);
    }

    public static bool CanSwitchFromThirdToFirst()
    {
        return CurrentSong == BardSettings.Instance.ThirdSong &&
               PromeSettings.Instance.GetQt(BRDQt.Song) &&
               SongTimerMs <= 45000f - GetSongDuration(BardSettings.Instance.ThirdSong) * 1000f &&
               IsSongReady(BardSettings.Instance.FirstSong);
    }

    public static bool CanSwitchFromNone()
    {
        return CurrentSong == Song.None &&
               PromeSettings.Instance.GetQt(BRDQt.Song) &&
               (IsSongReady(BardSettings.Instance.FirstSong) ||
                IsSongReady(BardSettings.Instance.SecondSong) ||
                IsSongReady(BardSettings.Instance.ThirdSong));
    }

    public static uint GetNextSongAction()
    {
        var settings = BardSettings.Instance;
        var currentSong = CurrentSong;
        var lastSong = BardBattleData.Instance.LastSong;

        if ((currentSong == settings.ThirdSong || lastSong == settings.ThirdSong) &&
            IsSongReady(settings.FirstSong))
            return GetSpellBySong(settings.FirstSong);

        if ((currentSong == settings.FirstSong || lastSong == settings.FirstSong) && IsSongReady(settings.SecondSong))
            return GetSpellBySong(settings.SecondSong);

        if ((currentSong == settings.SecondSong || lastSong == settings.SecondSong) && IsSongReady(settings.ThirdSong))
            return GetSpellBySong(settings.ThirdSong);

        if (currentSong == Song.None)
        {
            if (lastSong == settings.FirstSong && IsSongReady(settings.SecondSong))
                return GetSpellBySong(settings.SecondSong);
            if (lastSong == settings.SecondSong && IsSongReady(settings.ThirdSong))
                return GetSpellBySong(settings.ThirdSong);
            if (lastSong == settings.ThirdSong && IsSongReady(settings.FirstSong))
                return GetSpellBySong(settings.FirstSong);
            if (IsSongReady(settings.FirstSong))
                return GetSpellBySong(settings.FirstSong);
            if (IsSongReady(settings.SecondSong))
                return GetSpellBySong(settings.SecondSong);
            if (IsSongReady(settings.ThirdSong))
                return GetSpellBySong(settings.ThirdSong);
        }

        return GetSpellBySong(settings.FirstSong);
    }

    public static bool ShouldSwitchSong(out uint songActionId, bool allowNoTarget = false)
    {
        return ShouldSwitchSong(out songActionId, out _, allowNoTarget);
    }

    public static bool ShouldSwitchSong(out uint songActionId, out string reason, bool allowNoTarget = false)
    {
        songActionId = 0;
        reason = string.Empty;

        if (!PromeSettings.Instance.GetQt(BRDQt.Song))
        {
            reason = "QT唱歌关闭";
            return false;
        }

        if (RecentlyUsedAnySong())
        {
            reason = "刚使用过歌曲";
            return false;
        }

        if (!allowNoTarget && Core.Core.Target == null)
        {
            reason = "当前无目标";
            return false;
        }

        var gcdRemainMs = ActionHelper.GetGcdRemain() * 1000f;
        if (!allowNoTarget && gcdRemainMs <= 530f)
        {
            reason = $"GCD窗口不足:{gcdRemainMs:F0}ms";
            return false;
        }

        return IsSongOrderNormal()
                   ? ShouldSwitchNormalSong(out songActionId, allowNoTarget, gcdRemainMs, out reason)
                   : ShouldSwitchCustomSong(out songActionId, out reason);
    }

    public static string SongDisplayName(Song song)
    {
        return song switch
        {
            Song.WanderersMinuet => "旅神",
            Song.MagesBallad => "贤者",
            Song.ArmysPaeon => "军神",
            _ => "无"
        };
    }

    private static bool ShouldSwitchCustomSong(out uint songActionId, out string reason)
    {
        songActionId = 0;
        reason = GetCustomSongBlockedReason();

        if (reason.Length > 0)
            return false;

        songActionId = GetNextSongAction();
        if (songActionId == 0)
        {
            reason = "无法解析下一首歌";
            return false;
        }

        reason = $"自定义歌轴切歌:{SongDisplayName(GetSongBySpell(songActionId))}";
        return true;
    }

    private static string GetCustomSongBlockedReason()
    {
        if (CanSwitchFromFirstToSecond() || CanSwitchFromSecondToThird() || CanSwitchFromThirdToFirst() || CanSwitchFromNone())
            return string.Empty;

        var settings = BardSettings.Instance;
        return CurrentSong switch
        {
            _ when CurrentSong == settings.FirstSong =>
                BuildSongWaitMessage(settings.FirstSong, settings.SecondSong),
            _ when CurrentSong == settings.SecondSong =>
                BuildSongWaitMessage(settings.SecondSong, settings.ThirdSong),
            _ when CurrentSong == settings.ThirdSong =>
                BuildSongWaitMessage(settings.ThirdSong, settings.FirstSong),
            Song.None => "无当前歌曲且下一首未就绪",
            _ => $"当前歌曲不在自定义歌轴:{CurrentSong}"
        };
    }

    private static string BuildSongWaitMessage(Song currentSong, Song nextSong)
    {
        var requiredTimerMs = 45000f - GetSongDuration(currentSong) * 1000f;
        if (SongTimerMs > requiredTimerMs)
            return $"{SongDisplayName(currentSong)}时间未到:{SongTimerMs:F0}>{requiredTimerMs:F0}ms";

        if (!IsSongReady(nextSong))
            return $"{SongDisplayName(nextSong)}未就绪:{GetSongCooldownMs(nextSong):F0}ms";

        return "自定义歌轴条件未满足";
    }

    private static bool ShouldSwitchNormalSong(out uint songActionId, bool allowNoTarget, float gcdRemainMs, out string reason)
    {
        songActionId = 0;
        reason = string.Empty;

        var settings = BardSettings.Instance;
        if (!allowNoTarget &&
            BRDSkill.EmpyrealArrow.GetActionCooldown() * 1000f < 1000f &&
            Core.Core.Me?.Level >= 90 &&
            PromeSettings.Instance.GetQt(BRDQt.EmpyrealArrow))
        {
            reason = $"等待九天连箭:{BRDSkill.EmpyrealArrow.GetActionCooldown() * 1000f:F0}ms";
            return false;
        }

        if (!IsSongReady(Song.MagesBallad) &&
            !IsSongReady(Song.ArmysPaeon) &&
            !IsSongReady(Song.WanderersMinuet))
        {
            reason = "三首歌均未就绪";
            return false;
        }

        if (CurrentSong == Song.WanderersMinuet &&
            SongTimerMs < 45000f - settings.WandererSongDuration * 1000f &&
            IsSongReady(Song.MagesBallad))
        {
            songActionId = BRDSkill.MagesBallad;
            reason = "旅神到时长, 切贤者";
            return true;
        }

        if (CurrentSong == Song.MagesBallad &&
            SongTimerMs < 45000f - settings.MageSongDuration * 1000f &&
            IsSongReady(Song.ArmysPaeon))
        {
            songActionId = BRDSkill.ArmysPaeon;
            reason = "贤者到时长, 切军神";
            return true;
        }

        if (CurrentSong == Song.ArmysPaeon &&
            (allowNoTarget || gcdRemainMs <= settings.WandererBeforeGcdTime) &&
            SongTimerMs < 45000f - settings.ArmySongDuration * 1000f &&
            IsSongReady(Song.WanderersMinuet) &&
            !PromeSettings.Instance.GetQt(BRDQt.BurstWithWanderer))
        {
            songActionId = BRDSkill.TheWanderersMinuet;
            reason = "军神到时长, 切旅神";
            return true;
        }

        if (CurrentSong == Song.None &&
            (IsSongReady(Song.WanderersMinuet) || IsSongReady(Song.MagesBallad) || IsSongReady(Song.ArmysPaeon)))
        {
            songActionId = GetNextSongAction();
            if (songActionId != 0)
            {
                reason = $"无歌曲, 起{SongDisplayName(GetSongBySpell(songActionId))}";
                return true;
            }
        }

        if (PromeSettings.Instance.GetQt(BRDQt.Burst) &&
            PromeSettings.Instance.GetQt(BRDQt.BurstWithWanderer) &&
            IsSongReady(Song.WanderersMinuet) &&
            (allowNoTarget || gcdRemainMs <= settings.WandererBeforeGcdTime) &&
            IsWandererBurstWindowReady())
        {
            songActionId = BRDSkill.TheWanderersMinuet;
            reason = "爆发对齐旅神";
            return true;
        }

        reason = GetNormalSongBlockedReason(gcdRemainMs);
        return false;
    }

    private static string GetNormalSongBlockedReason(float gcdRemainMs)
    {
        var settings = BardSettings.Instance;
        return CurrentSong switch
        {
            Song.WanderersMinuet => BuildSongWaitMessage(Song.WanderersMinuet, Song.MagesBallad),
            Song.MagesBallad => BuildSongWaitMessage(Song.MagesBallad, Song.ArmysPaeon),
            Song.ArmysPaeon when gcdRemainMs > settings.WandererBeforeGcdTime =>
                $"等旅神前置窗口:{gcdRemainMs:F0}>{settings.WandererBeforeGcdTime}ms",
            Song.ArmysPaeon when PromeSettings.Instance.GetQt(BRDQt.BurstWithWanderer) =>
                $"军神中, 等爆发旅神 战歌:{BRDSkill.BattleVoice.GetActionCooldown() * 1000f:F0}ms",
            Song.ArmysPaeon => BuildSongWaitMessage(Song.ArmysPaeon, Song.WanderersMinuet),
            Song.None => "无歌曲但没有可用歌曲",
            _ => $"未知歌曲状态:{CurrentSong}"
        };
    }

    private static float GetSongCooldownMs(Song song)
    {
        var actionId = GetSpellBySong(song);
        return actionId == 0 ? 0 : actionId.GetActionCooldown() * 1000f;
    }

    private static bool IsWandererBurstWindowReady()
    {
        var me = Core.Core.Me;
        if (me == null) return false;

        var battleVoiceMs = BRDSkill.BattleVoice.GetActionCooldown() * 1000f;
        var ragingMs = BRDSkill.RagingStrikes.GetActionCooldown() * 1000f;
        var finaleMs = BRDSkill.RadiantFinale.GetActionCooldown() * 1000f;

        if (me.Level < 90)
            return battleVoiceMs <= 1600f;

        return (battleVoiceMs <= 1600f && finaleMs <= 4400f) ||
               (ragingMs <= 2200f && battleVoiceMs <= 3750f);
    }
}

