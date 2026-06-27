using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardApexGcd : IDecisionResolver
{
    private const uint ApexArrow = BRDSkill.ApexArrow;
    private const uint RagingStrikesBuff = BRDBuff.RagingStrikes;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.Apex))
            return new CheckResult(false, "绝峰箭QT关闭");
        if (!BardHelper.IsUnlocked(ApexArrow))
            return new CheckResult(false, "绝峰箭未解锁");
        if (BardHelper.RecentlyUsed(ApexArrow, 5000))
            return new CheckResult(false, "绝峰箭刚使用过");

        var soul = JobGaugeHelper.BRD.GetSoulVoice;

        if (BardSettings.Instance.IsDailyMode && soul >= 90)
            return new CheckResult(true, $"日常模式灵魂之声:{soul}");

        if (!PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "爆发QT关闭，交给关闭爆发绝峰逻辑");

        var partyBuffCountdown = BardBattleData.Instance.First120SBuffSpellId.GetActionCooldown();
        if (BardHelper.HasNoPartyBuff())
        {
            if (soul == 100 &&
                partyBuffCountdown > 53 &&
                !BardBattleData.Instance.HasUseApexArrowInCurrentNonBurstingPeriod)
                return new CheckResult(true, $"非团辅灵魂之声满，团辅CD:{partyBuffCountdown:F1}s");

            if (soul >= 80 &&
                partyBuffCountdown is > 43 and <= 53 &&
                !BardBattleData.Instance.HasUseApexArrowInCurrentNonBurstingPeriod)
                return new CheckResult(true, $"非团辅80灵魂之声窗口，团辅CD:{partyBuffCountdown:F1}s");

            if (soul >= 40 &&
                partyBuffCountdown is <= 43 and >= 39 &&
                !BardBattleData.Instance.HasUseApexArrowInCurrentNonBurstingPeriod)
                return new CheckResult(true, $"团辅前低灵魂之声倾泻，灵魂之声:{soul}");
        }

        if (BardHelper.HasAllPartyBuff())
        {
            BardBattleData.Instance.HasUseApexArrowInCurrentNonBurstingPeriod = false;

            if (soul == 100)
                return new CheckResult(true, "爆发期灵魂之声满");
            if (soul >= 80)
            {
                if (BardHelper.HasSelfStatusWithTimeLeft(RagingStrikesBuff, 8000))
                    return new CheckResult(false, "猛者剩余充足，等待更晚绝峰");

                return new CheckResult(true, $"爆发期80灵魂之声，猛者小于8秒或不存在:{soul}");
            }
        }

        return new CheckResult(false, $"绝峰条件不满足 灵魂之声:{soul} 团辅CD:{partyBuffCountdown:F1}s");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(ApexArrow), ActionType.Gcd, ActionTargetType.Target);
    }
}

