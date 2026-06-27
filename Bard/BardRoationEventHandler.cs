using PromeRotation.Managers.CombatEventManager;
using PromeRotation.Rotation;
using WotouTC.Bard.Data;

namespace WotouTC.Bard;

public class BardRoationEventHandler : IRotationEventHandler
{
    private static bool _eventsAttached;

    public BardRoationEventHandler()
    {
        if (_eventsAttached) return;

        CombatEventManager.OnActionEffect += BardCombatEventRecorder.OnActionEffect;
        _eventsAttached = true;
    }

    public void OnUpdate()
    {
    }

    public void OnOutOfBattleUpdate()
    {
        BardNoTargetSongSwitcher.MarkOutOfBattle();
    }

    public void OnBattleStarted()
    {
        BardCombatEventRecorder.ResetBattleState();
    }

    public void OnBattleUpdate()
    {
        BardNoTargetSongSwitcher.TrySwitch();
    }

    public void OnNoTarget()
    {
    }

    public void OnBattleEnded()
    {
        BardCombatEventRecorder.ResetBattleState();
        BardSettings.Instance.ResetSongOrderNormal();
    }

    public void OnTerritoryChanged(ushort territoryId)
    {
    }
}

