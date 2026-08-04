using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Action;

public class BardHeartBreakOffGcd : IDecisionResolver
{
    private const uint HeartBreak = BRDSkill.HeartBreak;
    private const uint RainOfDeath = BRDSkill.RainofDeath;
    private const uint SecondSong = BRDSkill.MagesBallad;
    private const uint Sidewinder = BRDSkill.Sidewinder;
    private const uint Barrage = BRDSkill.Barrage;
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint EmpyrealArrow = BRDSkill.EmpyrealArrow;
    private const uint ArmysPaeon = BRDSkill.ArmysPaeon;
    private const uint MagesBallad = BRDSkill.MagesBallad;

    public CheckResult Check()
    {
        var actionId = BardHelper.Adjust(HeartBreak);
        var charges = ActionHelper.GetActionCharges(actionId);
        if (Core.Core.Target == null)
            return new CheckResult(false, "当前无目标");
        if (ActionHelper.GetGcdRemain() * 1000f <= 650f)
            return new CheckResult(false, "GCD窗口不足");
        if (charges < 1f)
            return new CheckResult(false, $"碎心箭未就绪:{charges:F1}层");
        if (EmpyrealArrow.GetActionCooldown() * 1000f < 1000f &&
            PromeSettings.Instance.GetQt(BRDQt.EmpyrealArrow) &&
            BardHelper.IsUnlocked(EmpyrealArrow))
            return new CheckResult(false, "等待九天连箭");
        if (BardHelper.RecentlyUsed(EmpyrealArrow, 650))
            return new CheckResult(false, "九天刚使用过");
        if (Sidewinder.GetActionCooldown() * 1000f < 650f &&
            PromeSettings.Instance.GetQt(BRDQt.Sidewinder) &&
            BardHelper.IsUnlocked(Sidewinder))
            return new CheckResult(false, "等待侧风诱导箭");
        if (RagingStrikes.GetActionCooldown() * 1000f < 3000f &&
            PromeSettings.Instance.GetQt(BRDQt.Burst) &&
            BardHelper.IsUnlocked(RagingStrikes))
            return new CheckResult(false, "等待猛者强击");
        if (BattleVoice.GetActionCooldown() * 1000f < 3000f &&
            PromeSettings.Instance.GetQt(BRDQt.Burst) &&
            BardHelper.IsUnlocked(BattleVoice))
            return new CheckResult(false, "等待战斗之声");
        if (Barrage.GetActionCooldown() * 1000f < 650f &&
            PromeSettings.Instance.GetQt(BRDQt.Burst) &&
            BardHelper.IsUnlocked(Barrage))
            return new CheckResult(false, "等待纷乱箭");

        var repertoire = JobGaugeHelper.BRD.GetRepertoire;
        // 不和两层诗心以上的完美音调冲突，抢团辅最后一个能力技能
        if (!BardHelper.HasSelfStatusWithTimeLeft(BardBattleData.Instance.First120SBuffId, 1200) &&
            BardHelper.HasSelfStatus(BardBattleData.Instance.First120SBuffId) &&
            repertoire >= 2)
            return new CheckResult(false, "让位团辅尾端完美音调");

        // 不和切贤者歌前最后一个完美音调冲突
        var wandererSongDuration = BardSettings.Instance.WandererSongDuration * 1000f;
        if (BardHelper.CurrentSong == Song.Wanderer &&
            BardHelper.SongTimerMs < 45600f - wandererSongDuration &&
            MagesBallad.GetActionCooldown() * 1000f <= 1200f &&
            PromeSettings.Instance.GetQt(BRDQt.Song))
            return new CheckResult(false, "让位切贤者前完美音调");

        // 不和切军神歌冲突
        var mageSongDuration = BardSettings.Instance.MageSongDuration * 1000f;
        if (BardHelper.CurrentSong == BardHelper.GetSongBySpell(SecondSong) &&
            BardHelper.SongTimerMs < 45600f - mageSongDuration &&
            ArmysPaeon.GetActionCooldown() * 1000f <= 600f &&
            PromeSettings.Instance.GetQt(BRDQt.Song))
            return new CheckResult(false, "让位切军神");

        // 满三层碎心箭，使用
        if (charges >= ActionHelper.GetMaxCharges(actionId) - 0.1f)
            return new CheckResult(true, "碎心箭满层");

        // 团辅期，使用
        if (BardHelper.HasAllPartyBuff())
            return new CheckResult(true, "团辅期碎心箭");

        if (PromeSettings.Instance.GetQt(BRDQt.HeartBreakSave) &&
            BardHelper.PartyBuffWillBeReadyIn(28500f) &&
            Core.Core.Me?.Level > 80)
            return new CheckResult(false, "团辅即将就绪，攒碎心箭");

        if (PromeSettings.Instance.GetQt(BRDQt.HeartBreakSave) &&
            BardHelper.PartyBuffWillBeReadyIn(13500f) &&
            Core.Core.Me?.Level <= 80)
            return new CheckResult(false, "团辅即将就绪，攒碎心箭");

        // 旅神期间，不和三层诗心的完美音调冲突
        if (repertoire == 3 && BardHelper.CurrentSong == Song.Wanderer)
            return new CheckResult(false, "让位三层诗心完美音调");

        if (repertoire == 2 &&
            EmpyrealArrow.GetActionCooldown() * 1000f < 2900f &&
            !BardHelper.RecentlyUsed(EmpyrealArrow))
            return new CheckResult(false, "等待九天触发诗心");

        // 设置保留碎心箭的层数 - 高难模式only
        if (charges <= BardSettings.Instance.HeartBreakSaveStack &&
            !BardSettings.Instance.IsDailyMode)
            return new CheckResult(false, $"保留碎心箭层数:{BardSettings.Instance.HeartBreakSaveStack}");

        return new CheckResult(true, "碎心箭");
    }

    public PAction GetAction()
    {
        return GetHeartBreakAction();
    }

    private static PAction GetHeartBreakAction()
    {
        if (TargetHelper.EnemyInRangeTarget(Core.Core.Target, 8) > 1 &&
            PromeSettings.Instance.GetQt(BRDQt.AOE) &&
            BardHelper.IsUnlocked(RainOfDeath))
            return new PAction(RainOfDeath, ActionType.OffGcd, ActionTargetType.Target);

        return new PAction(BardHelper.Adjust(HeartBreak), ActionType.OffGcd, ActionTargetType.Target);
    }
}


