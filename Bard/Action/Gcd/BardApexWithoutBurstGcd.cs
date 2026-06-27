using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardApexWithoutBurstGcd : IDecisionResolver
{
    private const uint ApexArrow = BRDSkill.ApexArrow;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.Apex))
            return new CheckResult(false, "绝峰箭QT关闭");
        if (!BardHelper.IsUnlocked(ApexArrow))
            return new CheckResult(false, "绝峰箭未解锁");
        if (BardHelper.RecentlyUsed(ApexArrow, 5000))
            return new CheckResult(false, "绝峰箭刚使用过");
        if (PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "爆发QT开启，交给普通绝峰逻辑");
        if (JobGaugeHelper.BRD.GetSoulVoice == 100)
            return new CheckResult(true, "爆发QT关闭且灵魂之声满");

        return new CheckResult(false, $"灵魂之声不足:{JobGaugeHelper.BRD.GetSoulVoice}");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(ApexArrow), ActionType.Gcd, ActionTargetType.Target);
    }
}

