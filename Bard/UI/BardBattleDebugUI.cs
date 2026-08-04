using Dalamud.Bindings.ImGui;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.UI;

internal static class BardBattleDebugUI
{
    public static void Draw()
    {
        var data = BardBattleData.Instance;
        if (!ImGui.BeginChild("##BardBattleDebug", default, false,
                ImGuiWindowFlags.HorizontalScrollbar))
        {
            ImGui.EndChild();
            return;
        }

        DrawCurrentState(data);
        Draw120SBuffOrder(data);
        DrawLastActions(data);
        DrawEventHistory(data);

        ImGui.EndChild();
    }

    private static void DrawCurrentState(BardBattleData data)
    {
        if (!ImGui.CollapsingHeader("当前战斗数据", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        DrawValue(nameof(data.HasUseIronJawsInCurrentBursting), data.HasUseIronJawsInCurrentBursting);
        DrawValue("IronJawsBurstWindow", data.IsWithinIronJawsBurstWindow());
        DrawValue("LastRagingStrikesActionTime", FormatTick(data.LastRagingStrikesActionTime));
        DrawValue(nameof(data.HasUseApexArrowInCurrentNonBurstingPeriod),
            data.HasUseApexArrowInCurrentNonBurstingPeriod);
        DrawValue("HasAllPartyBuff", BardHelper.HasAllPartyBuff());
        DrawValue("HasAnyPartyBuff", BardHelper.HasAnyPartyBuff());
        DrawValue("ActionEffect 收到/通过/来源过滤/无玩家",
            $"{data.ActionEffectsReceived}/{data.ActionEffectsAccepted}/"
            + $"{data.ActionEffectsSourceFiltered}/{data.ActionEffectsNoPlayer}");
        DrawValue("最近事件 Action/Source/Player/Seq",
            $"{data.LastObservedActionId}/{data.LastObservedActionSourceId}/"
            + $"{data.LastObservedPlayerEntityId}/{data.LastObservedActionSequence}");
        DrawValue(nameof(data.PitchPerfectMinEnemyCount), data.PitchPerfectMinEnemyCount);
        DrawValue(nameof(data.TotalStopTime), data.TotalStopTime.ToString("F3"));
        DrawValue(nameof(data.RestStopTime), data.RestStopTime.ToString("F3"));
        DrawValue(nameof(data.GcdCountDown), data.GcdCountDown);
        DrawValue(nameof(data.BaseGcdHoldStartTime), FormatTick(data.BaseGcdHoldStartTime));
        DrawValue(nameof(data.CurrentBaseGcdHoldTime), data.CurrentBaseGcdHoldTime.ToString("F3"));
        DrawValue(nameof(data.LastSong), BardSongUI.SongDisplayName(data.LastSong));
        DrawValue(nameof(data.LastSongTime), FormatTick(data.LastSongTime));
        DrawValue(nameof(data.DotBlackList),
            data.DotBlackList.Count == 0 ? "空" : string.Join(", ", data.DotBlackList));
    }

    private static void Draw120SBuffOrder(BardBattleData data)
    {
        if (!ImGui.CollapsingHeader("120秒技能记录", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        Draw120SBuffSlot(1, data.HasFirst120SBuff, data.First120SBuffSpellId, data.First120SBuffId);
        Draw120SBuffSlot(2, data.HasSecond120SBuff, data.Second120SBuffSpellId, data.Second120SBuffId);
        Draw120SBuffSlot(3, data.HasThird120SBuff, data.Third120SBuffSpellId, data.Third120SBuffId);
    }

    private static void Draw120SBuffSlot(int slot, bool recorded, uint actionId, uint buffId)
    {
        var state = recorded ? "已记录" : "默认/待记录";
        ImGui.TextWrapped(
            $"[{slot}] {state}  {BardBattleData.GetActionName(actionId)}  Action:{actionId}  Buff:{buffId}");
    }

    private static void DrawLastActions(BardBattleData data)
    {
        if (!ImGui.CollapsingHeader("最近技能时间"))
            return;

        foreach (var pair in data.GetLastActionTimesSnapshot().OrderByDescending(pair => pair.Value))
        {
            ImGui.TextWrapped(
                $"{BardBattleData.GetActionName(pair.Key)} ({pair.Key}): {FormatTick(pair.Value)}");
        }
    }

    private static void DrawEventHistory(BardBattleData data)
    {
        if (!ImGui.CollapsingHeader("状态变化日志", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        if (ImGui.Button("清空日志"))
            data.ClearDebugEvents();

        var events = data.GetDebugEventsSnapshot();
        for (var index = events.Count - 1; index >= 0; index--)
        {
            var entry = events[index];
            ImGui.TextWrapped($"{FormatTick(entry.Timestamp)}  {entry.Message}");
        }
    }

    private static void DrawValue(string name, object value)
    {
        ImGui.TextWrapped($"{name}: {value}");
    }

    private static string FormatTick(long tick)
    {
        if (tick == 0)
            return "未记录";

        var elapsed = Math.Max(0, Environment.TickCount64 - tick);
        return elapsed < 1000 ? $"{elapsed}ms前" : $"{elapsed / 1000d:F1}s前";
    }
}
