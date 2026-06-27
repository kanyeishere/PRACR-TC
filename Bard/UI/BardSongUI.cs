using ImGuiNET;
using Dalamud.Game.ClientState.JobGauge.Enums;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.UI;

internal static class BardSongUI
{
    public static void DrawSongSettings()
    {
        var settings = BardSettings.Instance;

        var firstSong = settings.FirstSong;
        if (DrawSongCombo("第一首", ref firstSong))
            settings.FirstSong = firstSong;

        var secondSong = settings.SecondSong;
        if (DrawSongCombo("第二首", ref secondSong))
            settings.SecondSong = secondSong;

        var thirdSong = settings.ThirdSong;
        if (DrawSongCombo("第三首", ref thirdSong))
            settings.ThirdSong = thirdSong;

        if (ImGui.Button("重置默认歌序"))
            settings.ResetSongOrderNormal();

        var wandererDuration = settings.WandererSongDuration;
        if (ImGui.SliderFloat("旅神歌时长", ref wandererDuration, 3f, 45f, "%.1f"))
            settings.WandererSongDuration = wandererDuration;

        var mageDuration = settings.MageSongDuration;
        if (ImGui.SliderFloat("贤者歌时长", ref mageDuration, 3f, 45f, "%.1f"))
            settings.MageSongDuration = mageDuration;

        var armyDuration = settings.ArmySongDuration;
        if (ImGui.SliderFloat("军神歌时长", ref armyDuration, 3f, 45f, "%.1f"))
            settings.ArmySongDuration = armyDuration;
    }

    private static bool DrawSongCombo(string label, ref Song song)
    {
        var changed = false;
        if (!ImGui.BeginCombo(label, SongDisplayName(song))) return false;

        foreach (var candidate in new[] { Song.Wanderer, Song.Mage, Song.Army })
        {
            var selected = song == candidate;
            if (ImGui.Selectable(SongDisplayName(candidate), selected))
            {
                song = candidate;
                changed = true;
            }
            if (selected)
                ImGui.SetItemDefaultFocus();
        }

        ImGui.EndCombo();
        return changed;
    }

    public static string SongDisplayName(Song song)
    {
        return song switch
        {
            Song.Wanderer => "旅神",
            Song.Mage => "贤者",
            Song.Army => "军神",
            _ => "无"
        };
    }
}


