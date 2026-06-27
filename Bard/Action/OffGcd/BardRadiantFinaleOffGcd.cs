using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardRadiantFinaleOffGcd : IDecisionResolver
{
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint RadiantFinale = BRDSkill.RadiantFinale;

    public CheckResult Check()
    {
        if (!PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "QT爆发关闭");

        if (!BardHelper.IsUnlocked(RadiantFinale))
            return new CheckResult(false, "光明神未解锁");

        var cooldownMs = RadiantFinale.GetActionCooldown() * 1000f;
        if (cooldownMs > 50f)
            return new CheckResult(false, $"光明神CD中:{cooldownMs:F0}ms");

        if (BardHelper.HasSelfStatus(BRDBuff.RadiantFinale))
            return new CheckResult(false, "已有光明神Buff");

        if (!BardHelper.RecentlyUsed(BattleVoice, 2500) &&
            !BardHelper.HasSelfStatus(BRDBuff.BattleVoice))
            return new CheckResult(false, "等待战斗之声后使用");

        return new CheckResult(true, "战歌后光明神");
    }

    public PAction GetAction()
    {
        return new PAction(RadiantFinale, ActionType.OffGcd, ActionTargetType.Self);
    }
}

