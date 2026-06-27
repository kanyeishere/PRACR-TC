using PromeRotation.Timeline.Core;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Timeline.Actions;

public class BardDotBlacklistAction
    : IAction, ISerializableAction, IJobNodeDescriptor
{
    private string _action = "add";
    private uint _dataId;
    private string _idList = "";

    public string NodeDisplayName => "Bard/DoT黑名单管理";

    public NodeParamInfo[] Params => new[]
    {
        new NodeParamInfo("action", "操作", "add=添加, remove=移除, clear=清空",
            "string", new[] {
                ("add", "添加"),
                ("remove", "移除"),
                ("clear", "清空"),
            }),
        new NodeParamInfo("data_id", "DataID", "要添加/移除的目标DataId（清空时忽略）", "uint"),
    };

    public string GetParam(string fieldName) => fieldName switch
    {
        "action" => _action,
        "data_id" => _dataId.ToString(),
        _ => ""
    };

    public void SetParam(string fieldName, string value)
    {
        switch (fieldName)
        {
            case "action": _action = value; break;
            case "data_id" when uint.TryParse(value, out var v): _dataId = v; break;
        }
    }

    public void Execute()
    {
        var list = BardBattleData.Instance.DotBlackList;
        switch (_action)
        {
            case "add" when _dataId > 0:
                list.Add(_dataId);
                break;
            case "remove" when _dataId > 0:
                list.Remove(_dataId);
                break;
            case "clear":
                list.Clear();
                break;
        }
    }

    public ActionDto ToDto() => new()
    {
        Type = "barddotblacklist",
        Params = new Dictionary<string, string>
        {
            ["action"] = _action,
            ["data_id"] = _dataId.ToString()
        }
    };

    public static void Register(RotationNodeContext context)
    {
        ActionFactory.Register(context, "barddotblacklist", dto =>
        {
            var a = new BardDotBlacklistAction();
            if (dto.Params?.TryGetValue("action", out var act) == true) a._action = act;
            if (dto.Params?.TryGetValue("data_id", out var did) == true && uint.TryParse(did, out var id))
                a._dataId = id;
            return a;
        });
    }
}
