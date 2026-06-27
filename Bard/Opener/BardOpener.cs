using PromeRotation.Data;
using PromeRotation.Rotation;

namespace WotouTC.Bard.Opener;

public class BardOpener: IOpener
{
    public string OpenerName => "Test Opener";
    public void InitializeCountdown(CountDownHandler countdownHandler)
    {
    }

    public List<PAction> InCombatSequence => new()
    {
    };
}

