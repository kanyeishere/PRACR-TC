using PromeRotation.Data;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardResonantArrowMaxGcd : IDecisionResolver
{
    private const uint ResonantArrow = BRDSkill.ResonantArrow;
    private const uint ResonantArrowReady = BRDBuff.ResonantArrowReady;
    private const uint BarrageBuff = BRDBuff.Barrage;

    public CheckResult Check()
    {
        if (!BardHelper.IsUnlocked(ResonantArrow))
            return new CheckResult(false, "共鸣箭未解锁");

        if (BardHelper.HasSelfStatus(BarrageBuff) &&
            !BardHelper.HasSelfStatusWithTimeLeft(BarrageBuff, 3000))
            return new CheckResult(false, "纷乱箭即将过期");

        if (BardHelper.HasSelfStatus(ResonantArrowReady) &&
            !BardHelper.HasSelfStatusWithTimeLeft(ResonantArrowReady, 6000))
            return new CheckResult(true, "共鸣箭预备即将过期");

        return new CheckResult(false, "共鸣箭预备时间充足或不存在");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(ResonantArrow), ActionType.Gcd, ActionTargetType.Target);
    }
}

