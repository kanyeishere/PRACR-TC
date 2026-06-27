using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BaseGcd : IDecisionResolver
{
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint RadiantFinale = BRDSkill.RadiantFinale;

    public CheckResult Check()
    {
        if (ShouldHoldBaseGcd(out var reason))
            return new CheckResult(false, reason);

        return new CheckResult(true, "基础GCD");
    }

    public PAction GetAction()
    {
        return BardHelper.GetBaseGcd();
    }

    private static bool ShouldHoldBaseGcd(out string reason)
    {
        var settings = BardSettings.Instance;

        if (BardHelper.RecentlyUsed(BattleVoice, settings.GcdTimeAfterBuff) ||
            BardHelper.RecentlyUsed(RadiantFinale, settings.GcdTimeAfterBuff))
        {
            reason = $"团辅后延迟基础GCD:{settings.GcdTimeAfterBuff}ms";
            return true;
        }

        reason = string.Empty;
        return false;
    }
}

