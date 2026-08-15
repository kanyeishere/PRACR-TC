using System.Text.Json;
using ECommons.Logging;
using PromeRotation.Config;

namespace WotouTC.Dancer.Data;

public enum DancerOpenerType
{
    StandardStep,
    TechnicalStep
}

public sealed class DancerSettings
{
    private static readonly JsonSerializerOptions Options = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private static readonly object SaveLock = new();
    public static DancerSettings Instance { get; } = Load();

    public bool UsePotionInOpener = false;
    public bool IsDailyMode = false;
    public bool EnableAutoDancing = true;
    public bool EnableAutoDancePartner = true;
    public bool EnableAutoPeloton = true;
    public bool EnableAutoDancePartnerInFullAutoMode = true;
    public bool UseDancePartnerMacro = true;
    public string DancePartnerMacroText = "/p <t> 闭式舞姿";
    public DancerOpenerType OpenerType = DancerOpenerType.StandardStep;
    public int OpenerTime = 300;
    public int OpenerStandardStepTime = 15000;
    public int OpenerTechnicalStepTime = 5800;
    public int StandardStepCdTolerance = 1000;
    public int SaberDanceEspritThreshold = 70;
    public int TillanaEspritThreshold = 20;
    public int TillanaLastGcdEspritThreshold = 30;
    public int FanDanceSaveStack = 3;
    public int M6SAutoTargetCount = 1;

    public void Save()
    {
        try
        {
            var directory = ACRAuthorSetting.GetSettingsDirectory("Wotou-TC");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "DancerSettings.json");
            var temp = path + ".tmp";
            lock (SaveLock)
            {
                File.WriteAllText(temp, JsonSerializer.Serialize(this, Options));
                File.Move(temp, path, true);
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"[Wotou-TC] 保存舞者设置失败: {ex}");
        }
    }

    private static DancerSettings Load()
    {
        try
        {
            var path = Path.Combine(ACRAuthorSetting.GetSettingsDirectory("Wotou-TC"), "DancerSettings.json");
            if (File.Exists(path))
                return JsonSerializer.Deserialize<DancerSettings>(File.ReadAllText(path), Options) ?? new();
        }
        catch (Exception ex)
        {
            PluginLog.Error($"[Wotou-TC] 读取舞者设置失败，将使用默认值: {ex}");
        }

        return new DancerSettings();
    }
}
