using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardNaturesMinneOffGcd : IDecisionResolver
{
    private const uint NaturesMinne = BRDSkill.NaturesMinne;
    private const uint EmpyrealArrow = BRDSkill.EmpyrealArrow;
    private const uint Recitation = 1896;
    private const uint Zoe = 2611;
    private const uint NeutralSect = 1892;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.NatureMinne))
            return new CheckResult(false, "大地神QT关闭");

        if (NaturesMinne.GetActionCooldown() > 0.05f)
            return new CheckResult(false, $"大地神未就绪:{NaturesMinne.GetActionCooldown() * 1000f:F0}ms");

        if (ActionHelper.GetGcdRemain() * 1000f <= 650f)
            return new CheckResult(false, "GCD窗口不足");

        if (EmpyrealArrow.GetActionCooldown() * 1000f < 700f &&
            PromeSettings.Instance.GetQt(BRDQt.EmpyrealArrow) &&
            BardHelper.IsUnlocked(EmpyrealArrow))
            return new CheckResult(false, "等待九天连箭");

        // 检查队友是否有秘策/活化/中间学派
        var party = PartyHelper.GetUIParty();
        if (party.Any(m => m.HasStatus(Recitation) || m.HasStatus(Zoe) || m.HasStatus(NeutralSect)))
            return new CheckResult(true, "大地神对齐治疗Buff");

        return new CheckResult(false, "无可用治疗Buff对齐");
    }

    public PAction GetAction()
    {
        return new PAction(NaturesMinne, ActionType.OffGcd, ActionTargetType.Target);
    }
}
