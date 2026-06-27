using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Opener;

/// <summary>
/// FR零式(伊甸) 100级1G起手，会起手吃爆发药，风+九天+歌起手。
/// </summary>
public class BardFROpener100 : IOpener
{
    public string OpenerName => "100级 FR起手";

    public void InitializeCountdown(CountDownHandler countdownHandler)
    {
        // FR两件套可能需要在倒计时提前吃爆发药
        if (PromeSettings.Instance.GetQt(BRDQt.Potion) &&
            BardSettings.Instance.UsePotionInOpener)
        {
            var potionId = GameData.GetBestPotionId();
            if (potionId != 0)
                countdownHandler.AddAction(1000, () => new PAction(potionId, ActionType.Item, ActionTargetType.Self));
        }
    }

    public List<PAction> InCombatSequence => BuildSequence();

    private static List<PAction> BuildSequence()
    {
        var actions = new List<PAction>
        {
            // 1G: 风+九天+第一首歌
            Gcd(BRDSkill.Windbite)
        };
        AddIfUnlocked(actions, BRDSkill.EmpyrealArrow, ActionType.OffGcd, ActionTargetType.Target);
        AddFirstReadySong(actions);

        // 2G: 毒+碎心+猛者
        actions.Add(Gcd(BRDSkill.VenomousBite));
        actions.Add(OffGcd(GetHeartBreakActionId(), ActionTargetType.Target));
        actions.Add(OffGcd(BRDSkill.RagingStrikes, ActionTargetType.Self));

        // 3G: 基础GCD+战歌+光明神
        actions.Add(BardHelper.GetBaseGcd());
        actions.Add(OffGcd(BRDSkill.BattleVoice, ActionTargetType.Self));
        AddIfUnlocked(actions, BRDSkill.RadiantFinale, ActionType.OffGcd, ActionTargetType.Self);

        // 4G: 返场+完美音调(如果三层诗心)
        actions.Add(BardHelper.IsUnlocked(BRDSkill.RadiantEncore)
                        ? Gcd(BRDSkill.RadiantEncore)
                        : BardHelper.GetBaseGcd());

        return actions;
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
