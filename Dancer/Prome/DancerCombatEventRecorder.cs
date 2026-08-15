using PromeRotation.Helpers;
using PromeRotation.LogSystem;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

internal static class DancerCombatEventRecorder
{
    public static void OnActionEffect(LogSystemActionEffectEvent ev)
    {
        var playerEntityId = (ulong)Updaters.PlayerCacheUpdater.Snapshot.EntityId;
        if (playerEntityId == 0)
            playerEntityId = Core.Core.Me?.EntityId ?? 0;
        if (playerEntityId == 0 || ev.SourceId != playerEntityId)
            return;

        ActionHelper.RecordAction(ev.ActionId);
        if (ev.ActionId == DancerDefinesData.Spells.TechnicalStep)
            DancerBattleData.Instance.TechnicalStepCount++;
        else if (ev.ActionId == DancerDefinesData.Spells.DanceOfTheDawn)
            DancerBattleData.Instance.DanceOfTheDawnCount++;
    }
}
