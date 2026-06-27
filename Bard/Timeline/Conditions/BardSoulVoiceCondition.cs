using System;
using System.Collections.Generic;
using PromeRotation.Helpers;
using PromeRotation.Timeline.Core;

namespace WotouTC.Bard.Timeline.Conditions;

public class BardSoulVoiceCondition
    : ICondition, ISerializableCondition, IJobNodeDescriptor
{
    private int _operatorIndex; // 0=< 1=> 2=<= 3=>= 4==
    private int _soulVoice;

    public string NodeDisplayName => "Bard/判断灵魂之声";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo(
            "operator",
            "条件运算符",
            "比较灵魂之声的方式",
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
            "灵魂之声",
            "灵魂之声阈值 (0-100)",
            "int")
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "operator" => _operatorIndex.ToString(),
        "value" => _soulVoice.ToString(),
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
                _soulVoice = Math.Clamp(v, 0, 100);
                break;
        }
    }

    public bool EvaluateImmediate()
    {
        var sv = JobGaugeHelper.BRD.GetSoulVoice;
        return _operatorIndex switch
        {
            0 => sv < _soulVoice,
            1 => sv > _soulVoice,
            2 => sv <= _soulVoice,
            3 => sv >= _soulVoice,
            4 => sv == _soulVoice,
            _ => false
        };
    }

    public bool EvaluateWait() => EvaluateImmediate();

    public ConditionDto ToDto() => new()
    {
        Type = "bardsoulvoice",
        Params = new Dictionary<string, string>
        {
            ["operator"] = _operatorIndex.ToString(),
            ["value"] = _soulVoice.ToString()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ConditionFactory.Register(context, "bardsoulvoice", dto =>
        {
            var c = new BardSoulVoiceCondition();
            if (dto.Params?.TryGetValue("operator", out var op) == true && int.TryParse(op, out var o))
                c._operatorIndex = Math.Clamp(o, 0, 4);
            if (dto.Params?.TryGetValue("value", out var v) == true && int.TryParse(v, out var val))
                c._soulVoice = Math.Clamp(val, 0, 100);
            return c;
        });
    }
}
