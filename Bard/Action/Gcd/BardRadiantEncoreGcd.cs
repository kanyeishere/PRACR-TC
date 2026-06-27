using PromeRotation.Data;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardRadiantEncoreGcd : IDecisionResolver
{
    private const uint RadiantEncore = BRDSkill.RadiantEncore;
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint RadiantEncoreReady = BRDBuff.RadiantEncoreReady;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "爆发QT关闭");
        if (!BardHelper.IsUnlocked(RadiantEncore))
            return new CheckResult(false, "返场余音未解锁");

        if (BardHelper.RecentlyUsed(RagingStrikes, 25000) &&
            BardHelper.RecentlyUsed(BattleVoice, 25000) &&
            BardHelper.HasSelfStatus(RadiantEncoreReady))
            return new CheckResult(true, "团辅后返场余音预备");

        if (BardHelper.RecentlyUsed(RagingStrikes, 25000) &&
            BardHelper.RecentlyUsed(BattleVoice, 25000) &&
            BardHelper.IsUnlockedWithCdCheck(RadiantEncore))
            return new CheckResult(true, "团辅后返场余音可用");

        return new CheckResult(false, "返场余音条件不满足");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(RadiantEncore), ActionType.Gcd, ActionTargetType.Target);
    }
}

