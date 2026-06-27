using PromeRotation.Data;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardBlastArrowGcd : IDecisionResolver
{
    private const uint BlastArrow = BRDSkill.BlastArrow;
    private const uint BlastArrowReady = BRDBuff.BlastArrowReady;
    private const uint HawksEye = BRDBuff.HawksEye;
    private const uint CausticBiteDot = BRDBuff.CausticBite;
    private const uint StormBiteDot = BRDBuff.Stormbite;
    private const uint VenomousBiteDot = BRDBuff.VenomousBite;
    private const uint WindBiteDot = BRDBuff.Windbite;

    public CheckResult Check()
    {
        if (!BardHelper.IsUnlocked(BlastArrow))
            return new CheckResult(false, "爆破箭未解锁");

        var target = Core.Core.Target;

        if (BardHelper.HasSelfStatus(HawksEye) &&
            PromeSettings.Instance.GetQt(BRDQt.DOT) &&
            target != null &&
            HasDotExpiringSoon(target))
            return new CheckResult(false, "鹰眼中且即将续毒，保留爆破箭");

        if (BardHelper.HasSelfStatus(BlastArrowReady))
            return new CheckResult(true, "爆破箭预备");

        return new CheckResult(false, "没有爆破箭预备");
    }

    public PAction GetAction()
    {
        return new PAction(BardHelper.Adjust(BlastArrow), ActionType.Gcd, ActionTargetType.Target);
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

