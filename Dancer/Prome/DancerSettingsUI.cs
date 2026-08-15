using Dalamud.Bindings.ImGui;
using System.Numerics;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

internal static class DancerSettingsUI
{
    public static void Draw()
    {
        var settings = DancerSettings.Instance;
        ImGui.Checkbox("日随模式", ref settings.IsDailyMode);
        if (settings.IsDailyMode)
        {
            ImGui.Checkbox("自动舞步", ref settings.EnableAutoDancing);
            ImGui.SameLine();
            ImGui.Checkbox("自动速行", ref settings.EnableAutoPeloton);
            ImGui.SameLine();
            ImGui.Checkbox("自动舞伴", ref settings.EnableAutoDancePartner);
        }
        else
        {
            ImGui.Checkbox("高难模式自动舞伴", ref settings.EnableAutoDancePartnerInFullAutoMode);
        }

        ImGui.Separator();
        ImGui.Checkbox("起手使用爆发药", ref settings.UsePotionInOpener);
        var standard = settings.OpenerType == DancerOpenerType.StandardStep;
        if (ImGui.RadioButton("标准舞起手", standard)) settings.OpenerType = DancerOpenerType.StandardStep;
        ImGui.SameLine();
        if (ImGui.RadioButton("技巧舞起手", !standard)) settings.OpenerType = DancerOpenerType.TechnicalStep;
        ImGui.SetNextItemWidth(180);
        if (settings.OpenerType == DancerOpenerType.StandardStep)
            ImGui.SliderInt("标准舞提前时间（毫秒）", ref settings.OpenerStandardStepTime, 5500, 15000);
        else
            ImGui.SliderInt("技巧舞提前时间（毫秒）", ref settings.OpenerTechnicalStepTime, 5500, 15000);
        ImGui.SetNextItemWidth(180);
        ImGui.SliderInt("舞步结束提前时间（毫秒）", ref settings.OpenerTime, 0, 1000);

        ImGui.Separator();
        ImGui.SetNextItemWidth(180);
        ImGui.SliderInt("剑舞伶俐阈值", ref settings.SaberDanceEspritThreshold, 50, 100);
        ImGui.SetNextItemWidth(180);
        ImGui.SliderInt("爆发期提拉纳阈值", ref settings.TillanaEspritThreshold, 0, 50);
        ImGui.SetNextItemWidth(180);
        ImGui.SliderInt("爆发末段提拉纳阈值", ref settings.TillanaLastGcdEspritThreshold, 0, 50);
        ImGui.SetNextItemWidth(180);
        ImGui.SliderInt("扇舞保留层数", ref settings.FanDanceSaveStack, 0, 4);

        ImGui.Separator();
        ImGui.Checkbox("切换舞伴时发送宏", ref settings.UseDancePartnerMacro);
        if (settings.UseDancePartnerMacro)
            ImGui.InputTextMultiline("##舞伴宏", ref settings.DancePartnerMacroText, 1000,
                new Vector2(-1, ImGui.GetTextLineHeight() * 4));

        if (ImGui.Button("保存舞者设置")) settings.Save();
    }
}
