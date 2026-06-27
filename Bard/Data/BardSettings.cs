using Dalamud.Game.ClientState.JobGauge.Enums;

namespace WotouTC.Bard.Data;

public class BardSettings
{
    public static BardSettings Instance { get; } = new();
    public float WandererSongDuration { get; set; } = 42.6f;
    public float MageSongDuration { get; set; } = 39.2f;
    public float ArmySongDuration { get; set; } = 39f;

    public Song FirstSong { get; set; } = Song.Wanderer;
    public Song SecondSong { get; set; } = Song.Mage;
    public Song ThirdSong { get; set; } = Song.Army;

    public int WandererBeforeGcdTime { get; set; } = 750;

    public int UseBattleVoiceBeforeGcdTimeInMs { get; set; } = 1350;

    public int RagingStrikeBeforeGcdTime { get; set; } = 750;

    public int PotionBeforeGcdTime { get; set; } = 700;

    public int GcdAnimationTime { get; set; } = 700;

    public int GcdTimeAfterBuff { get; set; } = 580;

    public bool UsePotionInOpener { get; set; } = false;

    public int Opener { get; set; } = 0;
    public bool IsDailyMode { get; set; } = false;

    public bool ApplyDotOnTrashMobs { get; set; } = false;

    public float HeartBreakSaveStack { get; set; } = 0f;
    
    public void ResetSongOrderNormal()
    {
        FirstSong = Song.Wanderer;
        SecondSong = Song.Mage;
        ThirdSong = Song.Army;
    }
}


