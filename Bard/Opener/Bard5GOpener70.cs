using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Opener;

/// <summary>
/// 70级5G团辅起手（如神兵），按风→毒→基础→基础→基础+双团辅的顺序。
/// </summary>
public class Bard5GOpener70 : IOpener
{
    public string OpenerName => "70级 5G团辅起手";

    public void InitializeCountdown(CountDownHandler countdownHandler)
    {
    }

    public List<PAction> InCombatSequence => BuildSequence();

    private static List<PAction> BuildSequence()
    {
        var actions = new List<PAction>
        {
            // 1G: 风+第一首歌
            Gcd(BRDSkill.Windbite)
        };
        AddFirstReadySong(actions);

        // 2G: 毒+九天+碎心
        actions.Add(Gcd(BRDSkill.VenomousBite));
        AddIfUnlocked(actions, BRDSkill.EmpyrealArrow, ActionType.OffGcd, ActionTargetType.Target);
        actions.Add(OffGcd(GetHeartBreakActionId(), ActionTargetType.Target));

        // 3G: 基础GCD+爆发药
        actions.Add(BardHelper.GetBaseGcd());
        AddPotionIfEnabled(actions);

        // 4G: 基础GCD
        actions.Add(BardHelper.GetBaseGcd());

        // 5G: 基础GCD+战歌+猛者
        actions.Add(BardHelper.GetBaseGcd());
        actions.Add(OffGcd(BRDSkill.BattleVoice, ActionTargetType.Self));
        actions.Add(OffGcd(BRDSkill.RagingStrikes, ActionTargetType.Self));

        return actions;
    }

    private static void AddPotionIfEnabled(List<PAction> actions)
    {
        if (PromeSettings.Instance.GetQt(BRDQt.Potion) &&
            BardSettings.Instance.UsePotionInOpener &&
            GameData.GetBestPotionId() != 0)
            actions.Add(new PAction(GameData.GetBestPotionId(), ActionType.Item, ActionTargetType.Self));
    }

    private static void AddFirstReadySong(List<PAction> actions)
    {
        var settings = BardSettings.Instance;
        foreach (var song in new[] { settings.FirstSong, settings.SecondSong, settings.ThirdSong })
        {
            var songActionId = BardHelper.GetSpellBySong(song);
            if (!BardHelper.IsUnlocked(songActionId))
                continue;

            actions.Add(OffGcd(songActionId, ActionTargetType.Target));
            return;
        }
    }

    private static void AddIfUnlocked(List<PAction> actions, uint actionId, ActionType actionType, ActionTargetType target)
    {
        if (BardHelper.IsUnlocked(actionId))
            actions.Add(new PAction(BardHelper.Adjust(actionId), actionType, target));
    }

    private static uint GetHeartBreakActionId()
    {
        if (PromeSettings.Instance.GetQt(BRDQt.AOE) && BardHelper.IsUnlocked(BRDSkill.RainofDeath))
            return BRDSkill.RainofDeath;

        return BardHelper.IsUnlocked(BRDSkill.HeartBreak)
                   ? BRDSkill.HeartBreak
                   : BRDSkill.Bloodletter;
    }

    private static PAction Gcd(uint actionId)
    {
        return new PAction(BardHelper.Adjust(actionId), ActionType.Gcd, ActionTargetType.Target)
        {
            RequiresVerification = true
        };
    }

    private static PAction OffGcd(uint actionId, ActionTargetType target)
    {
        return new PAction(BardHelper.Adjust(actionId), ActionType.OffGcd, target);
    }
}
