using System;
using System.Collections.Generic;
using PromeRotation.Helpers;
using PromeRotation.Timeline.Core;

namespace WotouTC.Bard.Timeline.Conditions;

public class BardRepertoireCondition
    : ICondition, ISerializableCondition, IJobNodeDescriptor
{
    private int _operatorIndex; // 0=< 1=> 2=<= 3=>= 4==
    private int _repertoire;

    public string NodeDisplayName => "Bard/判断诗心层数";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo(
            "operator",
            "条件运算符",
            "比较诗心层数的方式",
            "int",
            new[] {
                ("0", "<"),
                ("1", ">"),
                ("2", "<="),
                ("3", ">="),
                ("4", "=="),
            }),
        new NodeParamInfo(
            "value",
            "层数",
            "诗心层数阈值 (0-4)",
            "int")
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "operator" => _operatorIndex.ToString(),
        "value" => _repertoire.ToString(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        switch (fieldName)
        {
            case "operator" when int.TryParse(value, out var v):
                _operatorIndex = Math.Clamp(v, 0, 4);
                break;
            case "value" when int.TryParse(value, out var v):
                _repertoire = Math.Clamp(v, 0, 4);
                break;
        }
    }

    public bool EvaluateImmediate()
    {
        var current = JobGaugeHelper.BRD.GetRepertoire;
        return _operatorIndex switch
        {
            0 => current < _repertoire,
            1 => current > _repertoire,
            2 => current <= _repertoire,
            3 => current >= _repertoire,
            4 => current == _repertoire,
            _ => false
        };
    }

    public bool EvaluateWait() => EvaluateImmediate();

    public ConditionDto ToDto() => new()
    {
        Type = "bardrepertoire",
        Params = new Dictionary<string, string>
        {
            ["operator"] = _operatorIndex.ToString(),
            ["value"] = _repertoire.ToString()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ConditionFactory.Register(context, "bardrepertoire", dto =>
        {
            var c = new BardRepertoireCondition();
            if (dto.Params?.TryGetValue("operator", out var op) == true && int.TryParse(op, out var o))
                c._operatorIndex = Math.Clamp(o, 0, 4);
            if (dto.Params?.TryGetValue("value", out var v) == true && int.TryParse(v, out var val))
                c._repertoire = Math.Clamp(val, 0, 4);
            return c;
        });
    }
}
