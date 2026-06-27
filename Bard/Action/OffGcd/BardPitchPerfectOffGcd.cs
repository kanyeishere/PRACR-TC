using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardPitchPerfectOffGcd : IDecisionResolver
{
    private const uint PitchPerfect = BRDSkill.PitchPerfect;
    private const uint EmpyrealArrow = BRDSkill.EmpyrealArrow;
    private const uint MagesBallad = BRDSkill.MagesBallad;

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
        if (EmpyrealArrow.GetActionCooldown() * 1000f < 650f &&
            PromeSettings.Instance.GetQt(BRDQt.EmpyrealArrow) &&
            BardHelper.IsUnlocked(EmpyrealArrow))
            return new CheckResult(false, "等待九天连箭");
        if (TargetHelper.EnemyInRangeTarget(Core.Core.Target, 5) < BardBattleData.Instance.PitchPerfectMinEnemyCount)
            return new CheckResult(false, "目标数量不足");

        var repertoire = JobGaugeHelper.BRD.GetRepertoire;
        var songTimer = JobGaugeHelper.BRD.GetCurrentSongTimer;
        if (repertoire == 3)
            return new CheckResult(true, "三层诗心");
        if (songTimer <= 45600f - BardSettings.Instance.WandererSongDuration * 1000f && repertoire >= 1)
            return new CheckResult(true, "旅神最后一跳诗心");
        if (repertoire >= 1 && songTimer < 1000)
            return new CheckResult(true, "旅神即将结束");
        if (repertoire == 2 &&
            EmpyrealArrow.GetActionCooldown() * 1000f is < 2300f and > 600f &&
            !BardHelper.RecentlyUsed(EmpyrealArrow) &&
            songTimer >= 3000)
            return new CheckResult(true, "下个窗口预留九天");
        if (!BardHelper.HasSelfStatusWithTimeLeft(BardBattleData.Instance.First120SBuffId, 1000) &&
            BardHelper.HasSelfStatus(BardBattleData.Instance.First120SBuffId) &&
            repertoire >= 1)
            return new CheckResult(true, "团辅最后一个能力技");
        if (MagesBallad.GetActionCooldown() * 1000f <= 1200f &&
            repertoire >= 1 &&
            PromeSettings.Instance.GetQt(BRDQt.Song))
            return new CheckResult(true, "切贤者前完美音调");

        return new CheckResult(false, $"完美音调条件不满足 诗心:{repertoire}");
    }

    public PAction GetAction()
    {
        return new PAction(PitchPerfect, ActionType.OffGcd, ActionTargetType.Target);
    }
}

