using System.Collections.Generic;
using PromeRotation.Timeline.Core;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Timeline.Actions;

public class BardToggleDailyModeAction
    : IAction, ISerializableAction, IJobNodeDescriptor
{
    private bool _isDailyMode;

    public string NodeDisplayName => "Bard/切换模式（日随高难）";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("daily_mode", "日随模式",
            "true=日随模式, false=高难模式",
            "bool")
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "daily_mode" => _isDailyMode.ToString().ToLower(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        if (fieldName == "daily_mode")
            _isDailyMode = bool.TryParse(value, out var v) && v;
    }

    public void Execute()
    {
        BardSettings.Instance.IsDailyMode = _isDailyMode;
    }

    public ActionDto ToDto() => new()
    {
        Type = "bardtoggledailymode",
        Params = new Dictionary<string, string>
        {
            ["daily_mode"] = _isDailyMode.ToString().ToLower()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ActionFactory.Register(context, "bardtoggledailymode", dto =>
        {
            var a = new BardToggleDailyModeAction();
            if (dto.Params?.TryGetValue("daily_mode", out var raw) == true)
                a._isDailyMode = bool.TryParse(raw, out var v) && v;
            return a;
        });
    }
}
