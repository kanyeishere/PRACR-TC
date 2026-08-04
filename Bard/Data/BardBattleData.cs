using Dalamud.Game.ClientState.JobGauge.Enums;

namespace WotouTC.Bard.Data;

public class BardBattleData
{
    private const int MaxDebugEvents = 160;
    private static readonly uint[] Default120SBuffOrder =
        [BRDSkill.RagingStrikes, BRDSkill.BattleVoice, BRDSkill.RadiantFinale];

    public static BardBattleData Instance { get; } = new();

    private readonly object _debugLock = new();
    private readonly object _actionLock = new();
    private readonly Queue<BardBattleDebugEvent> _debugEvents = new();
    private readonly HashSet<uint> _recorded120SBuffActions = new();
    private long _lastRagingStrikesResetTime;
    private uint _lastRagingStrikesGlobalSequence;
    private bool _hasUseIronJawsInCurrentBursting;
    private bool _hasUseApexArrowInCurrentNonBurstingPeriod;
    private long _actionEffectsReceived;
    private long _actionEffectsSourceFiltered;
    private long _actionEffectsNoPlayer;
    private long _actionEffectsAccepted;
    private uint _lastObservedActionId;
    private ulong _lastObservedActionSourceId;
    private uint _lastObservedActionSequence;
    private ulong _lastObservedPlayerEntityId;

    public HashSet<uint> DotBlackList { get; } = new();

    public bool HasUseIronJawsInCurrentBursting
    {
        get => _hasUseIronJawsInCurrentBursting;
        private set
        {
            if (_hasUseIronJawsInCurrentBursting == value)
                return;

            _hasUseIronJawsInCurrentBursting = value;
            AddDebugEvent($"Iron Jaws 爆发标记 = {value}");
        }
    }

    public bool HasUseApexArrowInCurrentNonBurstingPeriod
    {
        get => _hasUseApexArrowInCurrentNonBurstingPeriod;
        set
        {
            if (_hasUseApexArrowInCurrentNonBurstingPeriod == value)
                return;

            _hasUseApexArrowInCurrentNonBurstingPeriod = value;
            AddDebugEvent($"Apex 非爆发标记 = {value}");
        }
    }

    public int PitchPerfectMinEnemyCount { get; set; }

    public double TotalStopTime { get; set; }

    public double RestStopTime { get; set; }

    public int GcdCountDown { get; set; } = 3;

    public long BaseGcdHoldStartTime { get; set; }

    public double CurrentBaseGcdHoldTime { get; set; }

    public bool HasFirst120SBuff { get; set; }

    public bool HasSecond120SBuff { get; set; }

    public bool HasThird120SBuff { get; set; }

    public uint First120SBuffSpellId { get; set; } = BRDSkill.RagingStrikes;

    public uint First120SBuffId { get; set; } = BRDBuff.RagingStrikes;

    public uint Second120SBuffSpellId { get; set; } = BRDSkill.BattleVoice;

    public uint Second120SBuffId { get; set; } = BRDBuff.BattleVoice;

    public uint Third120SBuffSpellId { get; set; } = BRDSkill.RadiantFinale;

    public uint Third120SBuffId { get; set; } = BRDBuff.RadiantFinale;

    public Song LastSong { get; set; } = Song.None;

    public long LastSongTime { get; set; }

    public Dictionary<uint, long> LastActionTimes { get; } = new();

    public long LastRagingStrikesActionTime => _lastRagingStrikesResetTime;

    public long ActionEffectsReceived => _actionEffectsReceived;

    public long ActionEffectsSourceFiltered => _actionEffectsSourceFiltered;

    public long ActionEffectsNoPlayer => _actionEffectsNoPlayer;

    public long ActionEffectsAccepted => _actionEffectsAccepted;

    public uint LastObservedActionId => _lastObservedActionId;

    public ulong LastObservedActionSourceId => _lastObservedActionSourceId;

    public uint LastObservedActionSequence => _lastObservedActionSequence;

    public ulong LastObservedPlayerEntityId => _lastObservedPlayerEntityId;

    public IReadOnlyList<BardBattleDebugEvent> GetDebugEventsSnapshot()
    {
        lock (_debugLock)
            return _debugEvents.ToArray();
    }

    public IReadOnlyList<KeyValuePair<uint, long>> GetLastActionTimesSnapshot()
    {
        lock (_actionLock)
            return LastActionTimes.ToArray();
    }

    public void ClearDebugEvents()
    {
        lock (_debugLock)
            _debugEvents.Clear();
    }

    public void ResetForBattle()
    {
        lock (_debugLock)
            _debugEvents.Clear();

        _recorded120SBuffActions.Clear();
        _lastRagingStrikesResetTime = 0;
        _lastRagingStrikesGlobalSequence = 0;
        _actionEffectsReceived = 0;
        _actionEffectsSourceFiltered = 0;
        _actionEffectsNoPlayer = 0;
        _actionEffectsAccepted = 0;
        _lastObservedActionId = 0;
        _lastObservedActionSourceId = 0;
        _lastObservedActionSequence = 0;
        _lastObservedPlayerEntityId = 0;
        HasUseIronJawsInCurrentBursting = false;
        HasUseApexArrowInCurrentNonBurstingPeriod = false;
        TotalStopTime = 0;
        RestStopTime = 0;
        GcdCountDown = 3;
        BaseGcdHoldStartTime = 0;
        CurrentBaseGcdHoldTime = 0;
        HasFirst120SBuff = false;
        HasSecond120SBuff = false;
        HasThird120SBuff = false;
        First120SBuffSpellId = BRDSkill.RagingStrikes;
        First120SBuffId = BRDBuff.RagingStrikes;
        Second120SBuffSpellId = BRDSkill.BattleVoice;
        Second120SBuffId = BRDBuff.BattleVoice;
        Third120SBuffSpellId = BRDSkill.RadiantFinale;
        Third120SBuffId = BRDBuff.RadiantFinale;
        LastSong = Song.None;
        LastSongTime = 0;
        lock (_actionLock)
            LastActionTimes.Clear();
        AddDebugEvent("战斗状态重置");
    }

    public bool ObserveActionEffect(uint actionId, ulong sourceId, uint globalSequence, ulong? playerEntityId)
    {
        _actionEffectsReceived++;
        _lastObservedActionId = actionId;
        _lastObservedActionSourceId = sourceId;
        _lastObservedActionSequence = globalSequence;
        _lastObservedPlayerEntityId = playerEntityId ?? 0;

        if (playerEntityId is null)
        {
            _actionEffectsNoPlayer++;
            return false;
        }

        if (sourceId != playerEntityId.Value)
        {
            _actionEffectsSourceFiltered++;
            return false;
        }

        _actionEffectsAccepted++;
        return true;
    }

    public void RecordAction(uint actionId, uint globalSequence = 0)
    {
        var now = Environment.TickCount64;
        lock (_actionLock)
            LastActionTimes[actionId] = now;
        AddDebugEvent($"ActionEffect: {GetActionName(actionId)} ({actionId}) Seq:{globalSequence}");
    }

    public void RecordGcdAction(uint actionId)
    {
        if (!IsBardGcd(actionId))
            return;

        if (GcdCountDown > 0)
            GcdCountDown--;

        BaseGcdHoldStartTime = 0;
        CurrentBaseGcdHoldTime = 0;
        AddDebugEvent($"GCD 记录: {GetActionName(actionId)}，倒计时={GcdCountDown}");
    }

    public void Record120SBuffAction(uint actionId, uint globalSequence = 0)
    {
        if (actionId is not (BRDSkill.RagingStrikes or BRDSkill.BattleVoice or BRDSkill.RadiantFinale))
            return;

        if (_recorded120SBuffActions.Contains(actionId))
        {
            AddDebugEvent($"120秒技能重复事件忽略: {GetActionName(actionId)} Seq:{globalSequence}");
            return;
        }

        if (!HasFirst120SBuff)
        {
            HasFirst120SBuff = true;
            First120SBuffSpellId = actionId;
            First120SBuffId = GetBuffIdBy120SBuffAction(actionId);
            _recorded120SBuffActions.Add(actionId);
            FillPending120SBuffSlots();
            AddDebugEvent($"120秒顺序[1] = {GetActionName(actionId)} Seq:{globalSequence}");
            return;
        }

        if (!HasSecond120SBuff)
        {
            HasSecond120SBuff = true;
            Second120SBuffSpellId = actionId;
            Second120SBuffId = GetBuffIdBy120SBuffAction(actionId);
            _recorded120SBuffActions.Add(actionId);
            FillPending120SBuffSlots();
            AddDebugEvent($"120秒顺序[2] = {GetActionName(actionId)} Seq:{globalSequence}");
            return;
        }

        if (!HasThird120SBuff)
        {
            HasThird120SBuff = true;
            Third120SBuffSpellId = actionId;
            Third120SBuffId = GetBuffIdBy120SBuffAction(actionId);
            _recorded120SBuffActions.Add(actionId);
            AddDebugEvent($"120秒顺序[3] = {GetActionName(actionId)} Seq:{globalSequence}");
            return;
        }

        AddDebugEvent($"120秒顺序已满，忽略: {GetActionName(actionId)}");
    }

    public void ResetIronJawsBurstUsage(uint globalSequence = 0)
    {
        var now = Environment.TickCount64;
        if (globalSequence != 0 && globalSequence == _lastRagingStrikesGlobalSequence)
        {
            AddDebugEvent($"重复猛者事件忽略，序列号相同: {globalSequence}");
            return;
        }

        if (_lastRagingStrikesResetTime != 0 && now - _lastRagingStrikesResetTime < 5000)
        {
            AddDebugEvent("重复猛者事件忽略，未重置 Iron Jaws 爆发标记");
            return;
        }

        _lastRagingStrikesResetTime = now;
        _lastRagingStrikesGlobalSequence = globalSequence;
        HasUseIronJawsInCurrentBursting = false;
        AddDebugEvent($"猛者开启新的爆发窗口 Seq:{globalSequence}");
    }

    public void RecordSong(Song song)
    {
        if (song == Song.None)
            return;

        LastSong = song;
        LastSongTime = Environment.TickCount64;
        AddDebugEvent($"歌曲 = {song}");
    }

    public void RecordIronJawsBurstUsage(bool isBurstWindow)
    {
        if (!isBurstWindow)
        {
            AddDebugEvent("Iron Jaws 命中，但不在猛者爆发窗口");
            return;
        }

        if (HasUseIronJawsInCurrentBursting)
        {
            AddDebugEvent("Iron Jaws 爆发标记已存在，重复事件忽略");
            return;
        }

        HasUseIronJawsInCurrentBursting = true;
        AddDebugEvent("Iron Jaws 计入当前猛者爆发");
    }

    public bool IsWithinIronJawsBurstWindow(int windowMilliseconds = 20000)
    {
        return _lastRagingStrikesResetTime != 0 &&
               Environment.TickCount64 - _lastRagingStrikesResetTime <= windowMilliseconds;
    }

    public long GetLastActionTime(uint actionId)
    {
        lock (_actionLock)
            return LastActionTimes.TryGetValue(actionId, out var time) ? time : 0;
    }

    private static bool IsBardGcd(uint actionId)
    {
        return actionId is BRDSkill.HeavyShot
            or BRDSkill.StraightShot
            or BRDSkill.BurstShot
            or BRDSkill.RefulgentArrow
            or BRDSkill.WideVolley
            or BRDSkill.QuickNock
            or BRDSkill.Ladonsbite
            or BRDSkill.Shadowbite
            or BRDSkill.ApexArrow
            or BRDSkill.BlastArrow
            or BRDSkill.ResonantArrow
            or BRDSkill.RadiantEncore
            or BRDSkill.VenomousBite
            or BRDSkill.CausticBite
            or BRDSkill.Windbite
            or BRDSkill.Stormbite
            or BRDSkill.IronJaws;
    }

    private static uint GetBuffIdBy120SBuffAction(uint actionId)
    {
        return actionId switch
        {
            BRDSkill.RagingStrikes => BRDBuff.RagingStrikes,
            BRDSkill.BattleVoice => BRDBuff.BattleVoice,
            BRDSkill.RadiantFinale => BRDBuff.RadiantFinale,
            _ => 0
        };
    }

    private void FillPending120SBuffSlots()
    {
        var remaining = Default120SBuffOrder
            .Where(actionId => !_recorded120SBuffActions.Contains(actionId))
            .ToArray();
        var next = 0;

        if (!HasFirst120SBuff && next < remaining.Length)
        {
            First120SBuffSpellId = remaining[next++];
            First120SBuffId = GetBuffIdBy120SBuffAction(First120SBuffSpellId);
        }

        if (!HasSecond120SBuff && next < remaining.Length)
        {
            Second120SBuffSpellId = remaining[next++];
            Second120SBuffId = GetBuffIdBy120SBuffAction(Second120SBuffSpellId);
        }

        if (!HasThird120SBuff && next < remaining.Length)
        {
            Third120SBuffSpellId = remaining[next];
            Third120SBuffId = GetBuffIdBy120SBuffAction(Third120SBuffSpellId);
        }
    }

    private void AddDebugEvent(string message)
    {
        var entry = new BardBattleDebugEvent(Environment.TickCount64, message);
        lock (_debugLock)
        {
            _debugEvents.Enqueue(entry);
            while (_debugEvents.Count > MaxDebugEvents)
                _debugEvents.Dequeue();
        }
    }

    public static string GetActionName(uint actionId)
    {
        return actionId switch
        {
            BRDSkill.RagingStrikes => "猛者强击",
            BRDSkill.BattleVoice => "战斗之声",
            BRDSkill.RadiantFinale => "光明神",
            BRDSkill.IronJaws => "铁颚箭",
            BRDSkill.ApexArrow => "绝峰箭",
            _ => "技能"
        };
    }
}

public readonly record struct BardBattleDebugEvent(long Timestamp, string Message);
