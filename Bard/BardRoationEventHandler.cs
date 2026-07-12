using PromeRotation.Data;
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
        // 复位全局起手标记，否则第二次战斗时框架会因 OpenerHasBeenExecuted 仍为 true 而跳过 GetOpener()，
        // 导致在 DrawSettings / PTL 里改的起手不会生效。
        PromeSettings.Instance.OpenerHasBeenExecuted = false;
        BardCombatEventRecorder.ResetBattleState();
        BardSettings.Instance.ResetSongOrderNormal();
    }

    public void OnTerritoryChanged(ushort territoryId)
    {
    }
}

