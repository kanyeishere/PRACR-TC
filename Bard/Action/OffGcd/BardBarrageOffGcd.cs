using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardBarrageOffGcd : IDecisionResolver
{
    private const uint Barrage = BRDSkill.Barrage;
    private const uint EmpyrealArrow = BRDSkill.EmpyrealArrow;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint BattleVoice = BRDSkill.BattleVoice;

    public CheckResult Check()
    {
        if (ActionHelper.GetGcdRemain() * 1000f <= 620f)
            return new CheckResult(false, "GCD窗口不足");
        if (!BardHelper.IsUnlockedWithCdCheck(Barrage))
            return new CheckResult(false, $"纷乱箭未就绪:{Barrage.GetActionCooldown() * 1000f:F0}ms");
        if (!PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "爆发QT关闭");
        if (EmpyrealArrow.GetActionCooldown() * 1000f < 650f &&
            PromeSettings.Instance.GetQt(BRDQt.EmpyrealArrow) &&
            BardHelper.IsUnlocked(EmpyrealArrow))
            return new CheckResult(false, "等待九天连箭");
        if (RagingStrikes.GetActionCooldown() * 1000f < 3000f &&
            BardHelper.IsUnlocked(RagingStrikes))
            return new CheckResult(false, "等待猛者强击");
        if (BattleVoice.GetActionCooldown() * 1000f < 3000f &&
            BardHelper.IsUnlocked(BattleVoice))
            return new CheckResult(false, "等待战斗之声");
        if (BardHelper.HasAllPartyBuff())
            return new CheckResult(true, "团辅齐全纷乱箭");

        return new CheckResult(false, "团辅未齐");
    }

    public PAction GetAction()
    {
        return new PAction(Barrage, ActionType.OffGcd, ActionTargetType.Self);
    }
}

