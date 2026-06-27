using System;
using System.Collections.Generic;
using PromeRotation.Timeline.Core;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Timeline.Actions;

public class BardPitchPerfectMinCountAction
    : IAction, ISerializableAction, IJobNodeDescriptor
{
    private int _minCount;

    public string NodeDisplayName => "Bard/完美音调最少敌人数";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("min_count", "最少敌人数量", "使用完美音调所需的最少敌人数", "int")
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "min_count" => _minCount.ToString(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        if (fieldName == "min_count" && int.TryParse(value, out var v))
            _minCount = Math.Max(0, v);
    }

    public void Execute()
    {
        BardBattleData.Instance.PitchPerfectMinEnemyCount = _minCount;
    }

    public ActionDto ToDto() => new()
    {
        Type = "bardpitchperfectmincount",
        Params = new Dictionary<string, string>
        {
            ["min_count"] = _minCount.ToString()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ActionFactory.Register(context, "bardpitchperfectmincount", dto =>
        {
            var a = new BardPitchPerfectMinCountAction();
            if (dto.Params?.TryGetValue("min_count", out var raw) == true && int.TryParse(raw, out var v))
                a._minCount = Math.Max(0, v);
            return a;
        });
    }
}
