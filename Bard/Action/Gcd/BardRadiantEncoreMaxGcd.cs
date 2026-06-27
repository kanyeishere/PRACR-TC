using PromeRotation.Data;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardRadiantEncoreMaxGcd : IDecisionResolver
{
    private const uint RadiantEncore = BRDSkill.RadiantEncore;
    private const uint RadiantEncoreReady = BRDBuff.RadiantEncoreReady;

    public CheckResult Check()
    {
        if (!BardHelper.IsUnlocked(RadiantEncore))
            return new CheckResult(false, "返场余音未解锁");

        if (BardHelper.HasSelfStatus(RadiantEncoreReady) &&
            !BardHelper.HasSelfStatusWithTimeLeft(RadiantEncoreReady, 6000))
            return new CheckResult(true, "返场余音预备即将过期");

        return new CheckResult(false, "返场余音预备时间充足或不存在");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(RadiantEncore), ActionType.Gcd, ActionTargetType.Target);
    }
}

