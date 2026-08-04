using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Rotation;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Opener;

/// <summary>
/// Executes the currently selected custom opener preset in the order configured by the user.
/// </summary>
public sealed class BardCustomOpener : IOpener
{
    private readonly List<uint> _skills;

    public BardCustomOpener()
    {
        var settings = BardSettings.Instance;
        settings.EnsureCustomOpeners();
        var index = Math.Clamp(settings.SelectedCustomOpenerIndex, 0, settings.CustomOpeners.Count - 1);
        _skills = new List<uint>(settings.CustomOpeners[index].Skills);
    }

    public string OpenerName => "自定义起手";

    public void InitializeCountdown(CountDownHandler countdownHandler)
    {
    }

    public List<PAction> InCombatSequence => BuildSequence(_skills);

    private static List<PAction> BuildSequence(IEnumerable<uint> skills)
    {
        var actions = new List<PAction>();
        foreach (var skillId in skills)
        {
            if (skillId == 0)
                continue;

            if (IsPotionSkill(skillId))
            {
                var potionId = GameData.GetBestPotionId();
                if (potionId != 0)
                    actions.Add(new PAction(potionId, ActionType.Item, ActionTargetType.Self));
                continue;
            }

            var adjustedId = BardHelper.Adjust(skillId);
            if (adjustedId == 0 || !BardHelper.IsUnlocked(adjustedId))
                continue;
            if (!ActionHelper.TryResolveActionType(adjustedId, out var type))
                continue;

            var target = ResolveTarget(adjustedId);
            var action = new PAction(adjustedId, type, target);
            if (type == ActionType.Gcd)
                action.RequiresVerification = true;
            actions.Add(action);
        }

        return actions;
    }

    private static ActionTargetType ResolveTarget(uint actionId)
    {
        if (ActionHelper.TryResolveActionTargetType(actionId, out var target))
            return target;

        // ActionManager range is unavailable during some loading states. Fall back to Lumina's
        // target flags so self buffs are not sent to the combat target.
        try
        {
            var action = ECommons.DalamudServices.Svc.Data
                .GetExcelSheet<Lumina.Excel.Sheets.Action>()
                .GetRowOrDefault(actionId);
            if (action.HasValue && !action.Value.CanTargetHostile)
                return ActionTargetType.Self;
        }
        catch
        {
            // Keep the normal target fallback if game data is not ready yet.
        }

        return ActionTargetType.Target;
    }

    private static bool IsPotionSkill(uint skillId)
    {
        return skillId is BRDSkill.Potion or 49235u or 45996u or 44163u or 44158u or 39728u or 37841u;
    }
}
