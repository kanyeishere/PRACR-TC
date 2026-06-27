using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardPitchPerfectMaxOffGcd : IDecisionResolver
{
    private const uint PitchPerfect = BRDSkill.PitchPerfect;

    public CheckResult Check()
    {
        if (Core.Core.Target == null)
            return new CheckResult(false, "当前无目标");
        if (ActionHelper.GetGcdRemain() * 1000f <= 650f)
            return new CheckResult(false, "GCD窗口不足");
        if (!BardHelper.IsUnlocked(PitchPerfect))
            return new CheckResult(false, "完美音调未解锁");
        if (BardHelper.CurrentSong != Song.WanderersMinuet)
            return new CheckResult(false, "当前不是旅神歌");
        if (TargetHelper.EnemyInRangeTarget(Core.Core.Target, 5) < BardBattleData.Instance.PitchPerfectMinEnemyCount)
            return new CheckResult(false, "目标数量不足");
        if (JobGaugeHelper.BRD.GetRepertoire == 3 && JobGaugeHelper.BRD.GetCurrentSongTimer % 3000 < 2600)
            return new CheckResult(true, "三层诗心完美音调");

        return new CheckResult(false, $"诗心不足:{JobGaugeHelper.BRD.GetRepertoire}");
    }

    public PAction GetAction()
    {
        return new PAction(PitchPerfect, ActionType.OffGcd, ActionTargetType.Target);
    }
}

