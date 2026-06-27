using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace WotouTC.Bard.Action;

public class BardSongAbility : IDecisionResolver
{
    private uint _songActionId;

    public CheckResult Check()
    {
        if (!BardSongHelper.ShouldSwitchSong(out _songActionId, out var reason))
            return new CheckResult(false, reason);

        return new CheckResult(true, reason);
    }

    public PAction GetAction()
    {
        return new PAction(_songActionId, ActionType.OffGcd, ActionTargetType.Target);
    }
}

