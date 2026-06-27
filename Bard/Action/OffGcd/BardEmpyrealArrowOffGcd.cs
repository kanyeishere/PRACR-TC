using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardEmpyrealArrowOffGcd : IDecisionResolver
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

        if (PromeSettings.Instance.GetQt(BRDQt.Burst) && BattleVoice.GetActionCooldown() * 1000f < 3000f)
            return new CheckResult(false, $"爆发前保留九天:战歌{BattleVoice.GetActionCooldown() * 1000f:F0}ms");

        /*if (PromeSettings.Instance.GetQt(BRDQt.Burst) && RagingStrikes.GetActionCooldown() * 1000f < 3000f)
            return new CheckResult(false, $"爆发前保留九天:猛者{RagingStrikes.GetActionCooldown() * 1000f:F0}ms");
            */

        /*if (EngageManager.GetBattleTime() < 3)
            return new CheckResult(false, $"开场3秒内不打九天:{EngageManager.GetBattleTime():F1}s");
            */

        if (!BardHelper.IsUnlocked(EmpyrealArrow))
            return new CheckResult(false, "九天未解锁");
        
        var cooldownMs = EmpyrealArrow.GetActionCooldown() * 1000f;
        if (cooldownMs <= 50f)
            return new CheckResult(true, "九天就绪");

        var gcdRemainMs = ActionHelper.GetGcdRemain() * 1000f;
        if (cooldownMs <= 400f &&
            gcdRemainMs < 400f &&
            cooldownMs - gcdRemainMs < 350f)
            return new CheckResult(true, $"九天即将就绪:{cooldownMs:F0}ms GCD:{gcdRemainMs:F0}ms");

        return new CheckResult(false, $"九天CD中:{cooldownMs:F0}ms");
    }

    public PAction GetAction()
    {
        return new PAction(EmpyrealArrow, ActionType.OffGcd, ActionTargetType.Target);
    }
}

