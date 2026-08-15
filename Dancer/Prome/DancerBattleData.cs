namespace WotouTC.Dancer.Data;

public sealed class DancerBattleData
{
    public static DancerBattleData Instance { get; } = new();
    public int TechnicalStepCount { get; set; }
    public int DanceOfTheDawnCount { get; set; }
    public bool HotkeyUseHighPrioritySlot { get; set; }
    public int LastWarningTime { get; set; }
    public bool EnableThreeOGcd { get; set; } = true;

    public void Reset()
    {
        TechnicalStepCount = 0;
        DanceOfTheDawnCount = 0;
        HotkeyUseHighPrioritySlot = false;
        LastWarningTime = 0;
        EnableThreeOGcd = true;
    }
}
