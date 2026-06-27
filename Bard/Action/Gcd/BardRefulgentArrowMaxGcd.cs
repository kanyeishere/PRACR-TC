using PromeRotation.Data;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardRefulgentArrowMaxGcd : IDecisionResolver
{
    private const uint BarrageBuff = BRDBuff.Barrage;
    private const uint RefulgentArrow = BRDSkill.RefulgentArrow;

    public CheckResult Check()
    {
        if (!BardHelper.IsUnlocked(RefulgentArrow))
            return new CheckResult(false, "辉煌箭未解锁");

        if (BardHelper.HasSelfStatus(BarrageBuff) &&
            !BardHelper.HasSelfStatusWithTimeLeft(BarrageBuff, 3000))
            return new CheckResult(true, "纷乱箭即将过期，强制辉煌箭");

        return new CheckResult(false, "纷乱箭时间充足或不存在");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(RefulgentArrow), ActionType.Gcd, ActionTargetType.Target);
    }
}

