using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardBattleVoiceOffGcd : IDecisionResolver
{
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "QT爆发关闭");

        if (PromeSettings.Instance.GetQt(BRDQt.BurstWithWanderer) &&
            BardHelper.CurrentSong != Song.WanderersMinuet)
            return new CheckResult(false, "等待旅神对齐爆发");

        if (!BardHelper.IsUnlocked(BattleVoice))
            return new CheckResult(false, "战斗之声未解锁");

        var cooldownMs = BattleVoice.GetActionCooldown() * 1000f;
        if (cooldownMs > 50f)
            return new CheckResult(false, $"战歌CD中:{cooldownMs:F0}ms");

        if (BardBattleData.Instance.First120SBuffSpellId == RagingStrikes &&
            RagingStrikes.GetActionCooldown() * 1000f < 2000f)
            return new CheckResult(false, $"等待猛者优先:{RagingStrikes.GetActionCooldown() * 1000f:F0}ms");

        if (!HasValid120Alignment(out var alignReason))
            return new CheckResult(false, alignReason);

        var gcdRemainMs = ActionHelper.GetGcdRemain() * 1000f;
        var useWindowMs = BardSettings.Instance.UseBattleVoiceBeforeGcdTimeInMs;
        if (gcdRemainMs >= useWindowMs)
            return new CheckResult(false, $"等待战歌窗口:{gcdRemainMs:F0}>={useWindowMs}ms");

        return new CheckResult(true, $"战歌就绪:{gcdRemainMs:F0}ms");
    }

    public PAction GetAction()
    {
        return new PAction(BattleVoice, ActionType.OffGcd, ActionTargetType.Self);
    }

    private static bool HasValid120Alignment(out string reason)
    {
        reason = string.Empty;
        var data = BardBattleData.Instance;
        if (data.First120SBuffSpellId != BattleVoice)
            return true;

        var waitLimitMs = ActionHelper.GetGcdTotal() * 1000f
                          + BardSettings.Instance.UseBattleVoiceBeforeGcdTimeInMs
                          - BardSettings.Instance.RagingStrikeBeforeGcdTime;
        var secondMs = GetCooldownMs(data.Second120SBuffSpellId);
        var thirdMs = GetCooldownMs(data.Third120SBuffSpellId);

        if (secondMs <= waitLimitMs && thirdMs <= waitLimitMs)
            return true;

        reason = $"等待后续120技能:{Math.Max(secondMs, thirdMs):F0}>{waitLimitMs:F0}ms";
        return false;
    }

    private static float GetCooldownMs(uint actionId)
    {
        return actionId == 0 ? 0f : actionId.GetActionCooldown() * 1000f;
    }
}

