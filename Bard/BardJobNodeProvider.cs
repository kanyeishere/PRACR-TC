using System;
using System.Collections.Generic;
using PromeRotation.Timeline.Core;
using WotouTC.Bard.Timeline.Conditions;
using WotouTC.Bard.Timeline.Actions;

namespace WotouTC.Bard;

public sealed class BardJobNodeProvider : IJobNodeProvider
{
    public void RegisterNodes(RotationNodeContext context)
    {
        // Conditions
        BardSongTypeCondition.Register(context);
        BardRepertoireCondition.Register(context);
        BardSongTimerCondition.Register(context);
        BardSoulVoiceCondition.Register(context);
        TargetInRangeCondition.Register(context);
        TargetNotInRangeCondition.Register(context);

        // Actions
        BardDotBlacklistAction.Register(context);
        BardHeartBreakSaveAction.Register(context);
        BardPitchPerfectMinCountAction.Register(context);
        BardSongDurationAction.Register(context);
        BardSongOrderAction.Register(context);
        BardPotionModeAction.Register(context);
        BardToggleDailyModeAction.Register(context);
    }

    public IReadOnlyList<(string DisplayName, string Description, Func<ICondition> Create)> GetConditionDescriptors()
        => new (string, string, Func<ICondition>)[]
        {
            ("Bard/判断歌曲类型", "当前歌曲类型与所选相同时条件成立",
                (Func<ICondition>)(() => new BardSongTypeCondition())),
            ("Bard/判断诗心层数", "诗心层数满足比较条件时成立",
                (Func<ICondition>)(() => new BardRepertoireCondition())),
            ("Bard/判断歌曲剩余时间", "歌曲剩余时间满足比较条件时成立",
                (Func<ICondition>)(() => new BardSongTimerCondition())),
            ("Bard/判断灵魂之声", "灵魂之声量谱满足比较条件时成立",
                (Func<ICondition>)(() => new BardSoulVoiceCondition())),
            ("通用/判断目标是否在范围内", "指定DataID的复数目标中有一个在设定范围内",
                (Func<ICondition>)(() => new TargetInRangeCondition())),
            ("通用/判断所有目标不在范围内", "指定DataID的所有目标都不在设定范围内",
                (Func<ICondition>)(() => new TargetNotInRangeCondition())),
        };

    public IReadOnlyList<(string DisplayName, string Description, Func<IAction> Create)> GetActionDescriptors()
        => new (string, string, Func<IAction>)[]
        {
            ("Bard/DoT黑名单管理", "添加/移除/清空 DoT 黑名单",
                (Func<IAction>)(() => new BardDotBlacklistAction())),
            ("Bard/碎心箭保留层数", "设置碎心箭保留层数",
                (Func<IAction>)(() => new BardHeartBreakSaveAction())),
            ("Bard/完美音调最少敌人数", "设置使用完美音调所需的最少敌人数",
                (Func<IAction>)(() => new BardPitchPerfectMinCountAction())),
            ("Bard/歌曲时长", "设置旅神/贤者/军神每首歌的持续时长",
                (Func<IAction>)(() => new BardSongDurationAction())),
            ("Bard/歌曲顺序", "设置歌曲顺序（非标准序将禁用对齐旅神）",
                (Func<IAction>)(() => new BardSongOrderAction())),
            ("Bard/爆发药模式", "切换起手吃爆发药或两分钟爆发吃",
                (Func<IAction>)(() => new BardPotionModeAction())),
            ("Bard/切换模式（日随高难）", "切换日随模式或高难模式",
                (Func<IAction>)(() => new BardToggleDailyModeAction())),
        };
}
