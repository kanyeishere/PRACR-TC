using System.Collections.Generic;
using PromeRotation.Timeline.Core;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Timeline.Actions;

public class BardPotionModeAction
    : IAction, ISerializableAction, IJobNodeDescriptor
{
    private bool _useInOpener = true;

    public string NodeDisplayName => "Bard/爆发药模式";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("use_in_opener", "起手吃爆发药",
            "true=起手吃, false=两分钟爆发吃",
            "bool")
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "use_in_opener" => _useInOpener.ToString().ToLower(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        if (fieldName == "use_in_opener")
            _useInOpener = bool.TryParse(value, out var v) && v;
    }

    public void Execute()
    {
        BardSettings.Instance.UsePotionInOpener = _useInOpener;
    }

    public ActionDto ToDto() => new()
    {
        Type = "bardpotionmode",
        Params = new Dictionary<string, string>
        {
            ["use_in_opener"] = _useInOpener.ToString().ToLower()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ActionFactory.Register(context, "bardpotionmode", dto =>
        {
            var a = new BardPotionModeAction();
            if (dto.Params?.TryGetValue("use_in_opener", out var raw) == true)
                a._useInOpener = bool.TryParse(raw, out var v) && v;
            return a;
        });
    }
}
