using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardApexMaxGcd : IDecisionResolver
{
    private const uint ApexArrow = BRDSkill.ApexArrow;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint CausticBiteDot = BRDBuff.CausticBite;
    private const uint StormBiteDot = BRDBuff.Stormbite;
    private const uint VenomousBiteDot = BRDBuff.VenomousBite;
    private const uint WindBiteDot = BRDBuff.Windbite;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.Apex))
            return new CheckResult(false, "绝峰箭QT关闭");
        if (!PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "爆发QT关闭");
        if (!BardHelper.IsUnlocked(ApexArrow))
            return new CheckResult(false, "绝峰箭未解锁");
        if (BardHelper.RecentlyUsed(ApexArrow, 5000))
            return new CheckResult(false, "绝峰箭刚使用过");

        var soul = JobGaugeHelper.BRD.GetSoulVoice;
        if (BardHelper.HasAllPartyBuff() && soul == 100)
            return new CheckResult(true, "团辅齐全且灵魂之声满");

        var partyBuffCountdown = BardBattleData.Instance.First120SBuffSpellId.GetActionCooldown();
        if (!BardBattleData.Instance.HasUseApexArrowInCurrentNonBurstingPeriod &&
            soul == 100 &&
            BardHelper.HasNoPartyBuff() &&
            partyBuffCountdown >= 43)
            return new CheckResult(true, $"非团辅灵魂之声满，团辅CD:{partyBuffCountdown:F1}s");

        if (BardHelper.RecentlyUsed(RagingStrikes, 10000) &&
            BardHelper.RecentlyUsed(BattleVoice, 10000) &&
            soul == 100)
            return new CheckResult(true, "团辅刚使用且灵魂之声满");

        var target = Core.Core.Target;
        if (target == null)
            return new CheckResult(false, "当前无目标");

        if (HasDotExpiringSoon(target) &&
            soul >= 95 &&
            !BardBattleData.Instance.HasUseApexArrowInCurrentNonBurstingPeriod &&
            partyBuffCountdown >= 43)
            return new CheckResult(true, $"DOT即将续毒且灵魂之声接近满:{soul}");

        return new CheckResult(false, $"满能量绝峰条件不满足 灵魂之声:{soul} 团辅CD:{partyBuffCountdown:F1}s");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(ApexArrow), ActionType.Gcd, ActionTargetType.Target);
    }

    private static bool HasDotExpiringSoon(Dalamud.Game.ClientState.Objects.Types.IBattleChara target)
    {
        return BardHelper.HasOwnStatus(target, CausticBiteDot) &&
               !BardHelper.HasOwnStatusWithTimeLeft(target, CausticBiteDot, 8000) ||
               BardHelper.HasOwnStatus(target, StormBiteDot) &&
               !BardHelper.HasOwnStatusWithTimeLeft(target, StormBiteDot, 8000) ||
               BardHelper.HasOwnStatus(target, VenomousBiteDot) &&
               !BardHelper.HasOwnStatusWithTimeLeft(target, VenomousBiteDot, 8000) ||
               BardHelper.HasOwnStatus(target, WindBiteDot) &&
               !BardHelper.HasOwnStatusWithTimeLeft(target, WindBiteDot, 8000);
    }
}

