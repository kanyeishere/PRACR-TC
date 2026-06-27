using System;
using System.Collections.Generic;
using PromeRotation.Timeline.Core;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Timeline.Actions;

public class BardSongDurationAction
    : IAction, ISerializableAction, IJobNodeDescriptor
{
    private float _wanderer = 42.6f;
    private float _mage = 39.2f;
    private float _army = 39f;

    public string NodeDisplayName => "Bard/歌曲时长";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("wanderer", "旅神歌时长", "旅神歌持续时长 (3-45)", "float"),
        new NodeParamInfo("mage", "贤者歌时长", "贤者歌持续时长 (3-45)", "float"),
        new NodeParamInfo("army", "军神歌时长", "军神歌持续时长 (3-45)", "float"),
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "wanderer" => _wanderer.ToString("F1"),
        "mage" => _mage.ToString("F1"),
        "army" => _army.ToString("F1"),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        switch (fieldName)
        {
            case "wanderer" when float.TryParse(value, out var v): _wanderer = Math.Clamp(v, 3f, 45f); break;
            case "mage" when float.TryParse(value, out var v): _mage = Math.Clamp(v, 3f, 45f); break;
            case "army" when float.TryParse(value, out var v): _army = Math.Clamp(v, 3f, 45f); break;
        }
    }

    public void Execute()
    {
        var settings = BardSettings.Instance;
        settings.WandererSongDuration = _wanderer;
        settings.MageSongDuration = _mage;
        settings.ArmySongDuration = _army;
    }

    public ActionDto ToDto() => new()
    {
        Type = "bardsongduration",
        Params = new Dictionary<string, string>
        {
            ["wanderer"] = _wanderer.ToString("F1"),
            ["mage"] = _mage.ToString("F1"),
            ["army"] = _army.ToString("F1")
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ActionFactory.Register(context, "bardsongduration", dto =>
        {
            var a = new BardSongDurationAction();
            if (dto.Params == null) return a;
            if (dto.Params.TryGetValue("wanderer", out var w) && float.TryParse(w, out var fw)) a._wanderer = Math.Clamp(fw, 3f, 45f);
            if (dto.Params.TryGetValue("mage", out var m) && float.TryParse(m, out var fm)) a._mage = Math.Clamp(fm, 3f, 45f);
            if (dto.Params.TryGetValue("army", out var r) && float.TryParse(r, out var fr)) a._army = Math.Clamp(fr, 3f, 45f);
            return a;
        });
    }
}
