using ECommons.DalamudServices;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardWardensPaeanOffGcd : IDecisionResolver
{
    private const uint TheWardensPaean = BRDSkill.TheWardensPaean;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.AutoWardensPaean))
            return new CheckResult(false, "自动驱散QT关闭");

        if (TheWardensPaean.GetActionCooldown() > 0.05f)
            return new CheckResult(false, $"光阴神未就绪:{TheWardensPaean.GetActionCooldown() * 1000f:F0}ms");

        // 检查队伍中是否有可驱散的状态
        var partyMembers = PartyHelper.GetUIParty();
        foreach (var member in partyMembers)
        {
            foreach (var status in member.StatusList)
            {
                if (status.StatusId == 0) continue;
                // 通过Lumina检查是否是可驱散状态
                if (Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>().TryGetRow(status.StatusId, out var statusRow))
                {
                    if (statusRow.CanDispel && status.RemainingTime > 0)
                        return new CheckResult(true, $"自动驱散:{member.Name.TextValue}");
                }
            }
        }

        return new CheckResult(false, "无可驱散目标");
    }

    public PAction GetAction()
    {
        return new PAction(TheWardensPaean, ActionType.OffGcd, ActionTargetType.Target);
    }
}
