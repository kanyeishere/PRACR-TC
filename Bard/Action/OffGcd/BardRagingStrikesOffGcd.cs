using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardRagingStrikesOffGcd : IDecisionResolver
{
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint RadiantFinale = BRDSkill.RadiantFinale;
    private const float PromeDecisionPaddingMs = 100f;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "QT爆发关闭");

        if (PromeSettings.Instance.GetQt(BRDQt.BurstWithWanderer) &&
            BardHelper.CurrentSong != Song.WanderersMinuet)
            return new CheckResult(false, "等待旅神对齐爆发");

        if (!BardHelper.IsUnlocked(RagingStrikes))
            return new CheckResult(false, "猛者强击未解锁");

        var cooldownMs = RagingStrikes.GetActionCooldown() * 1000f;
        if (cooldownMs > 50f)
            return new CheckResult(false, $"猛者CD中:{cooldownMs:F0}ms");

        if (!CanUseRagingBelowLevel90(out var lowLevelReason))
            return new CheckResult(false, lowLevelReason);

        if (!HasValid120Alignment(out var alignReason))
            return new CheckResult(false, alignReason);

        var gcdRemainMs = ActionHelper.GetGcdRemain() * 1000f;
        var useWindowMs = BardSettings.Instance.RagingStrikeBeforeGcdTime + PromeDecisionPaddingMs;
        if (gcdRemainMs > useWindowMs)
            return new CheckResult(false, $"等待猛者窗口:{gcdRemainMs:F0}>{useWindowMs:F0}ms");

        return new CheckResult(true, $"猛者就绪:{gcdRemainMs:F0}ms");
    }

    public PAction GetAction()
    {
        return new PAction(RagingStrikes, ActionType.OffGcd, ActionTargetType.Self);
    }

    private static bool CanUseRagingBelowLevel90(out string reason)
    {
        reason = string.Empty;
        var me = Core.Core.Me;
        if (me == null)
        {
            reason = "玩家数据为空";
            return false;
        }

        if (me.Level >= 90 || BardSettings.Instance.IsDailyMode || !BardHelper.IsUnlocked(BattleVoice))
            return true;

        if (BardHelper.RecentlyUsed(BattleVoice, 2500) || BardHelper.HasSelfStatus(BRDBuff.BattleVoice))
            return true;

        reason = "90级以下高难:等待战歌后猛者";
        return false;
    }

    private static bool HasValid120Alignment(out string reason)
    {
        reason = string.Empty;
        var data = BardBattleData.Instance;
        var gcdTotalMs = ActionHelper.GetGcdTotal() * 1000f;

        if (data.First120SBuffSpellId == RagingStrikes &&
            (GetCooldownMs(data.Second120SBuffSpellId) > gcdTotalMs - 650f ||
             GetCooldownMs(data.Third120SBuffSpellId) > gcdTotalMs))
        {
            reason = $"等待后续120技能 战歌:{GetCooldownMs(BattleVoice):F0}ms 光明:{GetCooldownMs(RadiantFinale):F0}ms";
            return false;
        }

        if (data.First120SBuffSpellId == BattleVoice &&
            BardHelper.IsUnlocked(BattleVoice) &&
            BattleVoice.GetActionCooldown() * 1000f < 2000f)
        {
            reason = $"战歌即将就绪:{BattleVoice.GetActionCooldown() * 1000f:F0}ms";
            return false;
        }

        return true;
    }

    private static float GetCooldownMs(uint actionId)
    {
        return actionId == 0 ? 0f : actionId.GetActionCooldown() * 1000f;
    }
}

