using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardDotGcd : IDecisionResolver
{
    private const uint VenomousBite = BRDSkill.VenomousBite;
    private const uint WindBite = BRDSkill.Windbite;
    private const uint RefulgentArrow = BRDSkill.RefulgentArrow;
    private const uint Shadowbite = BRDSkill.Shadowbite;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint BattleVoice = BRDSkill.BattleVoice;

    private const uint HawkEyeBuff = BRDBuff.HawksEye;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.DOT))
            return new CheckResult(false, "DOT QT关闭");

        var target = Core.Core.Target;
        if (target == null)
            return new CheckResult(false, "当前无目标");
        if (BardBattleData.Instance.DotBlackList.Contains(target.BaseId))
            return new CheckResult(false, "目标在战斗DOT黑名单");
        if (!BardHelper.IsBoss(target) &&
            !BardSettings.Instance.ApplyDotOnTrashMobs &&
            BardSettings.Instance.IsDailyMode)
            return new CheckResult(false, "日随模式小怪不打DOT");

        if (!BardHelper.HasAnyDot(target, BardHelper.WindDotBuffs) &&
            BardHelper.IsUnlocked(WindBite))
            return new CheckResult(true, "补风DOT");

        if (!BardHelper.HasAnyDot(target, BardHelper.PoisonDotBuffs) &&
            BardHelper.IsUnlocked(VenomousBite))
            return new CheckResult(true, "补毒DOT");

        return new CheckResult(false, "DOT已存在");
    }

    public PAction GetAction()
    {
        return GetActionFromAeLogic();
    }

    private PAction GetActionFromAeLogic()
    {
        if (PromeSettings.Instance.GetQt(BRDQt.ClearHawkEyesBuffBeforeDots))
        {
            if (BardHelper.HasSelfStatus(HawkEyeBuff) &&
                !BardHelper.IsUnlockedWithCdCheck(RagingStrikes) &&
                !BardHelper.IsUnlockedWithCdCheck(BattleVoice))
            {
                if (TargetHelper.EnemyInRangeTarget(Core.Core.Target, 5) > 1 &&
                    PromeSettings.Instance.GetQt(BRDQt.AOE) &&
                    BardHelper.IsUnlocked(Shadowbite))
                    return new PAction(BardHelper.Adjust(Shadowbite), ActionType.Gcd, ActionTargetType.Target);

                return new PAction(BardHelper.Adjust(RefulgentArrow), ActionType.Gcd, ActionTargetType.Target);
            }

            if (BardHelper.HasSelfStatus(HawkEyeBuff) &&
                !PromeSettings.Instance.GetQt(BRDQt.Burst))
            {
                if (TargetHelper.EnemyInRangeTarget(Core.Core.Target, 5) > 1 &&
                    PromeSettings.Instance.GetQt(BRDQt.AOE) &&
                    BardHelper.IsUnlocked(Shadowbite))
                    return new PAction(BardHelper.Adjust(Shadowbite), ActionType.Gcd, ActionTargetType.Target);

                return new PAction(BardHelper.Adjust(RefulgentArrow), ActionType.Gcd, ActionTargetType.Target);
            }
        }

        var target = Core.Core.Target;
        return !BardHelper.HasAnyDot(target, BardHelper.WindDotBuffs) &&
               BardHelper.IsUnlocked(WindBite)
            ? new PAction(BardHelper.Adjust(WindBite), ActionType.Gcd, ActionTargetType.Target)
            : new PAction(BardHelper.Adjust(VenomousBite), ActionType.Gcd, ActionTargetType.Target);
    }
}

