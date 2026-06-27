using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardHeartBreakMaxChargeOffGcd : IDecisionResolver
{
    private const uint HeartBreak = BRDSkill.HeartBreak;
    private const uint RainOfDeath = BRDSkill.RainofDeath;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint EmpyrealArrow = BRDSkill.EmpyrealArrow;

    public CheckResult Check()
    {
        var actionId = BardHelper.Adjust(HeartBreak);
        var charges = ActionHelper.GetActionCharges(actionId);
        if (Core.Core.Target == null)
            return new CheckResult(false, "当前无目标");
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
        if (charges < 1f)
            return new CheckResult(false, $"碎心箭未就绪:{charges:F1}层");
        if (charges >= ActionHelper.GetMaxCharges(actionId) - 0.1f)
            return new CheckResult(true, "碎心箭满层");

        return new CheckResult(false, $"碎心箭层数未满:{charges:F1}");
    }

    public PAction GetAction()
    {
        return GetHeartBreakAction();
    }

    private static PAction GetHeartBreakAction()
    {
        if (TargetHelper.EnemyInRangeTarget(Core.Core.Target, 8) > 1 &&
            PromeSettings.Instance.GetQt(BRDQt.AOE) &&
            BardHelper.IsUnlocked(RainOfDeath))
            return new PAction(RainOfDeath, ActionType.OffGcd, ActionTargetType.Target);

        return new PAction(BardHelper.Adjust(HeartBreak), ActionType.OffGcd, ActionTargetType.Target);
    }
}

