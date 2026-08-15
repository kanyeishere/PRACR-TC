using Dalamud.Bindings.ImGui;
using ECommons.ExcelServices;
using ECommons.Logging;
using PromeRotation;
using PromeRotation.Data;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using PromeRotation.Rotation;
using PromeRotation.Timeline;
using PromeRotation.Timeline.Core;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

[RotationMetadata((uint)Job.DNC, "舞者", "Wotou-TC", "1.0.0.0")]
public sealed class DancerRotation : IRotation
{
    public string RotationName => "舞者";
    public uint JobId => (uint)Job.DNC;
    public static IJobNodeProvider? NodeProvider { get; } = new DancerJobNodeProvider();

    private readonly List<IDecisionResolver> _gcdResolvers = new();
    private readonly List<IDecisionResolver> _offGcdResolvers = new();

    public static IReadOnlyDictionary<string, bool> QtList { get; } = new Dictionary<string, bool>
    {
        [QTKey.UsePotion] = false,
        [QTKey.Aoe] = true,
        [QTKey.StrongAlign] = true,
        [QTKey.TechnicalStep] = true,
        [QTKey.StandardStep] = true,
        [QTKey.Flourish] = true,
        [QTKey.SaberDance] = true,
        [QTKey.FanDance] = true,
        [QTKey.AutoCuringWaltz] = true,
        [QTKey.FinalBurst] = false,
        [QTKey.SmartAoeTarget] = false
    };

    public static IReadOnlyDictionary<string, Type> Openers { get; } = new Dictionary<string, Type>
    {
        ["标准舞起手"] = typeof(DancerStandardOpener),
        ["技巧舞起手"] = typeof(DancerTechnicalOpener)
    };

    public DancerRotation()
    {
        _gcdResolvers.AddRange(new IDecisionResolver[]
        {
            new DancerLastDanceHighGcd(),
            new DancerTechnicalStepDancingGcd(),
            new DancerTechnicalStepGcd(),
            new DancerFinishingMoveGcd(),
            new DancerStandardStepDancingGcd(),
            new DancerStandardStepGcd(),
            new Dancer1GBeforeTechStepGcd(),
            new DancerStarfallDanceHighGCD(),
            new DancerProcFountainFallHighGcd(),
            new DancerProcReverseCascadeHighGcd(),
            new DancerSaberDanceHighGcd(),
            new DancerTillanaGcd(),
            new DancerProcFountainFallMediumGcd(),
            new DancerProcReverseCascadeMediumGcd(),
            new DancerStarfallDanceGCD(),
            new DancerSaberDanceMediumGcd(),
            new DancerSaberDanceGcd(),
            new DancerLastDanceGcd(),
            new DancerProcGcd(),
            new DancerBaseGcd()
        });

        _offGcdResolvers.AddRange(new IDecisionResolver[]
        {
            new DancerPotionAbility(),
            new DancerDevilmentAbility(),
            new DancerFlourishAbility(),
            new DancerFanDance3Ability(),
            new DancerFanDanceAbility(),
            new DancerFanDance4Ability(),
            new DancerCuringWaltzAbility()
        });

        foreach (var (key, value) in QtList)
            PromeSettings.Instance.AddQt(key, value);

        DancerHotkeyUI.Setup();
    }

    public PAction? NextAlways() => null;
    public PAction? NextGcd() => Resolve(_gcdResolvers);
    public PAction? NextOffGcd() => Resolve(_offGcdResolvers);

    private static PAction? Resolve(IEnumerable<IDecisionResolver> resolvers)
    {
        foreach (var resolver in resolvers)
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
        AddStatuses(_gcdResolvers, RotationManager.GcdSolverStatus);
        AddStatuses(_offGcdResolvers, RotationManager.OffGcdSolverStatus);
    }

    private static void AddStatuses(IEnumerable<IDecisionResolver> resolvers, ICollection<SolverStatus> output)
    {
        foreach (var resolver in resolvers)
        {
            var result = resolver.Check();
            output.Add(new SolverStatus { Name = resolver.GetType().Name, Success = result.Success, Message = result.Message });
        }
    }

    public IOpener? GetOpener()
    {
        var openerName = PromeRotation.PureTimeline.PtlManager.CurrentOpener;
        var source = "PureTimeline";
        if (string.IsNullOrWhiteSpace(openerName))
        {
            openerName = TimelineManager.CurrentMeta?.Opener;
            source = "Timeline";
        }

        if (!string.IsNullOrWhiteSpace(openerName) && Openers.TryGetValue(openerName, out var openerType))
        {
            try
            {
                if (Activator.CreateInstance(openerType) is IOpener opener)
                {
                    PluginLog.Information($"[Wotou-TC/DNC] 从{source}加载起手：{openerName}");
                    return opener;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"[Wotou-TC/DNC] 创建起手失败：{ex}");
            }
        }

        return DancerSettings.Instance.OpenerType == DancerOpenerType.TechnicalStep
            ? new DancerTechnicalOpener()
            : new DancerStandardOpener();
    }

    public IRotationEventHandler GetEventHandler() => new DancerRotationEventHandler();

    public void DrawSettings()
    {
        if (!ImGui.BeginTabBar("Settings##Dancer")) return;
        if (ImGui.BeginTabItem("循环设置"))
        {
            DancerSettingsUI.Draw();
            ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }

    public void DrawQTs() { }
}
