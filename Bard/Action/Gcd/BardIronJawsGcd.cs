using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardIronJawsGcd : IDecisionResolver
{
    private const uint IronJaws = BRDSkill.IronJaws;

    private const uint CausticBiteDot = BRDBuff.CausticBite;
    private const uint StormBiteDot = BRDBuff.Stormbite;
    private const uint VenomousBiteDot = BRDBuff.VenomousBite;
    private const uint WindBiteDot = BRDBuff.Windbite;

    private const uint BattleVoiceBuff = BRDBuff.BattleVoice;
    private const uint RagingStrikesBuff = BRDBuff.RagingStrikes;
    private const uint HawksEyeBuff = BRDBuff.HawksEye;
    private const uint BarrageBuff = BRDBuff.Barrage;
    private const uint RadiantEncoreReady = BRDBuff.RadiantEncoreReady;
    private const uint ResonantArrowReady = BRDBuff.ResonantArrowReady;

    public CheckResult Check()
    {
        if (BardHelper.RecentlyUsed(IronJaws, 2500))
            return new CheckResult(false, "IronJaw刚使用过");
        if (!PromeSettings.Instance.GetQt(BRDQt.DOT))
            return new CheckResult(false, "DOT QT关闭");
        if (!BardHelper.IsUnlocked(IronJaws))
            return new CheckResult(false, "铁颚未解锁");

        var target = Core.Core.Target;
        if (target == null)
            return new CheckResult(false, "当前无目标");
        if (BardBattleData.Instance.DotBlackList.Contains(target.BaseId))
            return new CheckResult(false, "目标在战斗DOT黑名单");
        if (!BardHelper.HasAnyDot(target, BardHelper.WindDotBuffs) ||
            !BardHelper.HasAnyDot(target, BardHelper.PoisonDotBuffs))
            return new CheckResult(false, "DOT不完整，交给上DOT");

        if (BardHelper.HasSelfStatus(BarrageBuff) &&
            !BardHelper.HasSelfStatusWithTimeLeft(BarrageBuff, 3000))
            return new CheckResult(false, "纷乱箭即将过期");
        if (BardHelper.HasSelfStatus(RadiantEncoreReady) &&
            !BardHelper.HasSelfStatusWithTimeLeft(RadiantEncoreReady, 3000))
            return new CheckResult(false, "返场余音预备即将过期");
        if (BardHelper.HasSelfStatus(ResonantArrowReady) &&
            !BardHelper.HasSelfStatusWithTimeLeft(ResonantArrowReady, 3000))
            return new CheckResult(false, "共鸣箭预备即将过期");

        if (BardHelper.HasAllPartyBuff() &&
            !BardBattleData.Instance.HasUseIronJawsInCurrentBursting)
        {
            if (!BardHelper.HasSelfStatusWithTimeLeft(BattleVoiceBuff, 3000) ||
                !BardHelper.HasSelfStatusWithTimeLeft(RagingStrikesBuff, 3000))
                return new CheckResult(true, "爆发期强制截毒");

            if (!BardHelper.HasSelfStatusWithTimeLeft(BattleVoiceBuff, 10000) ||
                !BardHelper.HasSelfStatusWithTimeLeft(RagingStrikesBuff, 10000))
            {
                if (BardHelper.HasSelfStatus(HawksEyeBuff))
                    return new CheckResult(false, "鹰眼中不截毒");

                return new CheckResult(true, "爆发期10秒截毒");
            }

            return new CheckResult(false, "爆发Buff时间充足");
        }

        if (BardHelper.HasAnyPartyBuff())
            return new CheckResult(false, "团辅期不普通续毒");

        if (BardHelper.HasOwnStatusWithTimeLeft(target, CausticBiteDot, 5500) &&
            BardHelper.HasOwnStatusWithTimeLeft(target, StormBiteDot, 5500) ||
            BardHelper.HasOwnStatusWithTimeLeft(target, VenomousBiteDot, 5500) &&
            BardHelper.HasOwnStatusWithTimeLeft(target, WindBiteDot, 5500))
            return new CheckResult(false, "DOT时间仍大于5.5秒");

        if (!BardHelper.HasSelfStatus(HawksEyeBuff) &&
            !(JobGaugeHelper.BRD.GetSoulVoice == 100 && PromeSettings.Instance.GetQt(BRDQt.Apex)))
            return new CheckResult(true, "非爆发期续毒");

        if (BardHelper.HasOwnStatusWithTimeLeft(target, CausticBiteDot, 3000) &&
            BardHelper.HasOwnStatusWithTimeLeft(target, StormBiteDot, 3000) ||
            BardHelper.HasOwnStatusWithTimeLeft(target, VenomousBiteDot, 3000) &&
            BardHelper.HasOwnStatusWithTimeLeft(target, WindBiteDot, 3000))
            return new CheckResult(false, "DOT时间仍大于3秒");

        return new CheckResult(true, "鹰眼/满绝峰下低时长续毒");
    }

    public PAction GetAction()
    {
        return new PAction(IronJaws, ActionType.Gcd, ActionTargetType.Target);
    }
}

