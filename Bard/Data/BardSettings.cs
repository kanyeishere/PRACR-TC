using System.Text.Json;
using Dalamud.Game.ClientState.JobGauge.Enums;
using ECommons.Logging;
using PromeRotation.Config;

namespace WotouTC.Bard.Data;

public class BardSettings
{
    public sealed class CustomOpenerPreset
    {
        public string Name { get; set; } = "自定义起手";
        public List<uint> Skills { get; set; } = new();
    }

    private sealed class PersistedOpenerSettings
    {
        public bool UsePotionInOpener { get; set; }
        public int Opener { get; set; }
        public List<uint> CustomOpenerSkills { get; set; } = new();
        public List<CustomOpenerPreset> CustomOpeners { get; set; } = new();
        public int SelectedCustomOpenerIndex { get; set; }
    }

    private static readonly object SaveLock = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static BardSettings Instance { get; } = Load();
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

    // 保留旧版字段，便于从 AEAssist 配置迁移第一份自定义序列。
    public List<uint> CustomOpenerSkills { get; set; } = new();
    public List<CustomOpenerPreset> CustomOpeners { get; set; } = new();
    public int SelectedCustomOpenerIndex { get; set; } = 0;

    public bool ApplyDotOnTrashMobs { get; set; } = false;

    public float HeartBreakSaveStack { get; set; } = 0f;
    
    public void ResetSongOrderNormal()
    {
        FirstSong = Song.Wanderer;
        SecondSong = Song.Mage;
        ThirdSong = Song.Army;
    }

    public void EnsureCustomOpeners()
    {
        CustomOpeners ??= new List<CustomOpenerPreset>();
        CustomOpenerSkills ??= new List<uint>();
        CustomOpeners.RemoveAll(static preset => preset == null);

        if (CustomOpeners.Count == 0)
        {
            CustomOpeners.Add(new CustomOpenerPreset
            {
                Name = "自定义起手1",
                Skills = CustomOpenerSkills.Count > 0
                    ? new List<uint>(CustomOpenerSkills)
                    : new List<uint>()
            });
        }

        foreach (var preset in CustomOpeners)
        {
            preset.Name ??= string.Empty;
            preset.Skills ??= new List<uint>();
        }

        SelectedCustomOpenerIndex = Math.Clamp(
            SelectedCustomOpenerIndex,
            0,
            CustomOpeners.Count - 1);
    }

    public void Save()
    {
        EnsureCustomOpeners();

        try
        {
            var directory = ACRAuthorSetting.GetSettingsDirectory("Wotou-TC");
            var path = Path.Combine(directory, "BardSettings.json");
            var tempPath = path + ".tmp";

            lock (SaveLock)
            {
                var persisted = new PersistedOpenerSettings
                {
                    UsePotionInOpener = UsePotionInOpener,
                    Opener = Opener,
                    CustomOpenerSkills = CustomOpenerSkills,
                    CustomOpeners = CustomOpeners,
                    SelectedCustomOpenerIndex = SelectedCustomOpenerIndex
                };
                File.WriteAllText(tempPath, JsonSerializer.Serialize(persisted, JsonOptions));
                File.Move(tempPath, path, overwrite: true);
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"[Wotou-TC] 保存诗人设置失败: {ex}");
        }
    }

    private static BardSettings Load()
    {
        try
        {
            var directory = ACRAuthorSetting.GetSettingsDirectory("Wotou-TC");
            var path = Path.Combine(directory, "BardSettings.json");
            if (File.Exists(path))
            {
                var persisted = JsonSerializer.Deserialize<PersistedOpenerSettings>(File.ReadAllText(path), JsonOptions);
                if (persisted != null)
                {
                    var loaded = new BardSettings
                    {
                        UsePotionInOpener = persisted.UsePotionInOpener,
                        Opener = persisted.Opener,
                        CustomOpenerSkills = persisted.CustomOpenerSkills ?? new List<uint>(),
                        CustomOpeners = persisted.CustomOpeners ?? new List<CustomOpenerPreset>(),
                        SelectedCustomOpenerIndex = persisted.SelectedCustomOpenerIndex
                    };
                    loaded.EnsureCustomOpeners();
                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"[Wotou-TC] 读取诗人设置失败，将使用默认值: {ex}");
        }

        var defaults = new BardSettings();
        defaults.EnsureCustomOpeners();
        return defaults;
    }
}


