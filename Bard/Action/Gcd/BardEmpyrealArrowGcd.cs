using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardEmpyrealArrowGcd : IDecisionResolver
{
    private const uint EmpyrealArrow = BRDSkill.EmpyrealArrow;
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;

    public CheckResult Check()
    {
        if (Core.Core.Target == null)
            return new CheckResult(false, "当前无目标");

        if (!PromeSettings.Instance.GetQt(BRDQt.EmpyrealArrow))
            return new CheckResult(false, "QT九天连箭关闭");

        if (!PromeSettings.Instance.GetQt(BRDQt.StrongAlignment))
            return new CheckResult(false, "QT强对齐关闭");

        if (BardHelper.HasAnyPartyBuff())
            return new CheckResult(false, "团辅期不强插九天");

        if (PromeSettings.Instance.GetQt(BRDQt.Burst) && BattleVoice.GetActionCooldown() * 1000f < 3000f)
            return new CheckResult(false, $"爆发前保留九天:战歌{BattleVoice.GetActionCooldown() * 1000f:F0}ms");

        if (PromeSettings.Instance.GetQt(BRDQt.Burst) && RagingStrikes.GetActionCooldown() * 1000f < 3000f)
            return new CheckResult(false, $"爆发前保留九天:猛者{RagingStrikes.GetActionCooldown() * 1000f:F0}ms");

        if (!BardHelper.IsUnlocked(EmpyrealArrow))
            return new CheckResult(false, "九天未解锁");

        var cooldownMs = EmpyrealArrow.GetActionCooldown() * 1000f;
        var gcdRemainMs = ActionHelper.GetGcdRemain() * 1000f;

        if (cooldownMs <= 150f)
            return new CheckResult(true, "强插九天:已就绪");

        if (cooldownMs <= 400f &&
            gcdRemainMs < 400f &&
            cooldownMs - gcdRemainMs < 350f)
            return new CheckResult(true, $"强插九天:{cooldownMs:F0}ms GCD:{gcdRemainMs:F0}ms");

        return new CheckResult(false, $"九天未到强插窗口:{cooldownMs:F0}ms");
    }

    public PAction GetAction()
    {
        return new PAction(EmpyrealArrow, ActionType.Gcd, ActionTargetType.Target);
    }
}

