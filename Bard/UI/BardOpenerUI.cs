using Dalamud.Bindings.ImGui;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.UI;

internal static class BardOpenerUI
{
    private static string _customOpenerSearch = string.Empty;

    private static readonly (uint Id, string DisplayName)[] SkillOptions =
    [
        (49235u, "4级巧力之宝药"),
        (45996u, "3级巧力之宝药"),
        (44163u, "2级巧力之宝药"),
        (44158u, "1级巧力之宝药"),
        (39728u, "8级巧力之幻药"),
        (37841u, "7级巧力之幻药"),
        (BRDSkill.Potion, "当前最佳爆发药"),
        (BRDSkill.HeavyShot, "强力射击"),
        (BRDSkill.ArmsLength, "亲疏自行"),
        (BRDSkill.Peloton, "速行"),
        (BRDSkill.StraightShot, "直线射击"),
        (BRDSkill.Bloodletter, "失血箭"),
        (BRDSkill.HeadGraze, "伤头"),
        (BRDSkill.PitchPerfect, "完美音调"),
        (BRDSkill.EmpyrealArrow, "九天连箭"),
        (BRDSkill.Sidewinder, "侧风诱导箭"),
        (BRDSkill.RefulgentArrow, "辉煌箭"),
        (BRDSkill.BurstShot, "爆发射击"),
        (BRDSkill.QuickNock, "连珠箭"),
        (BRDSkill.RainofDeath, "死亡箭雨"),
        (BRDSkill.Shadowbite, "影噬箭"),
        (BRDSkill.ApexArrow, "绝峰箭"),
        (BRDSkill.Ladonsbite, "百首龙牙箭"),
        (BRDSkill.BlastArrow, "爆破箭"),
        (BRDSkill.VenomousBite, "毒咬箭"),
        (BRDSkill.Windbite, "风蚀箭"),
        (BRDSkill.IronJaws, "伶牙俐齿"),
        (BRDSkill.CausticBite, "烈毒咬箭"),
        (BRDSkill.Stormbite, "狂风蚀箭"),
        (BRDSkill.RagingStrikes, "猛者强击"),
        (BRDSkill.Barrage, "纷乱箭"),
        (BRDSkill.BattleVoice, "战斗之声"),
        (BRDSkill.RadiantFinale, "光明神的最终乐章"),
        (BRDSkill.MagesBallad, "贤者的叙事谣"),
        (BRDSkill.ArmysPaeon, "军神的赞美歌"),
        (BRDSkill.TheWanderersMinuet, "放浪神的小步舞曲"),
        (BRDSkill.RepellingShot, "后跃射击"),
        (BRDSkill.TheWardensPaean, "光阴神的礼赞凯歌"),
        (BRDSkill.NaturesMinne, "大地神的抒情恋歌"),
        (BRDSkill.SecondWind, "内丹"),
        (BRDSkill.Troubadour, "行吟"),
        (BRDSkill.HeartBreak, "碎心箭"),
        (BRDSkill.ResonantArrow, "共鸣箭"),
        (BRDSkill.RadiantEncore, "光明神的返场余音"),
        (BRDSkill.WideVolley, "广域射击"),
        (BRDSkill.FootGraze, "伤足"),
        (BRDSkill.LegGraze, "伤腿")
    ];

    public static void DrawOpenerSettings()
    {
        var settings = BardSettings.Instance;
        settings.EnsureCustomOpeners();
        var changed = false;

        var usePotionInOpener = settings.UsePotionInOpener;
        if (ImGui.Checkbox("起手吃爆发药", ref usePotionInOpener))
        {
            settings.UsePotionInOpener = usePotionInOpener;
            changed = true;
        }

        var opener = settings.Opener;
        if (ImGui.BeginCombo("起手选择", OpenerDisplayName(opener)))
        {
            changed |= SelectOpener("90-100级 3G团辅起手", 0, ref opener);
            changed |= SelectOpener("90-100级 2G团辅起手", 1, ref opener);
            changed |= SelectOpener("100级 1G团辅起手", 2, ref opener);
            changed |= SelectOpener("70-80级 3G团辅起手", 3, ref opener);
            changed |= SelectOpener("70级 5G团辅起手", 4, ref opener);
            changed |= SelectOpener("100级 FR起手", 6, ref opener);
            changed |= SelectOpener("自定义起手", 5, ref opener);
            ImGui.EndCombo();
        }

        if (settings.Opener != opener)
        {
            settings.Opener = opener;
            changed = true;
        }

        if (settings.Opener == 5)
            changed |= DrawCustomOpenerEditor(settings);

        if (changed)
            settings.Save();
    }

    private static bool SelectOpener(string label, int value, ref int opener)
    {
        var selected = opener == value;
        var clicked = ImGui.Selectable(label, selected);
        if (selected)
            ImGui.SetItemDefaultFocus();
        if (!clicked)
            return false;

        opener = value;
        return true;
    }

    private static string OpenerDisplayName(int opener)
    {
        return opener switch
        {
            1 => "90-100级 2G团辅起手",
            2 => "100级 1G团辅起手",
            3 => "70-80级 3G团辅起手",
            4 => "70级 5G团辅起手",
            5 => "自定义起手",
            6 => "100级 FR起手",
            _ => "90-100级 3G团辅起手"
        };
    }

    private static bool DrawCustomOpenerEditor(BardSettings settings)
    {
        var changed = false;
        settings.EnsureCustomOpeners();
        var presets = settings.CustomOpeners;

        ImGui.Separator();
        ImGui.Text("自定义起手预设：");
        ImGui.SameLine();
        if (ImGui.SmallButton("+ 新增预设"))
        {
            presets.Add(new BardSettings.CustomOpenerPreset
            {
                Name = $"自定义起手{presets.Count + 1}"
            });
            settings.SelectedCustomOpenerIndex = presets.Count - 1;
            changed = true;
        }

        for (var i = 0; i < presets.Count; i++)
        {
            ImGui.PushID($"preset_{i}");
            var selected = settings.SelectedCustomOpenerIndex == i;
            if (ImGui.RadioButton($"##select_{i}", selected))
            {
                settings.SelectedCustomOpenerIndex = i;
                changed = true;
            }

            ImGui.SameLine();
            var name = string.IsNullOrWhiteSpace(presets[i].Name)
                ? $"自定义起手{i + 1}"
                : presets[i].Name;
            ImGui.SetNextItemWidth(180);
            if (ImGui.InputText("##preset_name", ref name, 64))
            {
                presets[i].Name = name;
                changed = true;
            }

            ImGui.SameLine();
            if (ImGui.SmallButton("删除预设") && presets.Count > 1)
            {
                presets.RemoveAt(i);
                settings.SelectedCustomOpenerIndex = Math.Clamp(settings.SelectedCustomOpenerIndex, 0, presets.Count - 1);
                changed = true;
                ImGui.PopID();
                break;
            }
            ImGui.PopID();
        }

        settings.EnsureCustomOpeners();
        var current = presets[Math.Clamp(settings.SelectedCustomOpenerIndex, 0, presets.Count - 1)];
        ImGui.Separator();
        if (ImGui.Button("+ 添加技能"))
        {
            var filtered = FilteredSkillOptions();
            current.Skills.Add(filtered.Count > 0 ? filtered[0].Id : 0u);
            changed = true;
        }

        ImGui.SameLine();
        ImGui.Text("搜索技能：");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        ImGui.InputText("##CustomOpenerSearch", ref _customOpenerSearch, 100);

        var skillOptions = FilteredSkillOptions();
        ImGui.Separator();
        for (var i = 0; i < current.Skills.Count; i++)
        {
            var skillId = current.Skills[i];
            ImGui.PushID($"skill_{i}");
            ImGui.Text($"#{i + 1}");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(280);
            if (ImGui.BeginCombo("##CustomOpenerSkill", GetSkillName(skillId)))
            {
                foreach (var skill in skillOptions)
                {
                    var selected = skill.Id == skillId;
                    if (ImGui.Selectable(skill.DisplayName, selected))
                    {
                        current.Skills[i] = skill.Id;
                        changed = true;
                    }
                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            if (ImGui.SmallButton("上移") && i > 0)
            {
                (current.Skills[i - 1], current.Skills[i]) = (current.Skills[i], current.Skills[i - 1]);
                changed = true;
            }
            ImGui.SameLine();
            if (ImGui.SmallButton("下移") && i < current.Skills.Count - 1)
            {
                (current.Skills[i + 1], current.Skills[i]) = (current.Skills[i], current.Skills[i + 1]);
                changed = true;
            }
            ImGui.SameLine();
            if (ImGui.SmallButton("删除"))
            {
                current.Skills.RemoveAt(i);
                changed = true;
                ImGui.PopID();
                i--;
                continue;
            }
            ImGui.PopID();
        }

        return changed;
    }

    private static List<(uint Id, string DisplayName)> FilteredSkillOptions()
    {
        if (string.IsNullOrWhiteSpace(_customOpenerSearch))
            return SkillOptions.ToList();

        return SkillOptions
            .Where(skill => skill.DisplayName.Contains(_customOpenerSearch, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static string GetSkillName(uint skillId)
    {
        foreach (var skill in SkillOptions)
        {
            if (skill.Id == skillId)
                return skill.DisplayName;
        }

        return $"未知技能({skillId})";
    }
}
