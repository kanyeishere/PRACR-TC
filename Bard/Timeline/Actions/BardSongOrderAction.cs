using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Data;
using PromeRotation.Timeline.Core;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Timeline.Actions;

public class BardSongOrderAction
    : IAction, ISerializableAction, IJobNodeDescriptor
{
    private int _firstSong;  // 1=旅神 2=贤者 3=军神
    private int _secondSong;
    private int _thirdSong;

    public string NodeDisplayName => "Bard/歌曲顺序";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("first", "第一首歌", "1=旅神, 2=贤者, 3=军神",
            "int", new[] { ("1", "旅神"), ("2", "贤者"), ("3", "军神") }),
        new NodeParamInfo("second", "第二首歌", "1=旅神, 2=贤者, 3=军神",
            "int", new[] { ("1", "旅神"), ("2", "贤者"), ("3", "军神") }),
        new NodeParamInfo("third", "第三首歌", "1=旅神, 2=贤者, 3=军神",
            "int", new[] { ("1", "旅神"), ("2", "贤者"), ("3", "军神") }),
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "first" => _firstSong.ToString(),
        "second" => _secondSong.ToString(),
        "third" => _thirdSong.ToString(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        if (!int.TryParse(value, out var v) || v < 1 || v > 3) return;
        switch (fieldName)
        {
            case "first": _firstSong = v; break;
            case "second": _secondSong = v; break;
            case "third": _thirdSong = v; break;
        }
    }

    private static Song IntToSong(int idx) => idx switch
    {
        1 => Song.WanderersMinuet,
        2 => Song.MagesBallad,
        3 => Song.ArmysPaeon,
        _ => Song.None
    };

    public void Execute()
    {
        if (_firstSong < 1 || _firstSong > 3 ||
            _secondSong < 1 || _secondSong > 3 ||
            _thirdSong < 1 || _thirdSong > 3)
            return;
        if (_firstSong == _secondSong || _firstSong == _thirdSong || _secondSong == _thirdSong)
            return;

        var settings = BardSettings.Instance;
        settings.FirstSong = IntToSong(_firstSong);
        settings.SecondSong = IntToSong(_secondSong);
        settings.ThirdSong = IntToSong(_thirdSong);

        // 非标准歌轴时禁掉对齐旅神和强对齐
        if (settings.FirstSong != Song.WanderersMinuet ||
            settings.SecondSong != Song.MagesBallad ||
            settings.ThirdSong != Song.ArmysPaeon)
        {
            PromeSettings.Instance.SetQt(BRDQt.BurstWithWanderer, false);
        }
    }

    public ActionDto ToDto() => new()
    {
        Type = "bardsongorder",
        Params = new Dictionary<string, string>
        {
            ["first"] = _firstSong.ToString(),
            ["second"] = _secondSong.ToString(),
            ["third"] = _thirdSong.ToString()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ActionFactory.Register(context, "bardsongorder", dto =>
        {
            var a = new BardSongOrderAction();
            if (dto.Params == null) return a;
            if (dto.Params.TryGetValue("first", out var f) && int.TryParse(f, out var fi)) a._firstSong = Math.Clamp(fi, 1, 3);
            if (dto.Params.TryGetValue("second", out var s) && int.TryParse(s, out var si)) a._secondSong = Math.Clamp(si, 1, 3);
            if (dto.Params.TryGetValue("third", out var t) && int.TryParse(t, out var ti)) a._thirdSong = Math.Clamp(ti, 1, 3);
            return a;
        });
    }
}
