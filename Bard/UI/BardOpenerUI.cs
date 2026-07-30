using Dalamud.Bindings.ImGui;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.UI;

internal static class BardOpenerUI
{
    public static void DrawOpenerSettings()
    {
        var settings = BardSettings.Instance;

        var usePotionInOpener = settings.UsePotionInOpener;
        if (ImGui.Checkbox("起手吃爆发药", ref usePotionInOpener))
            settings.UsePotionInOpener = usePotionInOpener;

        var opener = settings.Opener;
        if (ImGui.BeginCombo("起手选择", OpenerDisplayName(opener)))
        {
            if (ImGui.Selectable("90-100级 3G团辅起手", opener == 0))
                settings.Opener = 0;
            if (ImGui.Selectable("90-100级 2G团辅起手", opener == 1))
                settings.Opener = 1;
            if (ImGui.Selectable("100级 1G团辅起手", opener == 2))
                settings.Opener = 2;
            if (ImGui.Selectable("70-80级 3G团辅起手", opener == 3))
                settings.Opener = 3;
            if (ImGui.Selectable("70级 5G团辅起手", opener == 4))
                settings.Opener = 4;
            if (ImGui.Selectable("100级 FR起手", opener == 6))
                settings.Opener = 6;

            ImGui.EndCombo();
        }
    }

    private static string OpenerDisplayName(int opener)
    {
        return opener switch
        {
            1 => "90-100级 2G团辅起手",
            2 => "100级 1G团辅起手",
            3 => "70-80级 3G团辅起手",
            4 => "70级 5G团辅起手",
            6 => "100级 FR起手",
            _ => "90-100级 3G团辅起手"
        };
    }
}

