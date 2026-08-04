using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.LogSystem;
using WotouTC.Bard.Data;

namespace WotouTC.Bard;

public static class BardCombatEventRecorder
{
    public static void ResetBattleState()
    {
        BardBattleData.Instance.ResetForBattle();
    }

    public static void OnLogSystemActionEffect(LogSystemActionEffectEvent ev)
    {
        var playerEntityId = (ulong)Updaters.PlayerCacheUpdater.Snapshot.EntityId;
        if (playerEntityId == 0)
            playerEntityId = Core.Core.Me?.EntityId ?? 0;

        if (!BardBattleData.Instance.ObserveActionEffect(
                ev.ActionId,
                ev.SourceId,
                ev.GlobalSequence,
                playerEntityId == 0 ? null : playerEntityId))
            return;

        BardBattleData.Instance.RecordAction(ev.ActionId, ev.GlobalSequence);
        BardBattleData.Instance.RecordGcdAction(ev.ActionId);
        BardBattleData.Instance.Record120SBuffAction(ev.ActionId, ev.GlobalSequence);
        ResetIronJawsBurstUsage(ev.ActionId, ev.GlobalSequence);
        RecordSong(ev.ActionId);
        RecordIronJawsBurstUsage(ev.ActionId);
        RecordApexUsage(ev.ActionId);
    }

    private static void RecordSong(uint actionId)
    {
        var song = BardSongHelper.GetSongBySpell(actionId);
        if (song == Song.None)
            return;

        BardBattleData.Instance.RecordSong(song);
    }

    private static void RecordIronJawsBurstUsage(uint actionId)
    {
        if (actionId != BRDSkill.IronJaws)
            return;

        // 猛者动作窗口是主判定；全团辅状态用于兼容插件中途加载或漏掉猛者事件。
        BardBattleData.Instance.RecordIronJawsBurstUsage(
            BardBattleData.Instance.IsWithinIronJawsBurstWindow() || BardHelper.HasAllPartyBuff());
    }

    private static void ResetIronJawsBurstUsage(uint actionId, uint globalSequence)
    {
        if (actionId == BRDSkill.RagingStrikes)
            BardBattleData.Instance.ResetIronJawsBurstUsage(globalSequence);
    }

    private static void RecordApexUsage(uint actionId)
    {
        if (actionId == BRDSkill.ApexArrow && BardHelper.HasNoPartyBuff())
            BardBattleData.Instance.HasUseApexArrowInCurrentNonBurstingPeriod = true;
    }
}
