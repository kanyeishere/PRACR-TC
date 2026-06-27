using PromeRotation.Data;
using PromeRotation.Managers;

namespace WotouTC.Bard;

public static class BardNoTargetSongSwitcher
{
    private const int AttemptThrottleMs = 1000;

    private static long _lastAttemptTime;
    private static bool _lastStatus;
    private static string _lastMessage = "尚未检查";

    public static void MarkOutOfBattle()
    {
        SetStatus(false, "非战斗不处理无目标切歌");
    }

    public static void TrySwitch()
    {
        if (!CanAttempt(out var blockedReason))
        {
            SetStatus(false, blockedReason);
            return;
        }

        if (!BardSongHelper.ShouldSwitchSong(out var songActionId, out var reason, allowNoTarget: true))
        {
            SetStatus(false, reason);
            return;
        }

        _lastAttemptTime = Environment.TickCount64;
        Updaters.ActionUpdater.UseAction4Receiver(songActionId, ActionType.OffGcd, 0);
        SetStatus(true, $"已发送无目标切歌:{BardSongHelper.SongDisplayName(BardSongHelper.GetSongBySpell(songActionId))} targetId=0");
    }

    public static SolverStatus GetStatus()
    {
        return new SolverStatus
        {
            Name = "NoTargetSongSwitch",
            Success = _lastStatus,
            Message = _lastMessage
        };
    }

    private static bool CanAttempt(out string reason)
    {
        if (PromeSettings.Instance.EnableAcr != AcrState.On)
        {
            reason = "ACR未开启";
            return false;
        }

        var me = Core.Core.Me;
        if (me == null)
        {
            reason = "本地玩家为空";
            return false;
        }

        if (me.IsDead)
        {
            reason = "玩家死亡";
            return false;
        }

        if (Core.Core.Target != null)
        {
            reason = "当前有目标, 交给oGCD求解器";
            return false;
        }

        if (Environment.TickCount64 - _lastAttemptTime < AttemptThrottleMs)
        {
            reason = "无目标切歌节流中";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static void SetStatus(bool success, string message)
    {
        _lastStatus = success;
        _lastMessage = message;
    }
}

