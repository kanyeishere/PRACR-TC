using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using WotouTC.Bard;
using WotouTC.Bard.Data;

namespace Wotou.Bard.Action;

/// <summary>
/// 为了解决玩家死亡后复活，重新进入战斗时，九天连箭与旅神歌同时CD预备，此时无法正确判断技能释放顺序的问题
/// </summary>
public class BurstingAfterDeathSequence : IDecisionResolver
{
    private const uint EmpyrealArrow = BRDSkill.EmpyrealArrow;
    private const uint WanderersMinuet = BRDSkill.TheWanderersMinuet;
    private const uint StormBite = BRDSkill.Stormbite;
    private const uint CausticBite = BRDSkill.CausticBite;

    public CheckResult Check()
    {
        var target = Core.Core.Target;
        if (target == null)
            return new CheckResult(false, "当前无目标");

        // 战斗初期不触发（避免与正常起手冲突）
        if (EngageManager.GetBattleTime() < 5f)
            return new CheckResult(false, "战斗时间小于5秒");

        // 九天连箭与旅神歌必须同时CD预备
        if (!BardHelper.IsUnlocked(EmpyrealArrow))
            return new CheckResult(false, "九天连箭未解锁");
        if (!BardHelper.IsUnlocked(WanderersMinuet))
            return new CheckResult(false, "旅神歌未解锁");

        if (EmpyrealArrow.GetActionCooldown() > 0.05f)
            return new CheckResult(false, "九天连箭CD中");
        if (WanderersMinuet.GetActionCooldown() > 0.05f)
            return new CheckResult(false, "旅神歌CD中");

        // QT检查
        if (!PromeSettings.Instance.GetQt(BRDQt.BurstWithWanderer))
            return new CheckResult(false, "QT:对齐旅神关闭");
        if (!PromeSettings.Instance.GetQt(BRDQt.Song))
            return new CheckResult(false, "QT:唱歌关闭");
        if (!PromeSettings.Instance.GetQt(BRDQt.EmpyrealArrow))
            return new CheckResult(false, "QT:九天关闭");
        
        return new CheckResult(true, "复活爆发序列:旅神+补DOT");
    }

    public PAction GetAction()
    {
        var actions = BuildSequence();

        ActionQueueManager.Enqueue(actions);

        return null;
    }

    private static List<PAction> BuildSequence()
    {
        var actions = new List<PAction>
        {
            // 第1步：先开旅神歌（能力技）
            new (WanderersMinuet, ActionType.OffGcd, ActionTargetType.Target),
        };

        // 第2步：判断当前GCD应该打什么
        actions.Add(GetGcdAction());

        return actions;
    }

    private static PAction GetGcdAction()
    {
        var target = Core.Core.Target;
        
        // 1) 目标在DOT黑名单 → 打基础GCD
        if (BardBattleData.Instance.DotBlackList.Contains(target.DataId))
            return BardHelper.GetBaseGcd();

        // 2) 目标缺少风DOT → 补风毒
        if (!BardHelper.HasAnyDot(target, BardHelper.WindDotBuffs) &&
            PromeSettings.Instance.GetQt(BRDQt.DOT))
            return new PAction(BardHelper.Adjust(StormBite), ActionType.Gcd, ActionTargetType.Target);

        // 3) 目标缺少毒DOT → 补毒
        if (!BardHelper.HasAnyDot(target, BardHelper.PoisonDotBuffs) &&
            PromeSettings.Instance.GetQt(BRDQt.DOT))
            return new PAction(BardHelper.Adjust(CausticBite), ActionType.Gcd, ActionTargetType.Target);

        // 4) 默认打基础GCD
        return BardHelper.GetBaseGcd();
    }
}
