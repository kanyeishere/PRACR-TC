using ECommons.Automation;
using PromeRotation.Data;
using PromeRotation.Core;
using PromeRotation.Managers;
using PromeRotation.Helpers;
using PromeRotation.UI.HotKey;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

internal static class DancerHotkeyUI
{
    private static HotkeyPanel? _panel;

    public static void Setup()
    {
        if (_panel != null && HotkeyManager.Instance.Panels.Contains(_panel)) return;
        _panel = new HotkeyPanel(5, "Wotou-TC Dancer Hotkeys", "wotoutc_dancer_hotkeys");
        _panel.AddHotkey("亲疏自行", new PAction(DancerDefinesData.Spells.ArmsLength, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("内丹", new PAction(DancerDefinesData.Spells.SecondWind, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("防守之桑巴", new PAction(DancerDefinesData.Spells.ShieldSamba, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("治疗之华尔兹", new PAction(DancerDefinesData.Spells.CuringWaltz, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("冲刺", new PAction(3, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("前冲步", new PAction(DancerDefinesData.Spells.EnAvant, ActionType.OffGcd, ActionTargetType.Self));
        _panel.AddHotkey("伤头", new PAction(DancerDefinesData.Spells.HeadGraze, ActionType.OffGcd, ActionTargetType.Target));
        _panel.AddHotkey("即兴表演", new List<PAction>
        {
            new(DancerDefinesData.Spells.Improvisation, ActionType.OffGcd, ActionTargetType.Self),
            new(DancerDefinesData.Spells.ImprovisationFinish, ActionType.OffGcd, ActionTargetType.Self)
        });
        _panel.AddHotkey("爆发药", new ExecuteLogic(EnqueuePotion), iconActionId: 44158);
        for (var index = 1; index <= 7; index++)
        {
            var partyIndex = index;
            _panel.AddHotkey($"舞伴{index + 1}",
                new ExecuteLogic(() => SwitchDancePartner(partyIndex)),
                iconActionId: DancerDefinesData.Spells.ClosedPosition);
        }
        HotkeyManager.Instance.AddHotkeyPanel(_panel);
    }

    private static void EnqueuePotion()
    {
        var potionId = GameData.GetBestPotionId();
        if (potionId != 0)
            HotkeyQueueManager.TryEnqueue(new PAction(potionId, ActionType.Item, ActionTargetType.Self));
    }

    private static void SwitchDancePartner(int partyIndex)
    {
        if (DancerHelper.IsDancing || !DancerHelper.IsReady(DancerDefinesData.Spells.ClosedPosition) ||
            PartyHelper.GetPartyMember(partyIndex) == null)
            return;

        var actions = new List<PAction>();
        if (DancerHelper.HasSelfStatus(DancerDefinesData.Buffs.ClosedPosition))
            actions.Add(new PAction(DancerDefinesData.Spells.Ending, ActionType.OffGcd, ActionTargetType.Self));
        actions.Add(new PAction(DancerDefinesData.Spells.ClosedPosition, ActionType.OffGcd,
            PartyHelper.GetTargetTypeByIndex(partyIndex)));
        HotkeyQueueManager.TryEnqueueList(actions);

        var settings = DancerSettings.Instance;
        if (!settings.UseDancePartnerMacro || string.IsNullOrWhiteSpace(settings.DancePartnerMacroText))
            return;

        foreach (var line in settings.DancePartnerMacroText.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0)
                Chat.SendMessage(trimmed.Replace("<t>", $"<{partyIndex + 1}>", StringComparison.OrdinalIgnoreCase));
        }
    }
}
