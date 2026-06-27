using PromeRotation.Timeline.Core;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Timeline.Actions;

public class BardHeartBreakSaveAction
    : IAction, ISerializableAction, IJobNodeDescriptor
{
    private float _stack = 0f;

    public string NodeDisplayName => "Bard/碎心箭保留层数";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("stack", "保留层数", "碎心箭保留层数 (0-3)", "float")
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "stack" => _stack.ToString("F1"),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        if (fieldName == "stack" && float.TryParse(value, out var v))
            _stack = Math.Clamp(v, 0f, 3f);
    }

    public void Execute()
    {
        BardSettings.Instance.HeartBreakSaveStack = _stack;
    }

    public ActionDto ToDto() => new()
    {
        Type = "bardheartbreaksave",
        Params = new Dictionary<string, string>
        {
            ["stack"] = _stack.ToString("F1")
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ActionFactory.Register(context, "bardheartbreaksave", dto =>
        {
            var a = new BardHeartBreakSaveAction();
            if (dto.Params?.TryGetValue("stack", out var raw) == true && float.TryParse(raw, out var v))
                a._stack = Math.Clamp(v, 0f, 3f);
            return a;
        });
    }
}
