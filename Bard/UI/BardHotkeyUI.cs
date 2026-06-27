using PromeRotation;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.UI;

internal static class BardHotkeyUI
{
    private static HotkeyPanel? _panel;

    public static void SetupHotkeyPanel()
    {
        _panel = new HotkeyPanel(columns: 5);

        _panel.AddHotkey("亲疏自行", new PAction(BRDSkill.ArmsLength, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("续毒",     new PAction(BRDSkill.IronJaws, ActionType.Gcd, ActionTargetType.Target));
        _panel.AddHotkey("内丹",     new PAction(BRDSkill.SecondWind, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("行吟",     new PAction(BRDSkill.Troubadour, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("大地神",   new PAction(BRDSkill.NaturesMinne, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("冲刺",     new PAction(3, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("后跳",     new PAction(BRDSkill.RepellingShot, ActionType.OffGcd, ActionTargetType.Target));
        _panel.AddHotkey("绝峰箭",   new ExecuteLogic(EnqueueApexArrowHotkey), iconActionId: BRDSkill.ApexArrow);
        _panel.AddHotkey("伤头",     new PAction(BRDSkill.HeadGraze, ActionType.OffGcd, ActionTargetType.Target));
        _panel.AddHotkey("停止移动", new ExecuteLogic(StopGreenMoveHotkey),   customIconPath: "Resources/stop-sign.png");

        HotkeyManager.Instance.AddHotkeyPanel(_panel);
    }

    private static void EnqueueApexArrowHotkey()
    {
        if (JobGaugeHelper.BRD.GetSoulVoice < 20)
            return;

        HotkeyQueueManager.TryEnqueue(
            new PAction(BardHelper.Adjust(BRDSkill.ApexArrow), ActionType.Gcd, ActionTargetType.Target));
    }

    private static void StopGreenMoveHotkey()
    {
        Plugin.Instance.GreenMoveSystem.Stop();
    }
}
