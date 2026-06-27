using FFXIVClientStructs.FFXIV.Client.Game;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Bard.Data;
using GameActionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;

namespace WotouTC.Bard.Action;

public class BardPotionOffGcd : IDecisionResolver
{
    private const uint RagingStrikes = BRDSkill.RagingStrikes;
    private const uint BattleVoice = BRDSkill.BattleVoice;
    private const uint RadiantFinale = BRDSkill.RadiantFinale;
    private uint _potionId;

    public CheckResult Check()
    {
        _potionId = 0;

        if (!PromeSettings.Instance.GetQt(BRDQt.Burst))
            return new CheckResult(false, "QT爆发关闭");

        if (!PromeSettings.Instance.GetQt(BRDQt.Potion))
            return new CheckResult(false, "QT爆发药关闭");

        _potionId = GameData.GetBestPotionId();
        if (_potionId == 0)
            return new CheckResult(false, "背包无可用爆发药");

        var potionCooldownMs = GetPotionCooldownMs(_potionId);
        if (potionCooldownMs > 500f)
            return new CheckResult(false, $"爆发药CD中:{potionCooldownMs:F0}ms");

        if (BardHelper.RecentlyUsed(_potionId, 110000))
            return new CheckResult(false, "刚使用过爆发药");

        if (ShouldUseBeforeBurstWanderer(out var songReason))
            return new CheckResult(true, songReason);

        if (ShouldUseBeforeRaging(out var ragingReason))
            return new CheckResult(true, ragingReason);

        return new CheckResult(false, GetWaitReason());
    }

    public PAction GetAction()
    {
        BardBattleData.Instance.RecordAction(_potionId);
        return new PAction(_potionId, PromeRotation.Data.ActionType.Item, ActionTargetType.Self);
    }

    private static bool ShouldUseBeforeBurstWanderer(out string reason)
    {
        reason = string.Empty;

        if (!PromeSettings.Instance.GetQt(BRDQt.BurstWithWanderer))
            return false;

        if (!IsBattleVoiceFirstBurstOrder())
            return false;

        if (!BardSongHelper.ShouldSwitchSong(out var songActionId, out var songReason))
            return false;

        if (songActionId != BRDSkill.TheWanderersMinuet || !songReason.Contains("爆发对齐旅神"))
            return false;

        if (BattleVoice.GetActionCooldown() * 1000f > 1600f)
            return false;

        if (BardHelper.IsUnlocked(RadiantFinale) && RadiantFinale.GetActionCooldown() * 1000f > 4400f)
            return false;

        reason = $"战歌先轴:旅神前爆发药({songReason})";
        return true;
    }

    private static bool IsBattleVoiceFirstBurstOrder()
    {
        return BardBattleData.Instance.First120SBuffSpellId == BattleVoice;
    }

    private static bool ShouldUseBeforeRaging(out string reason)
    {
        reason = string.Empty;

        var ragingCooldownMs = RagingStrikes.GetActionCooldown() * 1000f;
        if (ragingCooldownMs > BardSettings.Instance.PotionBeforeGcdTime)
            return false;

        var gcdRemainMs = ActionHelper.GetGcdRemain() * 1000f;
        var useWindowMs = BardSettings.Instance.RagingStrikeBeforeGcdTime +
                          BardSettings.Instance.PotionBeforeGcdTime + 100f;
        if (gcdRemainMs > useWindowMs)
            return false;

        reason = $"猛者前爆发药 猛者:{ragingCooldownMs:F0}ms GCD:{gcdRemainMs:F0}ms";
        return true;
    }

    private static string GetWaitReason()
    {
        var ragingMs = RagingStrikes.GetActionCooldown() * 1000f;
        var battleVoiceMs = BattleVoice.GetActionCooldown() * 1000f;
        var finaleMs = RadiantFinale.GetActionCooldown() * 1000f;
        return $"等待爆发药窗口 猛者:{ragingMs:F0}ms 战歌:{battleVoiceMs:F0}ms 光明:{finaleMs:F0}ms";
    }

    private static unsafe float GetPotionCooldownMs(uint potionId)
    {
        var actionManager = ActionManager.Instance();
        if (actionManager == null)
            return 0f;

        var itemId = potionId > 1000000 ? potionId - 1000000 : potionId;
        var cooldown = actionManager->GetRecastTime(GameActionType.Item, itemId) -
                       actionManager->GetRecastTimeElapsed(GameActionType.Item, itemId);
        return Math.Max(0f, cooldown * 1000f);
    }
}

