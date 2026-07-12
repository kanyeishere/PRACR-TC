using System;
using System.Collections.Generic;
using PromeRotation.Helpers;
using PromeRotation.Timeline.Core;

namespace WotouTC.Bard.Timeline.Conditions;

public class BardSongTimerCondition
    : ICondition, ISerializableCondition, IJobNodeDescriptor
{
    private int _operatorIndex; // 0=< 1=> 2=<= 3=>= 4==
    private int _timeMs; // 毫秒

    public string NodeDisplayName => "Bard/判断歌曲剩余时间";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo(
            "operator",
            "条件运算符",
            "比较歌曲剩余时间的方式",
            "enum",
            new[] {
                ("0", "<"),
                ("1", ">"),
                ("2", "<="),
                ("3", ">="),
                ("4", "=="),
            }),
        new NodeParamInfo(
            "time",
            "剩余时间 (ms)",
            "歌曲剩余时间阈值 (0-45000ms)",
            "int")
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "operator" => _operatorIndex.ToString(),
        "time" => _timeMs.ToString(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        switch (fieldName)
        {
            case "operator" when int.TryParse(value, out var v):
                _operatorIndex = Math.Clamp(v, 0, 4);
                break;
            case "time" when int.TryParse(value, out var v):
                _timeMs = Math.Clamp(v, 0, 45000);
                break;
        }
    }

    public bool EvaluateImmediate()
    {
        var timerMs = JobGaugeHelper.BRD.GetCurrentSongTimer;
        return _operatorIndex switch
        {
            0 => timerMs < _timeMs,
            1 => timerMs > _timeMs,
            2 => timerMs <= _timeMs,
            3 => timerMs >= _timeMs,
            4 => Math.Abs(timerMs - _timeMs) < 50,
            _ => false
        };
    }

    public bool EvaluateWait() => EvaluateImmediate();

    public ConditionDto ToDto() => new()
    {
        Type = "bardsongtimer",
        Params = new Dictionary<string, string>
        {
            ["operator"] = _operatorIndex.ToString(),
            ["time"] = _timeMs.ToString()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ConditionFactory.Register(context, "bardsongtimer", dto =>
        {
            var c = new BardSongTimerCondition();
            if (dto.Params?.TryGetValue("operator", out var op) == true && int.TryParse(op, out var o))
                c._operatorIndex = Math.Clamp(o, 0, 4);
            if (dto.Params?.TryGetValue("time", out var t) == true && int.TryParse(t, out var val))
                c._timeMs = Math.Clamp(val, 0, 45000);
            return c;
        });
    }
}
