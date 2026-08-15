using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using WotouTC.Dancer.Data;

namespace WotouTC.Dancer;

internal abstract class DancerResolver : IDecisionResolver
{
    protected static CheckResult Yes(string message) => new(true, message);
    protected static CheckResult No(string message) => new(false, message);
    protected static bool Qt(string key) => PromeSettings.Instance.GetQt(key);
    protected static bool Ready(uint actionId) => DancerHelper.IsReady(actionId);
    protected static float Cd(uint actionId) => DancerHelper.CooldownMs(actionId);
    protected static bool Buff(uint statusId) => DancerHelper.HasSelfStatus(statusId);
    protected static bool Buff(ushort statusId) => DancerHelper.HasSelfStatus(statusId);
    protected static bool BuffFor(uint statusId, int ms) => DancerHelper.HasSelfStatusWithTimeLeft(statusId, ms);
    protected static PAction Gcd(uint actionId) => DancerHelper.Gcd(actionId);
    protected static PAction AoEGcd(uint actionId) => DancerHelper.SmartGcd(actionId);
    protected static PAction Ability(uint actionId) => DancerHelper.OffGcd(actionId);
    public abstract CheckResult Check();
    public abstract PAction GetAction();
}

internal sealed class DancerPotionAbility : DancerResolver
{
    private uint _potion;
    public override CheckResult Check()
    {
        _potion = 0;
        if (!Qt(QTKey.UsePotion)) return No("爆发药QT关闭");
        if (DancerSettings.Instance.UsePotionInOpener) return No("爆发药设置为起手使用");
        if (!DancerHelper.IsDancing || !Buff(DancerDefinesData.Buffs.TechnicalStep) || DancerHelper.CompletedSteps < 2) return No("等待技巧舞第二步");
        if (DancerBattleData.Instance.TechnicalStepCount <= 1) return No("首轮技巧舞不使用二分钟药");
        _potion = GameData.GetBestPotionId();
        if (_potion == 0) return No("没有可用爆发药");
        if (DancerHelper.RecentlyUsed(_potion, 110000)) return No("爆发药冷却中");
        var itemId = _potion > 1_000_000 ? _potion - 1_000_000 : _potion;
        if (ActionHelper.GetItemCooldown(itemId) > .5f) return No("爆发药冷却中");
        return Yes("爆发药就绪");
    }
    public override PAction GetAction() => DancerHelper.Item(_potion);
}

internal sealed class DancerDevilmentAbility : DancerResolver
{
    public override CheckResult Check()
    {
        const uint action = DancerDefinesData.Spells.Devilment;
        if (!Ready(action)) return No("进攻之探戈未就绪");
        var finish = DancerDefinesData.Spells.QuadrupleTechnicalFinish;
        if (DancerHelper.RecentlyUsed(finish, 15000) || DancerHelper.RecentlyUsed(DancerDefinesData.Spells.TripleTechnicalFinish, 15000) ||
            DancerHelper.RecentlyUsed(DancerDefinesData.Spells.DoubleTechnicalFinish, 15000) || DancerHelper.RecentlyUsed(DancerDefinesData.Spells.SingleTechnicalFinish, 15000))
            return Yes("舞步结束后使用探戈");
        if (DancerHelper.IsUnlocked(DancerDefinesData.Spells.TechnicalStep)) return No("等待技巧舞步");
        return Yes("低等级直接使用探戈");
    }
    public override PAction GetAction() => Ability(DancerDefinesData.Spells.Devilment);
}

internal sealed class DancerFlourishAbility : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Ready(DancerDefinesData.Spells.Flourish)) return No("百花未就绪");
        if (!Qt(QTKey.Flourish)) return No("百花QT关闭");
        if (ActionHelper.GetGcdRemain() * 1000f <= 650f) return No("动画锁窗口不足");
        if (Cd(DancerDefinesData.Spells.TechnicalStep) < 10000 && Qt(QTKey.TechnicalStep)) return No("等待技巧舞步");
        if (DancerHelper.RecentlyUsed(DancerDefinesData.Spells.QuadrupleTechnicalFinish, 1500)) return No("刚结束技巧舞");
        if (Buff(DancerDefinesData.Buffs.ThreeFoldFanDance) || Ready(DancerDefinesData.Spells.FanDance3)) return No("优先处理扇舞资源");
        return Yes("百花就绪");
    }
    public override PAction GetAction() => Ability(DancerDefinesData.Spells.Flourish);
}

internal sealed class DancerFanDance3Ability : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Qt(QTKey.FanDance)) return No("扇舞QT关闭");
        if (!Ready(DancerDefinesData.Spells.FanDance3)) return No("扇舞III未就绪");
        if (ActionHelper.GetGcdRemain() * 1000f <= 650f) return No("动画锁窗口不足");
        if (DancerHelper.RecentlyUsed(DancerDefinesData.Spells.QuadrupleTechnicalFinish, 1500)) return No("刚结束技巧舞");
        if (Qt(QTKey.FinalBurst)) return Yes("倾泻扇舞III");
        if (Buff(DancerDefinesData.Buffs.ThreeFoldFanDance) && !BuffFor(DancerDefinesData.Buffs.ThreeFoldFanDance, 3500)) return Yes("扇舞III即将过期");
        if (Cd(DancerDefinesData.Spells.Flourish) <= 5000 && DancerHelper.IsUnlocked(DancerDefinesData.Spells.Flourish)) return Yes("百花前释放扇舞III");
        if (Buff(DancerDefinesData.Buffs.ThreeFoldFanDance) &&
            !BuffFor(DancerDefinesData.Buffs.ThreeFoldFanDance, 7500) &&
            Cd(DancerDefinesData.Spells.StandardStep) < 2500 &&
            !Buff(DancerDefinesData.Buffs.FinishingMoveReady)) return Yes("小舞前释放扇舞III");
        if (Buff(DancerDefinesData.Buffs.ThreeFoldFanDance) &&
            DancerHelper.StatusLeftMs(DancerDefinesData.Buffs.ThreeFoldFanDance) < Cd(DancerDefinesData.Spells.Devilment) + 1600 &&
            DancerHelper.IsUnlocked(DancerDefinesData.Spells.Devilment)) return Yes("扇舞III无法保留到爆发");
        if (!Qt(QTKey.TechnicalStep)) return Yes("技巧舞QT关闭");
        if (Cd(DancerDefinesData.Spells.TechnicalStep) <= 3500 && DancerHelper.IsUnlocked(DancerDefinesData.Spells.TechnicalStep)) return No("保留给技巧舞爆发");
        if (Cd(DancerDefinesData.Spells.TechnicalStep) <= 6000 &&
            Buff(DancerDefinesData.Buffs.SilkenSymmetry) &&
            DancerHelper.IsUnlocked(DancerDefinesData.Spells.TechnicalStep)) return No("技巧舞前保留对称触发");
        if (DancerHelper.Feathers > DancerSettings.Instance.FanDanceSaveStack + 1) return Yes("扇舞资源即将溢出");
        if (DancerHelper.Feathers > DancerSettings.Instance.FanDanceSaveStack && HasAnyProcBuff()) return Yes("触发资源窗口");
        if (Buff(DancerDefinesData.Buffs.Devilment) || Buff(DancerDefinesData.Buffs.Medicated)) return Yes("扇舞III资源窗口");
        if (!DancerHelper.IsUnlocked(DancerDefinesData.Spells.Devilment)) return Yes("低等级释放扇舞III");
        return No("保留扇舞资源");
    }
    public override PAction GetAction() => DancerHelper.SmartOffGcd(DancerDefinesData.Spells.FanDance3);

    private static bool HasAnyProcBuff() =>
        Buff(DancerDefinesData.Buffs.FlourishingSymmetry) ||
        Buff(DancerDefinesData.Buffs.FlourshingFlow) ||
        Buff(DancerDefinesData.Buffs.SilkenFlow) ||
        Buff(DancerDefinesData.Buffs.SilkenSymmetry);
}

internal sealed class DancerFanDanceAbility : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Qt(QTKey.FanDance)) return No("扇舞QT关闭");
        if (ActionHelper.GetGcdRemain() * 1000f <= 650f) return No("动画锁窗口不足");
        var fan = DancerDefinesData.Spells.FanDance;
        var fan2 = DancerDefinesData.Spells.FanDance2;
        if (!Ready(fan) && !Ready(fan2)) return No("扇舞未就绪");
        if (DancerHelper.RecentlyUsed(DancerDefinesData.Spells.QuadrupleTechnicalFinish, 1500)) return No("刚结束技巧舞");
        if (Ready(DancerDefinesData.Spells.FanDance3) || Buff(DancerDefinesData.Buffs.ThreeFoldFanDance)) return No("优先扇舞III");
        if (Qt(QTKey.FinalBurst)) return Yes("倾泻扇舞资源");
        if (Cd(DancerDefinesData.Spells.TechnicalStep) <= 3500 && Qt(QTKey.TechnicalStep) && DancerHelper.IsUnlocked(DancerDefinesData.Spells.TechnicalStep)) return No("技巧舞前保留扇舞");
        if (Cd(DancerDefinesData.Spells.TechnicalStep) <= 6000 && Buff(DancerDefinesData.Buffs.SilkenSymmetry) && Qt(QTKey.TechnicalStep) && DancerHelper.IsUnlocked(DancerDefinesData.Spells.TechnicalStep)) return No("技巧舞前保留对称触发");
        if (Cd(DancerDefinesData.Spells.Flourish) <= 1000 && Qt(QTKey.Flourish) && DancerHelper.IsUnlocked(DancerDefinesData.Spells.Flourish)) return No("百花前保留扇舞");
        if (DancerHelper.Feathers > DancerSettings.Instance.FanDanceSaveStack + 1) return Yes("扇舞资源即将溢出");
        if (DancerHelper.Feathers > DancerSettings.Instance.FanDanceSaveStack && HasAnyProcBuff()) return Yes("触发资源窗口");
        if (Buff(DancerDefinesData.Buffs.Devilment)) return Yes("探戈爆发窗口");
        if (Buff(DancerDefinesData.Buffs.Medicated) &&
            Cd(DancerDefinesData.Spells.TechnicalStep) >= 15000 &&
            Qt(QTKey.TechnicalStep) &&
            DancerHelper.IsUnlocked(DancerDefinesData.Spells.TechnicalStep)) return Yes("爆发药窗口");
        if (!DancerHelper.IsUnlocked(DancerDefinesData.Spells.Devilment)) return Yes("低等级释放扇舞");
        return No("保留扇舞资源");
    }
    public override PAction GetAction() => Qt(QTKey.Aoe) && DancerHelper.NearbyEnemies(5) > 2 && Ready(DancerDefinesData.Spells.FanDance2)
        ? DancerHelper.OffGcd(DancerDefinesData.Spells.FanDance2, ActionTargetType.Target)
        : DancerHelper.OffGcd(DancerDefinesData.Spells.FanDance, ActionTargetType.Target);

    private static bool HasAnyProcBuff() =>
        Buff(DancerDefinesData.Buffs.FlourishingSymmetry) ||
        Buff(DancerDefinesData.Buffs.FlourshingFlow) ||
        Buff(DancerDefinesData.Buffs.SilkenFlow) ||
        Buff(DancerDefinesData.Buffs.SilkenSymmetry);
}

internal sealed class DancerFanDance4Ability : DancerResolver
{
    public override CheckResult Check() => ActionHelper.GetGcdRemain() * 1000f > 650f && Ready(DancerDefinesData.Spells.FanDance4) ? Yes("扇舞IV就绪") : No("扇舞IV未就绪");
    public override PAction GetAction() => DancerHelper.SmartOffGcd(DancerDefinesData.Spells.FanDance4, angle: 90f);
}

internal sealed class DancerCuringWaltzAbility : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Qt(QTKey.AutoCuringWaltz)) return No("自动华尔兹QT关闭");
        if (!Ready(DancerDefinesData.Spells.CuringWaltz)) return No("治疗之华尔兹未就绪");
        var me = Core.Core.Me;
        var injured = me == null ? 0 : PartyHelper.GetParty().Count(p => p.CurrentHp > 0 && p.MaxHp > 0 && (double)p.CurrentHp / p.MaxHp < .7 && System.Numerics.Vector3.Distance(me.Position, p.Position) <= 5);
        return injured >= 4 ? Yes($"低血量队员:{injured}") : No($"低血量队员不足:{injured}");
    }
    public override PAction GetAction() => Ability(DancerDefinesData.Spells.CuringWaltz);
}

internal sealed class DancerBaseGcd : DancerResolver
{
    public override CheckResult Check() => Yes("基础连击");
    public override PAction GetAction() => DancerHelper.BaseGcd();
}

internal sealed class DancerBaseHighGcd : DancerResolver
{
    public override CheckResult Check() => ActionHelper.GetComboLeftTime() * 1000f >= 3000 ? Yes("连击窗口") : No("连击窗口不足");
    public override PAction GetAction() => DancerHelper.BaseGcd();
}

internal sealed class DancerProcGcd : DancerResolver
{
    public override CheckResult Check()
    {
        var ready = new[] { DancerDefinesData.Spells.ReverseCascade, DancerDefinesData.Spells.Fountainfall, DancerDefinesData.Spells.RisingWindmill, DancerDefinesData.Spells.Bloodshower }.Any(Ready);
        if (!ready) return No("没有可用触发技能");
        if (!new[] { DancerDefinesData.Buffs.FlourishingSymmetry, DancerDefinesData.Buffs.FlourshingFlow, DancerDefinesData.Buffs.SilkenFlow, DancerDefinesData.Buffs.SilkenSymmetry }.Any(Buff)) return No("没有触发增益");
        return Yes("处理触发连击");
    }
    public override PAction GetAction() => DancerHelper.ProcGcd();
}

internal sealed class Dancer1GBeforeTechStepGcd : DancerResolver
{
    public override CheckResult Check() => Qt(QTKey.TechnicalStep) && DancerHelper.IsUnlocked(DancerDefinesData.Spells.TechnicalStep) && Cd(DancerDefinesData.Spells.TechnicalStep) < 2 * ActionHelper.GetGcdTotal() * 1000f - 200 ? Yes("技巧舞步前填充1G") : No("技巧舞步未临近");
    public override PAction GetAction()
    {
        var combo = ActionHelper.GetLastComboID();
        if (combo == DancerDefinesData.Spells.Cascade) return Gcd(DancerDefinesData.Spells.Fountain);
        if (combo == DancerDefinesData.Spells.Windmill && DancerHelper.CanUseAoe()) return Gcd(DancerDefinesData.Spells.Bladeshower);
        if (combo != DancerDefinesData.Spells.Fountain && combo != DancerDefinesData.Spells.Bladeshower)
            return DancerHelper.BaseGcd();
        if (Buff(DancerDefinesData.Buffs.SilkenFlow) || Buff(DancerDefinesData.Buffs.SilkenSymmetry))
            return DancerHelper.ProcGcd();
        if (DancerHelper.Esprit >= 50) return AoEGcd(DancerDefinesData.Spells.SaberDance);
        if (Buff(DancerDefinesData.Buffs.LastDanceReady)) return AoEGcd(DancerDefinesData.Spells.LastDance);
        return DancerHelper.BaseGcd();
    }
}

internal sealed class DancerTechnicalStepDancingGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Buff(DancerDefinesData.Buffs.TechnicalStep)) return No("不在技巧舞步");
        if (Buff(DancerDefinesData.Buffs.StandardStep) || !DancerHelper.IsDancing) return No("不是技巧舞步状态");
        return Yes("技巧舞步中");
    }
    public override PAction GetAction() => DancerHelper.CompletedSteps >= 4 ? DancerHelper.Gcd(DancerDefinesData.Spells.QuadrupleTechnicalFinish, ActionTargetType.Self) : DancerHelper.StepAction() ?? DancerHelper.Gcd(DancerDefinesData.Spells.TechnicalStep, ActionTargetType.Self);
}

internal sealed class DancerStandardStepDancingGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Buff(DancerDefinesData.Buffs.StandardStep) || Buff(DancerDefinesData.Buffs.TechnicalStep) || !DancerHelper.IsDancing) return No("不在标准舞步");
        return Yes("标准舞步中");
    }
    public override PAction GetAction() => DancerHelper.CompletedSteps >= 2 ? DancerHelper.Gcd(DancerDefinesData.Spells.DoubleStandardFinish, ActionTargetType.Self) : DancerHelper.StepAction() ?? DancerHelper.Gcd(DancerDefinesData.Spells.StandardStep, ActionTargetType.Self);
}

internal sealed class DancerTechnicalStepGcd : DancerResolver
{
    public override CheckResult Check()
    {
        var action = DancerDefinesData.Spells.TechnicalStep;
        if (!Qt(QTKey.TechnicalStep) || !DancerHelper.IsUnlocked(action) || DancerHelper.IsDancing) return No("技巧舞步不可用");
        if (Cd(DancerDefinesData.Spells.Devilment) - Cd(action) > 6500) return No("等待探戈对齐");
        if (Ready(action) || (Qt(QTKey.StrongAlign) && Cd(action) <= 1500)) return Yes("技巧舞步就绪");
        return No("技巧舞步CD中");
    }
    public override PAction GetAction() => DancerHelper.Gcd(DancerDefinesData.Spells.TechnicalStep, ActionTargetType.Self);
}

internal sealed class DancerStandardStepGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Qt(QTKey.StandardStep) || DancerHelper.IsDancing || !DancerHelper.IsUnlocked(DancerDefinesData.Spells.StandardStep)) return No("标准舞步不可用");
        if (Buff(DancerDefinesData.Buffs.FinishingMoveReady)) return No("等待落幕舞结束");
        if (Core.Core.Me?.Level == 90 &&
            ((Buff(DancerDefinesData.Buffs.TechnicalFinish) && !BuffFor(DancerDefinesData.Buffs.TechnicalFinish, 3700)) ||
             (Buff(DancerDefinesData.Buffs.Devilment) && !BuffFor(DancerDefinesData.Buffs.Devilment, 3700))))
            return No("技巧舞爆发保护");
        if (Core.Core.Me?.Level >= 96 &&
            Buff(DancerDefinesData.Buffs.TechnicalFinish) &&
            DancerHelper.StatusLeftMs(DancerDefinesData.Buffs.TechnicalFinish) - Cd(DancerDefinesData.Spells.Flourish) > 6000 &&
            Qt(QTKey.Flourish))
            return No("等待百花对齐");
        if (Cd(DancerDefinesData.Spells.StandardStep) <= DancerSettings.Instance.StandardStepCdTolerance || Ready(DancerDefinesData.Spells.StandardStep)) return Yes("标准舞步就绪");
        return No("标准舞步CD中");
    }
    public override PAction GetAction() => Ready(DancerDefinesData.Spells.FinishingMove) || Buff(DancerDefinesData.Buffs.FinishingMoveReady)
        ? DancerHelper.Gcd(DancerDefinesData.Spells.FinishingMove, ActionTargetType.Self)
        : DancerHelper.Gcd(DancerDefinesData.Spells.StandardStep, ActionTargetType.Self);
}

internal sealed class DancerFinishingMoveGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Qt(QTKey.StandardStep) || !Buff(DancerDefinesData.Buffs.FinishingMoveReady)) return No("没有落幕舞预备");
        if (!DancerHelper.IsUnlocked(DancerDefinesData.Spells.FinishingMove)) return No("落幕舞未解锁");
        return Ready(DancerDefinesData.Spells.FinishingMove) || Cd(DancerDefinesData.Spells.FinishingMove) <= DancerSettings.Instance.StandardStepCdTolerance ? Yes("落幕舞就绪") : No("落幕舞CD中");
    }
    public override PAction GetAction() => DancerHelper.Gcd(DancerDefinesData.Spells.FinishingMove, ActionTargetType.Self);
}

internal sealed class DancerLastDanceHighGcd : DancerResolver
{
    public override CheckResult Check() => Ready(DancerDefinesData.Spells.LastDance) && Buff(DancerDefinesData.Buffs.LastDanceReady) && Cd(DancerDefinesData.Spells.StandardStep) <= 3500 ? Yes("落幕舞即将过期") : No("落幕舞高优先级条件不足");
    public override PAction GetAction() => AoEGcd(DancerDefinesData.Spells.LastDance);
}

internal sealed class DancerLastDanceGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Ready(DancerDefinesData.Spells.LastDance) || !Buff(DancerDefinesData.Buffs.LastDanceReady)) return No("落幕舞不可用");
        if (Qt(QTKey.FinalBurst) || DancerSettings.Instance.IsDailyMode) return Yes("释放落幕舞");
        if (BuffFor(DancerDefinesData.Buffs.LastDanceReady, (int)Cd(DancerDefinesData.Spells.Devilment) + 2500) && Qt(QTKey.TechnicalStep)) return No("等待技巧舞爆发");
        return Yes("落幕舞资源释放");
    }
    public override PAction GetAction() => AoEGcd(DancerDefinesData.Spells.LastDance);
}

internal sealed class DancerStarfallDanceHighGCD : DancerResolver
{
    public override CheckResult Check() => Ready(DancerDefinesData.Spells.StarfallDance) && Buff(DancerDefinesData.Buffs.FlourishingStarfall) && !BuffFor(DancerDefinesData.Buffs.FlourishingStarfall, 3500) ? Yes("流星舞即将过期") : No("流星舞高优先级条件不足");
    public override PAction GetAction() => AoEGcd(DancerDefinesData.Spells.StarfallDance);
}

internal sealed class DancerStarfallDanceGCD : DancerResolver
{
    public override CheckResult Check() => Ready(DancerDefinesData.Spells.StarfallDance) && Buff(DancerDefinesData.Buffs.FlourishingStarfall) ? Yes("流星舞就绪") : No("没有流星舞预备");
    public override PAction GetAction() => AoEGcd(DancerDefinesData.Spells.StarfallDance);
}

internal sealed class DancerTillanaGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Ready(DancerDefinesData.Spells.Tillana)) return No("提拉纳未就绪");
        if (Buff(DancerDefinesData.Buffs.FlourishingFinish) && !BuffFor(DancerDefinesData.Buffs.FlourishingFinish, 3000)) return Yes("提拉纳即将过期");
        if (DancerHelper.NearbyEnemies(15) == 0) return No("没有范围内目标");
        if (Qt(QTKey.StandardStep) && Cd(DancerDefinesData.Spells.StandardStep) < 2 * ActionHelper.GetGcdTotal() * 1000f - 1000) return No("小舞前保护");
        if (Qt(QTKey.StandardStep) &&
            Cd(DancerDefinesData.Spells.StandardStep) < 3 * ActionHelper.GetGcdTotal() * 1000f - 1000 &&
            Buff(DancerDefinesData.Buffs.LastDanceReady)) return No("小舞前保留落幕舞");
        if (Buff(DancerDefinesData.Buffs.Devilment) && DancerHelper.Esprit <= DancerSettings.Instance.TillanaEspritThreshold) return Yes("爆发期提拉纳");
        if (!Buff(DancerDefinesData.Buffs.Devilment) && DancerHelper.Esprit <= 40) return Yes("资源接近溢出");
        if (Buff(DancerDefinesData.Buffs.Devilment) &&
            !BuffFor(DancerDefinesData.Buffs.Devilment, 2600) &&
            DancerHelper.Esprit <= DancerSettings.Instance.TillanaLastGcdEspritThreshold &&
            !Buff(DancerDefinesData.Buffs.SilkenFlow) &&
            !Buff(DancerDefinesData.Buffs.SilkenSymmetry)) return Yes("爆发末段提拉纳");
        return No("提拉纳资源条件不足");
    }
    public override PAction GetAction() => Gcd(DancerDefinesData.Spells.Tillana);
}

internal sealed class DancerSaberDanceHighGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Qt(QTKey.SaberDance) || !Ready(DancerDefinesData.Spells.SaberDance) || !Buff(DancerDefinesData.Buffs.Devilment))
            return No("高优先级剑舞条件不足");
        if (DancerHelper.Esprit >= 70) return Yes("爆发期高能量剑舞");
        if (DancerBattleData.Instance.DanceOfTheDawnCount == 0 && DancerHelper.Esprit >= 50)
            return Yes("起手第一发剑舞");
        return No("高优先级剑舞条件不足");
    }
    public override PAction GetAction() => AoEGcd(DancerDefinesData.Spells.SaberDance);
}

internal sealed class DancerSaberDanceMediumGcd : DancerResolver
{
    public override CheckResult Check() => Qt(QTKey.SaberDance) && Ready(DancerDefinesData.Spells.SaberDance) && DancerHelper.Esprit >= 50 && (Buff(DancerDefinesData.Buffs.Devilment) || Buff(DancerDefinesData.Buffs.Medicated)) ? Yes("爆发期剑舞") : No("剑舞中优先级条件不足");
    public override PAction GetAction() => AoEGcd(DancerDefinesData.Spells.SaberDance);
}

internal sealed class DancerSaberDanceGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!Qt(QTKey.SaberDance) || !Ready(DancerDefinesData.Spells.SaberDance)) return No("剑舞未就绪");
        if (Qt(QTKey.FinalBurst) || Ready(DancerDefinesData.Spells.Tillana)) return Yes("释放剑舞");
        if (Qt(QTKey.StandardStep) && Cd(DancerDefinesData.Spells.StandardStep) < 3500 && DancerHelper.Esprit >= 80 && !Buff(DancerDefinesData.Buffs.FinishingMoveReady)) return Yes("小舞前防溢出");
        return DancerHelper.Esprit >= DancerSettings.Instance.SaberDanceEspritThreshold ? Yes("达到剑舞阈值") : No("伶俐不足");
    }
    public override PAction GetAction() => AoEGcd(DancerDefinesData.Spells.SaberDance);
}

internal sealed class DancerProcFountainFallHighGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!AnyProcReady() || DancerHelper.Feathers >= 4) return No("坠喷泉高优先级条件不足");
        var flourishFlow = Buff(DancerDefinesData.Buffs.FlourshingFlow);
        var silkenFlow = Buff(DancerDefinesData.Buffs.SilkenFlow);
        var flourishSymmetry = Buff(DancerDefinesData.Buffs.FlourishingSymmetry);
        var standardSoon = Cd(DancerDefinesData.Spells.StandardStep) < 5500 &&
                           !Buff(DancerDefinesData.Buffs.FinishingMoveReady);
        if ((silkenFlow && !BuffFor(DancerDefinesData.Buffs.SilkenFlow, 3500)) ||
            (flourishFlow && !BuffFor(DancerDefinesData.Buffs.FlourshingFlow, 3500)) ||
            (flourishFlow && !BuffFor(DancerDefinesData.Buffs.FlourshingFlow, 8000) && standardSoon) ||
            (silkenFlow && !BuffFor(DancerDefinesData.Buffs.SilkenFlow, 8000) && standardSoon) ||
            (flourishFlow && flourishSymmetry &&
             !BuffFor(DancerDefinesData.Buffs.FlourshingFlow, 6000) &&
             !BuffFor(DancerDefinesData.Buffs.FlourishingSymmetry, 6000)) ||
            (flourishFlow && flourishSymmetry && standardSoon &&
             !BuffFor(DancerDefinesData.Buffs.FlourshingFlow, 11000) &&
             !BuffFor(DancerDefinesData.Buffs.FlourishingSymmetry, 11000)))
            return Yes("坠喷泉即将过期");
        return No("坠喷泉高优先级条件不足");
    }
    public override PAction GetAction() => DancerHelper.CanUseAoe()
        ? AoEGcd(DancerDefinesData.Spells.Bloodshower)
        : Gcd(DancerDefinesData.Spells.Fountainfall);

    private static bool AnyProcReady() =>
        Ready(DancerDefinesData.Spells.ReverseCascade) ||
        Ready(DancerDefinesData.Spells.Fountainfall) ||
        Ready(DancerDefinesData.Spells.RisingWindmill) ||
        Ready(DancerDefinesData.Spells.Bloodshower);
}

internal sealed class DancerProcFountainFallMediumGcd : DancerResolver
{
    public override CheckResult Check() => Ready(DancerDefinesData.Spells.Fountainfall) && DancerHelper.Feathers < 4 && Buff(DancerDefinesData.Buffs.SilkenFlow) && Buff(DancerDefinesData.Buffs.Devilment) && DancerHelper.StatusLeftMs(DancerDefinesData.Buffs.SilkenFlow) < DancerHelper.StatusLeftMs(DancerDefinesData.Buffs.Devilment) ? Yes("坠喷泉团辅内释放") : No("坠喷泉中优先级条件不足");
    public override PAction GetAction() => DancerHelper.CanUseAoe()
        ? AoEGcd(DancerDefinesData.Spells.Bloodshower)
        : Gcd(DancerDefinesData.Spells.Fountainfall);
}

internal sealed class DancerProcReverseCascadeHighGcd : DancerResolver
{
    public override CheckResult Check()
    {
        if (!AnyProcReady() || DancerHelper.Feathers >= 4) return No("逆瀑泻高优先级条件不足");
        var flourishFlow = Buff(DancerDefinesData.Buffs.FlourshingFlow);
        var flourishSymmetry = Buff(DancerDefinesData.Buffs.FlourishingSymmetry);
        var silkenSymmetry = Buff(DancerDefinesData.Buffs.SilkenSymmetry);
        var standardSoon = Cd(DancerDefinesData.Spells.StandardStep) < 5500 &&
                           !Buff(DancerDefinesData.Buffs.FinishingMoveReady);
        if ((flourishSymmetry && !BuffFor(DancerDefinesData.Buffs.FlourishingSymmetry, 3500)) ||
            (silkenSymmetry && !BuffFor(DancerDefinesData.Buffs.SilkenSymmetry, 3500)) ||
            (flourishSymmetry && !BuffFor(DancerDefinesData.Buffs.FlourishingSymmetry, 8000) && standardSoon) ||
            (silkenSymmetry && !BuffFor(DancerDefinesData.Buffs.SilkenSymmetry, 8000) && standardSoon) ||
            (flourishFlow && flourishSymmetry &&
             !BuffFor(DancerDefinesData.Buffs.FlourshingFlow, 6000) &&
             !BuffFor(DancerDefinesData.Buffs.FlourishingSymmetry, 6000)) ||
            (flourishFlow && flourishSymmetry && standardSoon &&
             !BuffFor(DancerDefinesData.Buffs.FlourshingFlow, 11000) &&
             !BuffFor(DancerDefinesData.Buffs.FlourishingSymmetry, 11000)))
            return Yes("逆瀑泻即将过期");
        return No("逆瀑泻高优先级条件不足");
    }
    public override PAction GetAction() => DancerHelper.CanUseAoe()
        ? AoEGcd(DancerDefinesData.Spells.RisingWindmill)
        : Gcd(DancerDefinesData.Spells.ReverseCascade);

    private static bool AnyProcReady() =>
        Ready(DancerDefinesData.Spells.ReverseCascade) ||
        Ready(DancerDefinesData.Spells.Fountainfall) ||
        Ready(DancerDefinesData.Spells.RisingWindmill) ||
        Ready(DancerDefinesData.Spells.Bloodshower);
}

internal sealed class DancerProcReverseCascadeMediumGcd : DancerResolver
{
    public override CheckResult Check() => Ready(DancerDefinesData.Spells.ReverseCascade) && DancerHelper.Feathers < 4 && Buff(DancerDefinesData.Buffs.SilkenSymmetry) && Buff(DancerDefinesData.Buffs.Devilment) && DancerHelper.StatusLeftMs(DancerDefinesData.Buffs.SilkenSymmetry) < DancerHelper.StatusLeftMs(DancerDefinesData.Buffs.Devilment) ? Yes("逆瀑泻团辅内释放") : No("逆瀑泻中优先级条件不足");
    public override PAction GetAction() => DancerHelper.CanUseAoe()
        ? AoEGcd(DancerDefinesData.Spells.RisingWindmill)
        : Gcd(DancerDefinesData.Spells.ReverseCascade);
}
