using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardSidewinderOffGcd : IDecisionResolver
{
    private const uint Sidewinder = BRDSkill.Sidewinder;
    private const uint EmpyrealArrow = BRDSkill.EmpyrealArrow;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint BattleVoice = BRDSkill.BattleVoice;

    public CheckResult Check()
    {
        if (Core.Core.Target == null)
            return new CheckResult(false, "当前无目标");
        if (!PromeSettings.Instance.GetQt(BRDQt.Sidewinder))
            return new CheckResult(false, "侧风诱导箭QT关闭");
        if (ActionHelper.GetGcdRemain() * 1000f <= 650f)
            return new CheckResult(false, "GCD窗口不足");
        if (EmpyrealArrow.GetActionCooldown() * 1000f < 650f &&
            PromeSettings.Instance.GetQt(BRDQt.EmpyrealArrow) &&
            BardHelper.IsUnlocked(EmpyrealArrow))
            return new CheckResult(false, "等待九天连箭");
        if (RagingStrikes.GetActionCooldown() * 1000f < 3000f &&
            PromeSettings.Instance.GetQt(BRDQt.Burst) &&
            BardHelper.IsUnlocked(RagingStrikes))
            return new CheckResult(false, "等待猛者强击");
        if (BattleVoice.GetActionCooldown() * 1000f < 3000f &&
            PromeSettings.Instance.GetQt(BRDQt.Burst) &&
            BardHelper.IsUnlocked(BattleVoice))
            return new CheckResult(false, "等待战斗之声");
        if (!BardHelper.IsUnlockedWithCdCheck(Sidewinder))
            return new CheckResult(false, $"侧风诱导箭未就绪:{Sidewinder.GetActionCooldown() * 1000f:F0}ms");

        return new CheckResult(true, "侧风诱导箭");
    }

    public PAction GetAction()
    {
        return new PAction(Sidewinder, ActionType.OffGcd, ActionTargetType.Target);
    }
}

