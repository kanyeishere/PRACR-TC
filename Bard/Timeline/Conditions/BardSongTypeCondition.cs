using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Helpers;
using PromeRotation.Timeline.Core;

namespace WotouTC.Bard.Timeline.Conditions;

public class BardSongTypeCondition
    : ICondition, ISerializableCondition, IJobNodeDescriptor
{
    private int _songIndex;

    public string NodeDisplayName => "Bard/判断歌曲类型";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo(
            "song_index",
            "歌曲类型",
            "0=无, 1=旅神, 2=贤者, 3=军神",
            "int",
            new[] {
                ("0", "无歌曲"),
                ("1", "旅神歌"),
                ("2", "贤者歌"),
                ("3", "军神歌"),
            })
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "song_index" => _songIndex.ToString(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        if (fieldName == "song_index" && int.TryParse(value, out var v))
            _songIndex = Math.Clamp(v, 0, 3);
    }

    public bool EvaluateImmediate()
    {
        var currentSong = JobGaugeHelper.BRD.GetCurrentSong;
        return _songIndex switch
        {
            0 => currentSong == Song.None,
            1 => currentSong == Song.WanderersMinuet,
            2 => currentSong == Song.MagesBallad,
            3 => currentSong == Song.ArmysPaeon,
            _ => false
        };
    }

    public bool EvaluateWait() => EvaluateImmediate();

    public ConditionDto ToDto() => new()
    {
        Type = "bardsongtype",
        Params = new Dictionary<string, string>
        {
            ["song_index"] = _songIndex.ToString()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ConditionFactory.Register(context, "bardsongtype", dto =>
        {
            var c = new BardSongTypeCondition();
            if (dto.Params?.TryGetValue("song_index", out var raw) == true && int.TryParse(raw, out var n))
                c._songIndex = Math.Clamp(n, 0, 3);
            return c;
        });
    }
}
