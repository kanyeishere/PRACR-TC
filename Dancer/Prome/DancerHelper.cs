using Dalamud.Game.ClientState.JobGauge.Enums;
using ECommons.ExcelServices;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

public static class DancerHelper
{
    public static bool IsUnlocked(uint actionId)
        => Core.Core.Me != null && ActionHelper.IsActionAvailableByLevelAndQuest(actionId);

    public static bool IsReady(uint actionId)
        => IsUnlocked(actionId) && CooldownMs(actionId) <= 50f;

    public static float CooldownMs(uint actionId)
        => ActionHelper.GetActionCooldown(ActionHelper.GetAdjustedActionId(actionId)) * 1000f;

    public static bool RecentlyUsed(uint actionId, int milliseconds = 1000)
        => ActionHelper.RecentlyUsed(actionId, milliseconds);

    public static bool HasSelfStatus(uint statusId)
        => Core.Core.Me?.HasStatus(statusId) == true;

    public static bool HasSelfStatusWithTimeLeft(uint statusId, int milliseconds)
        => Core.Core.Me?.GetStatusLeftTime(statusId) > milliseconds / 1000f;

    public static float StatusLeftMs(uint statusId)
        => (Core.Core.Me?.GetStatusLeftTime(statusId) ?? 0f) * 1000f;

    public static bool CanUseAoe()
        => PromeSettings.Instance.GetQt(QTKey.Aoe) && TargetHelper.EnemyInRange(5) > 1;

    public static uint NextStep => JobGaugeHelper.DNC.NextStep;
    public static int Esprit => JobGaugeHelper.DNC.Esprit;
    public static int Feathers => JobGaugeHelper.DNC.Feathers;
    public static bool IsDancing => JobGaugeHelper.DNC.IsDancing;
    public static int CompletedSteps => (int)JobGaugeHelper.DNC.CompletedSteps;

    public static uint Adjust(uint actionId)
    {
        var adjusted = ActionHelper.GetAdjustedActionId(actionId);
        return adjusted == 0 ? actionId : adjusted;
    }

    public static PAction Gcd(uint actionId, ActionTargetType target = ActionTargetType.Target)
        => new(Adjust(actionId), ActionType.Gcd, target);

    public static PAction OffGcd(uint actionId, ActionTargetType target = ActionTargetType.Self)
        => new(Adjust(actionId), ActionType.OffGcd, target);

    public static PAction Item(uint itemId)
        => new(itemId, ActionType.Item, ActionTargetType.Self);

    public static PAction SmartGcd(uint actionId, int minimumTargets = 1, float angle = 0f)
        => ApplySmartTarget(Gcd(actionId), minimumTargets, angle);

    public static PAction SmartOffGcd(uint actionId, int minimumTargets = 1, float angle = 0f)
        => ApplySmartTarget(OffGcd(actionId, ActionTargetType.Target), minimumTargets, angle);

    public static PAction? StepAction()
        => NextStep == 0 ? null : Gcd(NextStep, ActionTargetType.Self);

    public static PAction BaseGcd()
    {
        const uint cascade = DancerDefinesData.Spells.Cascade;
        const uint fountain = DancerDefinesData.Spells.Fountain;
        const uint windmill = DancerDefinesData.Spells.Windmill;
        const uint bladeshower = DancerDefinesData.Spells.Bladeshower;

        var combo = ActionHelper.GetLastComboID();
        if (combo == cascade && ActionHelper.GetComboLeftTime() > 0.01f)
            return Gcd(fountain);
        if (CanUseAoe() && combo == windmill && ActionHelper.GetComboLeftTime() > 0.01f)
            return Gcd(bladeshower);
        return Gcd(CanUseAoe() ? windmill : cascade);
    }

    public static PAction ProcGcd()
    {
        if (CanUseAoe())
        {
            if (HasSelfStatus(DancerDefinesData.Buffs.SilkenFlow) && IsUnlocked(DancerDefinesData.Spells.Bloodshower))
                return SmartGcd(DancerDefinesData.Spells.Bloodshower);
            if (HasSelfStatus(DancerDefinesData.Buffs.FlourshingFlow) && IsReady(DancerDefinesData.Spells.Bloodshower))
                return SmartGcd(DancerDefinesData.Spells.Bloodshower);
            if (IsReady(DancerDefinesData.Spells.Bloodshower))
                return SmartGcd(DancerDefinesData.Spells.Bloodshower);
            if (HasSelfStatus(DancerDefinesData.Buffs.SilkenSymmetry) && IsUnlocked(DancerDefinesData.Spells.RisingWindmill))
                return SmartGcd(DancerDefinesData.Spells.RisingWindmill);
            if (HasSelfStatus(DancerDefinesData.Buffs.FlourishingSymmetry) && IsReady(DancerDefinesData.Spells.RisingWindmill))
                return SmartGcd(DancerDefinesData.Spells.RisingWindmill);
            if (IsReady(DancerDefinesData.Spells.RisingWindmill))
                return SmartGcd(DancerDefinesData.Spells.RisingWindmill);
            if (HasSelfStatus(DancerDefinesData.Buffs.SilkenFlow) && IsUnlocked(DancerDefinesData.Spells.Fountainfall))
                return Gcd(DancerDefinesData.Spells.Fountainfall);
            if (HasSelfStatus(DancerDefinesData.Buffs.FlourshingFlow) && IsReady(DancerDefinesData.Spells.Fountainfall))
                return Gcd(DancerDefinesData.Spells.Fountainfall);
            if (IsReady(DancerDefinesData.Spells.Fountainfall))
                return Gcd(DancerDefinesData.Spells.Fountainfall);
            if (HasSelfStatus(DancerDefinesData.Buffs.SilkenSymmetry) && IsUnlocked(DancerDefinesData.Spells.ReverseCascade))
                return Gcd(DancerDefinesData.Spells.ReverseCascade);
            if (HasSelfStatus(DancerDefinesData.Buffs.FlourishingSymmetry) && IsReady(DancerDefinesData.Spells.ReverseCascade))
                return Gcd(DancerDefinesData.Spells.ReverseCascade);
            if (IsReady(DancerDefinesData.Spells.ReverseCascade))
                return Gcd(DancerDefinesData.Spells.ReverseCascade);
            return BaseGcd();
        }

        if (HasSelfStatus(DancerDefinesData.Buffs.SilkenFlow) && IsUnlocked(DancerDefinesData.Spells.Fountainfall))
            return Gcd(DancerDefinesData.Spells.Fountainfall);
        if (HasSelfStatus(DancerDefinesData.Buffs.FlourshingFlow) && IsReady(DancerDefinesData.Spells.Fountainfall))
            return Gcd(DancerDefinesData.Spells.Fountainfall);
        if (IsReady(DancerDefinesData.Spells.Fountainfall))
            return Gcd(DancerDefinesData.Spells.Fountainfall);
        if (HasSelfStatus(DancerDefinesData.Buffs.SilkenSymmetry) && IsUnlocked(DancerDefinesData.Spells.ReverseCascade))
            return Gcd(DancerDefinesData.Spells.ReverseCascade);
        if (HasSelfStatus(DancerDefinesData.Buffs.FlourishingSymmetry) && IsReady(DancerDefinesData.Spells.ReverseCascade))
            return Gcd(DancerDefinesData.Spells.ReverseCascade);
        if (IsReady(DancerDefinesData.Spells.ReverseCascade))
            return Gcd(DancerDefinesData.Spells.ReverseCascade);
        return BaseGcd();
    }

    public static int NearbyEnemies(float range) => (int)TargetHelper.EnemyInRange(range);

    private static PAction ApplySmartTarget(PAction action, int minimumTargets, float angle)
    {
        if (!PromeSettings.Instance.GetQt(QTKey.SmartAoeTarget))
            return action;

        var target = TargetHelper.GetMostCanTargetObjects(action.ActionId, minimumTargets, angle);
        if (target != null)
            action.NetworkTid = (uint)target.EntityId;
        return action;
    }
}
