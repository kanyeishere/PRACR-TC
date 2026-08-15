using PromeRotation;
using PromeRotation.Data;
using PromeRotation.Core;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.LogSystem;
using PromeRotation.Rotation;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

public sealed class DancerRotationEventHandler : IRotationEventHandler
{
    private static bool _eventsAttached;
    private static readonly IReadOnlyDictionary<uint, int> DancePartnerPriorities = new Dictionary<uint, int>
    {
        [41] = 1,  // VPR
        [42] = 2,  // PCT
        [20] = 3,  // MNK
        [25] = 4,  // BLM
        [39] = 5,  // RPR
        [34] = 6,  // SAM
        [30] = 7,  // NIN
        [22] = 8,  // DRG
        [35] = 9,  // RDM
        [27] = 10, // SMN
        [31] = 11, // MCH
        [23] = 12, // BRD
        [38] = 13, // DNC
        [36] = 14, // BLU
        [32] = 15, // DRK
        [37] = 16, // GNB
        [19] = 17, // PLD
        [21] = 18, // WAR
        [40] = 19, // SGE
        [24] = 20, // WHM
        [28] = 21, // SCH
        [33] = 22, // AST
        [2] = 23,  // PGL
        [7] = 24,  // THM
        [5] = 25,  // ARC
        [29] = 26, // ROG
        [26] = 27, // ACN
        [1] = 28,  // GLA
        [3] = 29,  // MRD
        [4] = 30,  // LNC
        [6] = 31   // CNJ
    };

    public DancerRotationEventHandler()
    {
        if (_eventsAttached) return;
        Plugin.Instance.LogSystem.Events.Subscribe<LogSystemActionEffectEvent>(DancerCombatEventRecorder.OnActionEffect);
        _eventsAttached = true;
    }

    public void OnUpdate() { }

    public void OnOutOfBattleUpdate()
    {
        if (Core.Core.Me == null || GameData.IsPlayerOccupied())
            return;

        var settings = DancerSettings.Instance;
        var allowDancing = settings.IsDailyMode && settings.EnableAutoDancing;
        var allowPartner = settings.EnableAutoDancePartner &&
                           (settings.IsDailyMode || settings.EnableAutoDancePartnerInFullAutoMode);
        var allowPeloton = settings.IsDailyMode && settings.EnableAutoPeloton;
        if (settings.IsDailyMode)
            PromeSettings.Instance.SetQt(QTKey.SmartAoeTarget, true);
        if (!allowDancing && !allowPartner && !allowPeloton)
            return;

        if (allowDancing && TryAdvanceDance(allowFinish: false))
            return;

        if (allowDancing && GameData.IsBoundByDuty && !DancerHelper.IsDancing && Core.Core.Target != null &&
            TargetHelper.EnemyInRange(30) > 0 &&
            !ActionQueueManager.HasActionsInGcdQueue() && DancerHelper.IsReady(DancerDefinesData.Spells.StandardStep))
        {
            ActionQueueManager.EnqueueAndRecord(
                DancerHelper.Gcd(DancerDefinesData.Spells.StandardStep, ActionTargetType.Self), true);
            return;
        }

        if (allowPartner && !DancerHelper.IsDancing &&
            !DancerHelper.HasSelfStatus(DancerDefinesData.Buffs.ClosedPosition) &&
            !DancerHelper.RecentlyUsed(DancerDefinesData.Spells.ClosedPosition, 2000) &&
            DancerHelper.IsReady(DancerDefinesData.Spells.ClosedPosition) &&
            !ActionQueueManager.HasActionsInOffGcdQueue())
        {
            var party = PartyHelper.GetParty();
            var me = Core.Core.Me;
            var bestIndex = -1;
            var bestPriority = int.MaxValue;
            uint bestMaxHp = 0;
            for (var index = 1; index < party.Count; index++)
            {
                var member = party[index];
                if (member == null || member.IsDead || !member.IsTargetable || member.EntityId == me?.EntityId)
                    continue;
                var priority = DancePartnerPriorities.GetValueOrDefault(member.ClassJob.RowId, int.MaxValue - 1);
                if (priority > bestPriority || (priority == bestPriority && member.MaxHp <= bestMaxHp))
                    continue;
                bestIndex = index;
                bestPriority = priority;
                bestMaxHp = member.MaxHp;
            }

            if (bestIndex >= 0)
                ActionQueueManager.EnqueueAndRecord(
                    DancerHelper.OffGcd(DancerDefinesData.Spells.ClosedPosition,
                        PartyHelper.GetTargetTypeByIndex(bestIndex)), true);
        }

        if (allowPeloton && !DancerHelper.IsDancing && MoveManager.IsLocalPlayerActuallyMoving &&
            !DancerHelper.HasSelfStatusWithTimeLeft(DancerDefinesData.Buffs.Peloton, 4000) &&
            !DancerHelper.RecentlyUsed(DancerDefinesData.Spells.Peloton, 5000) &&
            DancerHelper.IsReady(DancerDefinesData.Spells.Peloton) &&
            !ActionQueueManager.HasActionsInOffGcdQueue())
        {
            ActionQueueManager.EnqueueAndRecord(
                DancerHelper.OffGcd(DancerDefinesData.Spells.Peloton), true);
        }

    }

    public void OnBattleStarted() { }
    public void OnBattleUpdate() { }
    public void OnNoTarget() => TryAdvanceDance(allowFinish: true);

    public void OnBattleEnded()
    {
        PromeSettings.Instance.OpenerHasBeenExecuted = false;
        DancerBattleData.Instance.Reset();
        DancerSettings.Instance.FanDanceSaveStack = 3;
    }

    public void OnTerritoryChanged(ushort territoryId) { }

    private static bool TryAdvanceDance(bool allowFinish)
    {
        if (!DancerHelper.IsDancing || ActionQueueManager.HasActionsInGcdQueue())
            return false;

        var canFinish = allowFinish && GameData.IsInCombat() && TargetHelper.EnemyInRange(10) > 0;
        PAction? action = null;
        if (DancerHelper.HasSelfStatus(DancerDefinesData.Buffs.StandardStep))
            action = DancerHelper.CompletedSteps >= 2
                ? canFinish ? DancerHelper.Gcd(DancerDefinesData.Spells.DoubleStandardFinish, ActionTargetType.Self) : null
                : DancerHelper.StepAction();
        else if (DancerHelper.HasSelfStatus(DancerDefinesData.Buffs.TechnicalStep))
            action = DancerHelper.CompletedSteps >= 4
                ? canFinish ? DancerHelper.Gcd(DancerDefinesData.Spells.QuadrupleTechnicalFinish, ActionTargetType.Self) : null
                : DancerHelper.StepAction();

        if (action == null)
            return false;

        ActionQueueManager.EnqueueAndRecord(action, true);
        return true;
    }
}
