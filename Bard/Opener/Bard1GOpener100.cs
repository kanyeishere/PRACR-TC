using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;
using WotouTC.Bard.Data;

namespace WotouTC.Bard.Opener;

public class Bard1GOpener100 : IOpener
{
    public string OpenerName => "100级 1G团辅起手";

    public void InitializeCountdown(CountDownHandler countdownHandler)
    {
    }

    public List<PAction> InCombatSequence => BuildSequence();

    private static List<PAction> BuildSequence()
    {
        var actions = new List<PAction>
        {
            // 1G: 风+战歌+光明神+第一首歌
            Gcd(BRDSkill.Windbite)
        };
        actions.Add(OffGcd(BRDSkill.BattleVoice, ActionTargetType.Self));
        AddFirstReadySong(actions);
        AddIfUnlocked(actions, BRDSkill.RadiantFinale, ActionType.OffGcd, ActionTargetType.Self);

        // 2G: 毒+爆发药+碎心+猛者
        actions.Add(Gcd(BRDSkill.VenomousBite));
        AddPotionIfEnabled(actions);
        actions.Add(OffGcd(GetHeartBreakActionId(), ActionTargetType.Target));
        actions.Add(OffGcd(BRDSkill.RagingStrikes, ActionTargetType.Self));

        // 3G: 返场+九天
        actions.Add(BardHelper.IsUnlocked(BRDSkill.RadiantEncore)
                        ? Gcd(BRDSkill.RadiantEncore)
                        : BardHelper.GetBaseGcd());
        AddIfUnlocked(actions, BRDSkill.EmpyrealArrow, ActionType.OffGcd, ActionTargetType.Target);

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
