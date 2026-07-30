using System;
using System.Collections.Generic;
using System.Numerics;
using ECommons.DalamudServices;
using PromeRotation.Timeline.Core;

namespace WotouTC.Bard.Timeline.Conditions;

public class TargetInRangeCondition
    : ICondition, ISerializableCondition, IJobNodeDescriptor
{
    private uint _dataId;
    private float _radius = 2f;
    private float _centerX;
    private float _centerY;
    private float _centerZ;

    public string NodeDisplayName => "通用/判断目标是否在范围内";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("data_id", "目标Data ID", "目标的DataId", "uint"),
        new NodeParamInfo("radius", "范围半径", "检测范围半径 (1-200)", "float"),
        new NodeParamInfo("center_x", "中心点X", "中心点X坐标", "float"),
        new NodeParamInfo("center_y", "中心点Y", "中心点Y坐标", "float"),
        new NodeParamInfo("center_z", "中心点Z", "中心点Z坐标", "float"),
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "data_id" => _dataId.ToString(),
        "radius" => _radius.ToString("F1"),
        "center_x" => _centerX.ToString("F1"),
        "center_y" => _centerY.ToString("F1"),
        "center_z" => _centerZ.ToString("F1"),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        switch (fieldName)
        {
            case "data_id" when uint.TryParse(value, out var v): _dataId = v; break;
            case "radius" when float.TryParse(value, out var v): _radius = Math.Clamp(v, 1f, 200f); break;
            case "center_x" when float.TryParse(value, out var v): _centerX = v; break;
            case "center_y" when float.TryParse(value, out var v): _centerY = v; break;
            case "center_z" when float.TryParse(value, out var v): _centerZ = v; break;
        }
    }

    public bool EvaluateImmediate()
    {
        if (_dataId == 0) return false;
        var center = new Vector3(_centerX, _centerY, _centerZ);

        for (var i = 0; i < Svc.Objects.Length; i++)
        {
            var obj = Svc.Objects[i];
            if (obj == null || obj.BaseId != _dataId) continue;
            if (Vector3.Distance(center, obj.Position) <= _radius)
                return true;
        }
        return false;
    }

    public bool EvaluateWait() => EvaluateImmediate();

    public ConditionDto ToDto() => new()
    {
        Type = "targetinrange",
        Params = new Dictionary<string, string>
        {
            ["data_id"] = _dataId.ToString(),
            ["radius"] = _radius.ToString("F1"),
            ["center_x"] = _centerX.ToString("F1"),
            ["center_y"] = _centerY.ToString("F1"),
            ["center_z"] = _centerZ.ToString("F1")
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ConditionFactory.Register(context, "targetinrange", dto =>
        {
            var c = new TargetInRangeCondition();
            if (dto.Params == null) return c;
            if (dto.Params.TryGetValue("data_id", out var d) && uint.TryParse(d, out var did)) c._dataId = did;
            if (dto.Params.TryGetValue("radius", out var r) && float.TryParse(r, out var rad)) c._radius = Math.Clamp(rad, 1f, 200f);
            if (dto.Params.TryGetValue("center_x", out var x) && float.TryParse(x, out var fx)) c._centerX = fx;
            if (dto.Params.TryGetValue("center_y", out var y) && float.TryParse(y, out var fy)) c._centerY = fy;
            if (dto.Params.TryGetValue("center_z", out var z) && float.TryParse(z, out var fz)) c._centerZ = fz;
            return c;
        });
    }
}
