using PromeRotation.Data;
using PromeRotation.Timeline.Core;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

public sealed class DancerJobNodeProvider : IJobNodeProvider
{
    public void RegisterNodes(RotationNodeContext context)
    {
        DancerEspritCondition.Register(context);
        DancerFeatherCondition.Register(context);
        DancerQtAction.Register(context);
        DancerModeAction.Register(context);
        DancerPotionAction.Register(context);
        DancerThresholdAction.Register(context);
    }

    public IReadOnlyList<(string DisplayName, string Description, Func<ICondition> Create)> GetConditionDescriptors()
        => new (string, string, Func<ICondition>)[]
        {
            ("Dancer/判断伶俐", "伶俐量谱满足比较条件", () => new DancerEspritCondition()),
            ("Dancer/判断扇舞层数", "扇舞资源满足比较条件", () => new DancerFeatherCondition())
        };

    public IReadOnlyList<(string DisplayName, string Description, Func<IAction> Create)> GetActionDescriptors()
        => new (string, string, Func<IAction>)[]
        {
            ("Dancer/切换QT", "设置指定 QT", () => new DancerQtAction()),
            ("Dancer/切换模式", "切换日随/高难模式", () => new DancerModeAction()),
            ("Dancer/爆发药模式", "设置起手是否使用爆发药", () => new DancerPotionAction()),
            ("Dancer/资源阈值", "设置剑舞和提拉纳阈值", () => new DancerThresholdAction())
        };
}

internal abstract class DancerCompareCondition : ICondition, ISerializableCondition, IJobNodeDescriptor
{
    protected int Operator;
    protected int Value;
    protected abstract int Current { get; }
    protected abstract string Type { get; }
    public abstract string NodeDisplayName { get; }
    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("operator", "运算符", "比较方式", "enum", new[] { ("0", "<"), ("1", ">"), ("2", "<=") , ("3", ">="), ("4", "==") }),
        new NodeParamInfo("value", "阈值", "比较数值", "int")
    };
    public string GetParam(string fieldName) => fieldName == "operator" ? Operator.ToString() : fieldName == "value" ? Value.ToString() : "";
    public void SetParam(string fieldName, string value)
    {
        if (fieldName == "operator" && int.TryParse(value, out var op)) Operator = Math.Clamp(op, 0, 4);
        if (fieldName == "value" && int.TryParse(value, out var val)) Value = val;
    }
    public bool EvaluateImmediate() => Operator switch { 0 => Current < Value, 1 => Current > Value, 2 => Current <= Value, 3 => Current >= Value, 4 => Current == Value, _ => false };
    public bool EvaluateWait() => EvaluateImmediate();
    public ConditionDto ToDto() => new() { Type = Type, Params = new Dictionary<string, string> { ["operator"] = Operator.ToString(), ["value"] = Value.ToString() } };
}

internal sealed class DancerEspritCondition : DancerCompareCondition
{
    protected override int Current => DancerHelper.Esprit;
    protected override string Type => "danceresprit";
    public override string NodeDisplayName => "Dancer/判断伶俐";
    public static void Register(RotationNodeContext context) => ConditionFactory.Register(context, "danceresprit", dto => FromDto(new DancerEspritCondition(), dto));
    private static DancerEspritCondition FromDto(DancerEspritCondition condition, ConditionDto dto) { condition.SetFrom(dto); return condition; }
}

internal sealed class DancerFeatherCondition : DancerCompareCondition
{
    protected override int Current => DancerHelper.Feathers;
    protected override string Type => "dancerfeather";
    public override string NodeDisplayName => "Dancer/判断扇舞层数";
    public static void Register(RotationNodeContext context) => ConditionFactory.Register(context, "dancerfeather", dto => FromDto(new DancerFeatherCondition(), dto));
    private static DancerFeatherCondition FromDto(DancerFeatherCondition condition, ConditionDto dto) { condition.SetFrom(dto); return condition; }
}

internal static class DancerConditionExtensions
{
    public static void SetFrom(this DancerCompareCondition condition, ConditionDto dto)
    {
        if (dto.Params == null) return;
        if (dto.Params.TryGetValue("operator", out var op)) condition.SetParam("operator", op);
        if (dto.Params.TryGetValue("value", out var value)) condition.SetParam("value", value);
    }
}

internal abstract class DancerActionNode : IAction, ISerializableAction, IJobNodeDescriptor
{
    public abstract string NodeDisplayName { get; }
    public abstract string Type { get; }
    public virtual NodeParamInfo[] Params => Array.Empty<NodeParamInfo>();
    public virtual string GetParam(string fieldName) => "";
    public virtual void SetParam(string fieldName, string value) { }
    public abstract void Execute();
    public abstract ActionDto ToDto();
}

internal sealed class DancerQtAction : DancerActionNode
{
    private string _key = QTKey.Aoe;
    private bool _value;
    public override string NodeDisplayName => "Dancer/切换QT";
    public override string Type => "dancerqt";
    public override NodeParamInfo[] Params => new[] { new NodeParamInfo("key", "QT", "QT 名称", "string"), new NodeParamInfo("value", "开启", "true/false", "bool") };
    public override string GetParam(string fieldName) => fieldName == "key" ? _key : fieldName == "value" ? _value.ToString().ToLowerInvariant() : "";
    public override void SetParam(string fieldName, string value) { if (fieldName == "key") _key = value; if (fieldName == "value") _value = bool.TryParse(value, out var b) && b; }
    public override void Execute() => PromeSettings.Instance.SetQt(_key, _value);
    public override ActionDto ToDto() => new() { Type = Type, Params = new Dictionary<string, string> { ["key"] = _key, ["value"] = _value.ToString().ToLowerInvariant() } };
    public static void Register(RotationNodeContext context) => ActionFactory.Register(context, "dancerqt", dto => { var a = new DancerQtAction(); if (dto.Params != null) { if (dto.Params.TryGetValue("key", out var k)) a._key = k; if (dto.Params.TryGetValue("value", out var v)) a._value = bool.TryParse(v, out var b) && b; } return a; });
}

internal sealed class DancerModeAction : DancerActionNode
{
    private bool _daily;
    public override string NodeDisplayName => "Dancer/切换模式";
    public override string Type => "dancermode";
    public override NodeParamInfo[] Params => new[] { new NodeParamInfo("daily", "日随模式", "true=日随", "bool") };
    public override string GetParam(string fieldName) => _daily.ToString().ToLowerInvariant();
    public override void SetParam(string fieldName, string value) => _daily = bool.TryParse(value, out var b) && b;
    public override void Execute() => DancerSettings.Instance.IsDailyMode = _daily;
    public override ActionDto ToDto() => new() { Type = Type, Params = new Dictionary<string, string> { ["daily"] = _daily.ToString().ToLowerInvariant() } };
    public static void Register(RotationNodeContext context) => ActionFactory.Register(context, "dancermode", dto => { var a = new DancerModeAction(); if (dto.Params?.TryGetValue("daily", out var v) == true) a._daily = bool.TryParse(v, out var b) && b; return a; });
}

internal sealed class DancerPotionAction : DancerActionNode
{
    private bool _use;
    public override string NodeDisplayName => "Dancer/爆发药模式";
    public override string Type => "dancerpotion";
    public override NodeParamInfo[] Params => new[] { new NodeParamInfo("use_in_opener", "起手使用爆发药", "true=起手", "bool") };
    public override string GetParam(string fieldName) => _use.ToString().ToLowerInvariant();
    public override void SetParam(string fieldName, string value) => _use = bool.TryParse(value, out var b) && b;
    public override void Execute() => DancerSettings.Instance.UsePotionInOpener = _use;
    public override ActionDto ToDto() => new() { Type = Type, Params = new Dictionary<string, string> { ["use_in_opener"] = _use.ToString().ToLowerInvariant() } };
    public static void Register(RotationNodeContext context) => ActionFactory.Register(context, "dancerpotion", dto => { var a = new DancerPotionAction(); if (dto.Params?.TryGetValue("use_in_opener", out var v) == true) a._use = bool.TryParse(v, out var b) && b; return a; });
}

internal sealed class DancerThresholdAction : DancerActionNode
{
    private int _saber = 70;
    private int _tillana = 20;
    private int _last = 30;
    public override string NodeDisplayName => "Dancer/资源阈值";
    public override string Type => "dancerthreshold";
    public override NodeParamInfo[] Params => new[] { new NodeParamInfo("saber", "剑舞阈值", "50-100", "int"), new NodeParamInfo("tillana", "提拉纳阈值", "0-50", "int"), new NodeParamInfo("last", "末段提拉纳阈值", "0-50", "int") };
    public override string GetParam(string fieldName) => fieldName == "saber" ? _saber.ToString() : fieldName == "tillana" ? _tillana.ToString() : _last.ToString();
    public override void SetParam(string fieldName, string value) { if (!int.TryParse(value, out var v)) return; if (fieldName == "saber") _saber = Math.Clamp(v, 50, 100); if (fieldName == "tillana") _tillana = Math.Clamp(v, 0, 50); if (fieldName == "last") _last = Math.Clamp(v, 0, 50); }
    public override void Execute() { var s = DancerSettings.Instance; s.SaberDanceEspritThreshold = _saber; s.TillanaEspritThreshold = _tillana; s.TillanaLastGcdEspritThreshold = _last; }
    public override ActionDto ToDto() => new() { Type = Type, Params = new Dictionary<string, string> { ["saber"] = _saber.ToString(), ["tillana"] = _tillana.ToString(), ["last"] = _last.ToString() } };
    public static void Register(RotationNodeContext context) => ActionFactory.Register(context, "dancerthreshold", dto => { var a = new DancerThresholdAction(); if (dto.Params != null) { if (dto.Params.TryGetValue("saber", out var s)) a.SetParam("saber", s); if (dto.Params.TryGetValue("tillana", out var t)) a.SetParam("tillana", t); if (dto.Params.TryGetValue("last", out var l)) a.SetParam("last", l); } return a; });
}
