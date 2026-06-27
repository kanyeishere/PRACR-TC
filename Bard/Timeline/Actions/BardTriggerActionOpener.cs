using System;
using System.Collections.Generic;
using PromeRotation.Timeline.Core;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Timeline.Actions;

public class BardTriggerActionOpener : IAction, ISerializableAction, IJobNodeDescriptor
{
    private int _openerIndex;

    public string NodeDisplayName => "Bard/起手设置";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("opener", "起手选择",
            "Bard起手类型选择",
            "int",
            new[] {
                ("0", "3G团辅起手"),
                ("1", "2G团辅起手"),
                ("2", "1G团辅起手"),
                ("3", "70-80级 3G团辅起手"),
                ("4", "70级 5G团辅起手"),
                ("5", "自定义起手"),
                ("6", "FR起手"),
            })
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "opener" => _openerIndex.ToString(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        if (fieldName == "opener" && int.TryParse(value, out var v))
            _openerIndex = Math.Clamp(v, 0, 6);
    }

    public void Execute()
    {
        BardSettings.Instance.Opener = _openerIndex;
    }

    public ActionDto ToDto() => new()
    {
        Type = "bardtriggeractionopener",
        Params = new Dictionary<string, string>
        {
            ["opener"] = _openerIndex.ToString()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ActionFactory.Register(context, "bardtriggeractionopener", dto =>
        {
            var a = new BardTriggerActionOpener();
            if (dto.Params?.TryGetValue("opener", out var raw) == true && int.TryParse(raw, out var v))
                a._openerIndex = Math.Clamp(v, 0, 6);
            return a;
        });
    }
}
