using PromeRotation.Data;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardBlastArrowMaxGcd : IDecisionResolver
{
    private const uint BlastArrow = BRDSkill.BlastArrow;
    private const uint BlastArrowReady = BRDBuff.BlastArrowReady;

    public CheckResult Check()
    {
        if (!BardHelper.IsUnlocked(BlastArrow))
            return new CheckResult(false, "爆破箭未解锁");

        if (BardHelper.HasSelfStatus(BlastArrowReady) &&
            !BardHelper.HasSelfStatusWithTimeLeft(BlastArrowReady, 3000))
            return new CheckResult(true, "爆破箭预备即将过期");

        return new CheckResult(false, "爆破箭预备时间充足或不存在");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(BlastArrow), ActionType.Gcd, ActionTargetType.Target);
    }
}

