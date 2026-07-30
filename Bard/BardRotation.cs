using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.JobGauge.Enums;
using ECommons.ExcelServices;
using ECommons.Logging;
using PromeRotation;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using PromeRotation.Rotation;
using PromeRotation.Timeline;
using PromeRotation.Timeline.Core;
using Wotou.Bard.Action;
using WotouTC.Bard.Action;
using WotouTC.Bard.Data;
using WotouTC.Bard.Opener;
using WotouTC.Bard.UI;

namespace WotouTC.Bard;

[RotationMetadata((uint)Job.BRD, "诗人", "Wotou-TC", "1.0.0.0")]
public class BardRotation : IRotation
{
    public string RotationName => "诗人";
    public uint JobId => (uint)Job.BRD;
    public static IJobNodeProvider? NodeProvider { get; } = new BardJobNodeProvider();

    private readonly List<IDecisionResolver> _gcdResolvers = new();
    private readonly List<IDecisionResolver> _offGcdResolvers = new();

    public BardRotation()
    {
        _offGcdResolvers.Add(new BardPotionOffGcd());
        _offGcdResolvers.Add(new BardWardensPaeanOffGcd());
        _offGcdResolvers.Add(new BardNaturesMinneOffGcd());
        _offGcdResolvers.Add(new BardRadiantFinaleOffGcd());
        _offGcdResolvers.Add(new BardRagingStrikesOffGcd());
        _offGcdResolvers.Add(new BardBattleVoiceOffGcd());
        _offGcdResolvers.Add(new BardEmpyrealArrowOffGcd());
        _offGcdResolvers.Add(new BardPitchPerfectMaxOffGcd());
        _offGcdResolvers.Add(new BardHeartBreakMaxChargeOffGcd());
        _offGcdResolvers.Add(new BardBarrageOffGcd());
        _offGcdResolvers.Add(new BardSidewinderOffGcd());
        _offGcdResolvers.Add(new BardPitchPerfectOffGcd());
        _offGcdResolvers.Add(new BardSongAbility());
        _offGcdResolvers.Add(new BardHeartBreakOffGcd());

        _gcdResolvers.Add(new BurstingAfterDeathSequence());
        _gcdResolvers.Add(new BardBlastArrowMaxGcd());
        _gcdResolvers.Add(new BardIronJawsGcd());
        _gcdResolvers.Add(new BardApexMaxGcd());
        _gcdResolvers.Add(new BardRadiantEncoreMaxGcd());
        _gcdResolvers.Add(new BardResonantArrowMaxGcd());
        _gcdResolvers.Add(new BardBarrageBuffMaxGcd());
        _gcdResolvers.Add(new BardDotGcd());
        _gcdResolvers.Add(new BardApexWithoutBurstGcd());
        _gcdResolvers.Add(new BardRefulgentArrowMaxGcd());
        _gcdResolvers.Add(new BardApexGcd());
        _gcdResolvers.Add(new BardBlastArrowGcd());
        _gcdResolvers.Add(new BardRadiantEncoreGcd());
        _gcdResolvers.Add(new BardResonantArrowGcd());
        _gcdResolvers.Add(new BaseGcd());

        foreach (var (name, def) in QtList)
            PromeSettings.Instance.AddQt(name, def);

        BardHotkeyUI.SetupHotkeyPanel();
    }

    public static IReadOnlyDictionary<string, bool> QtList { get; } = new Dictionary<string, bool>
    {
        { BRDQt.Burst, true },
        { BRDQt.Potion, false },
        { BRDQt.Apex, true },
        { BRDQt.DOT, true },
        { BRDQt.Song, true },
        { BRDQt.BurstWithWanderer, true },
        { BRDQt.EmpyrealArrow, true },
        { BRDQt.Sidewinder, true },
        { BRDQt.HeartBreakSave, true },
        { BRDQt.ClearHawkEyesBuffBeforeDots, true },
        { BRDQt.NatureMinne, true },
        { BRDQt.AutoWardensPaean, true },
        { BRDQt.AOE, false }
    };

    public static IReadOnlyDictionary<string, Type> Openers { get; } = new Dictionary<string, Type>
    {
        {"90-100级 3G团辅起手", typeof(Bard3GOpener100)},
        {"90-100级 2G团辅起手", typeof(Bard2GOpener100)},
        {"100级 1G团辅起手", typeof(Bard1GOpener100)},
        {"70-80级 3G团辅起手", typeof(Bard3GOpener7080)},
        {"70级 5G团辅起手", typeof(Bard5GOpener70)},
        {"100级 FR起手", typeof(BardFROpener100)}
    };

    public PAction? NextAlways() => null;

    public PAction? NextGcd()
    {
        foreach (var resolver in _gcdResolvers)
        {
            if (resolver.Check().Success)
                return resolver.GetAction();
        }
        return null;
    }

    public PAction? NextOffGcd()
    {
        foreach (var resolver in _offGcdResolvers)
        {
            if (resolver.Check().Success)
                return resolver.GetAction();
        }
        return null;
    }

    public void UpdateDebugStatus()
    {
        RotationManager.GcdSolverStatus.Clear();
        RotationManager.OffGcdSolverStatus.Clear();

        foreach (var resolver in _gcdResolvers)
        {
            var result = resolver.Check();
            RotationManager.GcdSolverStatus.Add(new SolverStatus
            {
                Name = resolver.GetType().Name,
                Success = result.Success,
                Message = result.Message
            });
        }

        foreach (var resolver in _offGcdResolvers)
        {
            var result = resolver.Check();
            RotationManager.OffGcdSolverStatus.Add(new SolverStatus
            {
                Name = resolver.GetType().Name,
                Success = result.Success,
                Message = result.Message
            });
        }

        RotationManager.OffGcdSolverStatus.Add(BardNoTargetSongSwitcher.GetStatus());
    }

    public IOpener? GetOpener()
    {
        // 起手来源优先级：时间轴（PTL / 旧 Timeline）的 Meta.Opener > 设置页“起手”选择。
        // 时间轴指定的起手若存在，则覆盖设置页；这样本体“当前使用起手”显示与实际执行保持同源。
        if (TryCreateTimelineOpener(out var timelineOpener))
            return timelineOpener;

        return CreateOpenerBySettings(BardSettings.Instance.Opener);
    }

    private static bool TryCreateTimelineOpener(out IOpener? opener)
    {
        var openerName = PromeRotation.PureTimeline.PtlManager.CurrentOpener;
        var openerSource = "PureTimeline";

        if (string.IsNullOrWhiteSpace(openerName))
        {
            openerName = TimelineManager.CurrentMeta?.Opener;
            openerSource = "Timeline";
        }

        if (string.IsNullOrWhiteSpace(openerName))
        {
            opener = null;
            return false;
        }

        if (!Openers.TryGetValue(openerName, out var openerType))
        {
            PluginLog.Warning($"[ACR] {openerSource} 指定起手不存在：{openerName}");
            opener = null;
            return false;
        }

        try
        {
            if (Activator.CreateInstance(openerType) is IOpener created)
            {
                PluginLog.Information($"[ACR] 从{openerSource}加载起手：{openerName}");
                opener = created;
                return true;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"[ACR] 创建起手实例失败: {ex.Message}");
        }

        opener = null;
        return false;
    }

    // 设置页的起手 index 与 Openers 字典顺序不一致（5=自定义起手在字典中无对应，FR 为 6），
    // 因此这里保留显式映射，而不是按字典下标取。
    private static IOpener CreateOpenerBySettings(int openerIndex)
    {
        return openerIndex switch
        {
            0 => new Bard3GOpener100(),
            1 => new Bard2GOpener100(),
            2 => new Bard1GOpener100(),
            3 => new Bard3GOpener7080(),
            4 => new Bard5GOpener70(),
            6 => new BardFROpener100(),
            _ => new Bard3GOpener100()
        };
    }

    public IRotationEventHandler GetEventHandler()
    {
        return new BardRoationEventHandler();
    }

    public void DrawSettings()
    {
        if (!ImGui.BeginTabBar("Settings##Bard")) return;

        if (ImGui.BeginTabItem("歌轴"))
        {
            BardSongUI.DrawSongSettings();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("起手"))
        {
            BardOpenerUI.DrawOpenerSettings();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    public void DrawQTs()
    {
    }
}

