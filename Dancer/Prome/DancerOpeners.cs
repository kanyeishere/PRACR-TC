using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

public sealed class DancerStandardOpener : IOpener
{
    public string OpenerName => "标准舞起手";

    public void InitializeCountdown(CountDownHandler countdownHandler)
    {
        var settings = DancerSettings.Instance;
        countdownHandler.AddAction(settings.OpenerStandardStepTime,
            new PAction(DancerDefinesData.Spells.StandardStep, ActionType.Gcd, ActionTargetType.Self));
        countdownHandler.AddAction(settings.OpenerStandardStepTime - 1500, BuildStepAction);
        countdownHandler.AddAction(settings.OpenerStandardStepTime - 2500, BuildStepAction);
        if (PromeSettings.Instance.GetQt(QTKey.UsePotion) && settings.UsePotionInOpener && GameData.GetBestPotionId() != 0)
            countdownHandler.AddAction(1000, new PAction(GameData.GetBestPotionId(), ActionType.Item, ActionTargetType.Self));
        countdownHandler.AddAction(settings.OpenerTime,
            new PAction(DancerDefinesData.Spells.DoubleStandardFinish, ActionType.Gcd, ActionTargetType.Target));
    }

    public List<PAction> InCombatSequence => new()
    {
        new PAction(DancerDefinesData.Spells.TechnicalStep, ActionType.Gcd, ActionTargetType.Self)
    };

    private static PAction BuildStepAction() => DancerHelper.StepAction()
        ?? new PAction(DancerDefinesData.Spells.Emboite, ActionType.Gcd, ActionTargetType.Self);
}

public sealed class DancerTechnicalOpener : IOpener
{
    public string OpenerName => "技巧舞起手";

    public void InitializeCountdown(CountDownHandler countdownHandler)
    {
        var settings = DancerSettings.Instance;
        countdownHandler.AddAction(settings.OpenerTechnicalStepTime,
            new PAction(DancerDefinesData.Spells.TechnicalStep, ActionType.Gcd, ActionTargetType.Self));
        countdownHandler.AddAction(settings.OpenerTechnicalStepTime - 1500, BuildStepAction);
        countdownHandler.AddAction(settings.OpenerTechnicalStepTime - 2500, BuildStepAction);
        countdownHandler.AddAction(settings.OpenerTechnicalStepTime - 3500, BuildStepAction);
        countdownHandler.AddAction(settings.OpenerTechnicalStepTime - 4500, BuildStepAction);
        if (PromeSettings.Instance.GetQt(QTKey.UsePotion) && settings.UsePotionInOpener && GameData.GetBestPotionId() != 0)
            countdownHandler.AddAction(1000, new PAction(GameData.GetBestPotionId(), ActionType.Item, ActionTargetType.Self));
        countdownHandler.AddAction(settings.OpenerTime,
            new PAction(DancerDefinesData.Spells.QuadrupleTechnicalFinish, ActionType.Gcd, ActionTargetType.Target));
    }

    public List<PAction> InCombatSequence => new();

    private static PAction BuildStepAction() => DancerHelper.StepAction()
        ?? new PAction(DancerDefinesData.Spells.Emboite, ActionType.Gcd, ActionTargetType.Self);
}
