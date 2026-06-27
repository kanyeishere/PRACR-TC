using PromeRotation.Data;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardResonantArrowGcd : IDecisionResolver
{
    private const uint ResonantArrow = BRDSkill.ResonantArrow;
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint ResonantArrowReady = BRDBuff.ResonantArrowReady;
    private const uint BarrageBuff = BRDBuff.Barrage;

    public CheckResult Check()
    {
        if (!BardHelper.IsUnlocked(ResonantArrow))
            return new CheckResult(false, "共鸣箭未解锁");
        if (!BardHelper.HasSelfStatus(ResonantArrowReady))
            return new CheckResult(false, "没有共鸣箭预备");
        if (BardHelper.HasSelfStatus(BarrageBuff) &&
            !BardHelper.HasSelfStatusWithTimeLeft(BarrageBuff, 3000))
            return new CheckResult(false, "纷乱箭即将过期");

        if (BardHelper.HasAllPartyBuff())
            return new CheckResult(true, "团辅齐全共鸣箭");

        if (BardHelper.RecentlyUsed(RagingStrikes, 30000) &&
            BardHelper.RecentlyUsed(BattleVoice, 30000))
            return new CheckResult(true, "团辅后共鸣箭");

        if (!BardHelper.HasSelfStatusWithTimeLeft(ResonantArrowReady, 10000))
            return new CheckResult(true, "共鸣箭预备小于10秒");

        return new CheckResult(false, "共鸣箭等待团辅或过期保护");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(ResonantArrow), ActionType.Gcd, ActionTargetType.Target);
    }
}

