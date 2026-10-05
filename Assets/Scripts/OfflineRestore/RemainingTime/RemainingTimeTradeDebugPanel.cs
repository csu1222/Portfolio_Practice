using UnityEngine;
using System;

public class RemainingTimeTradeDebugPanel : MonoBehaviour
{
    [SerializeField] private RemainingTimeTradeManager manager;
    [SerializeField] private RemainingTimeSaveService saveService;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindAnyObjectByType<RemainingTimeTradeManager>();
        }

        if (saveService == null)
        {
            saveService = FindAnyObjectByType<RemainingTimeSaveService>();
        }
    }

    public void TradeStartButton()
    {
        manager.TradeStart();
    }

    public void SaveButton()
    {
        saveService.Save(manager.CreateSnapshot());
    }

    public void LoadButton()
    {
        RemainingTimeSaveData snapshot = saveService.Load();
        if (snapshot != null)
        {
            manager.ApplySnapshot(snapshot);
        }
        PrintDebugLog();
    }

    public void RestoreButton()
    {
        // Happy path

        RemainingTimeSaveData snapshot = saveService.Load();

        if (snapshot != null)
        {
            if (manager.ApplySnapshot(snapshot))
            {
                manager.Restore(DateTime.UtcNow);
            }
        }



        // 2. Offline Complete 
        // Restore ÈÄ SaveRemainingTime < OfflineElapsed
        //{
        //    DateTime currentUtc = DateTime.UtcNow;

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 5f,
        //        lastAppliedUtcTicks = currentUtc.AddSeconds(-10).Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (snapshot != null)
        //    {
        //        if (manager.ApplySnapshot(snapshot))
        //        {

        //            bool result = manager.Restore(currentUtc);

        //            Debug.Log(
        //                $"[Offline Complete Test] " +
        //                $"Restore={result}, " +
        //                $"Remaining={manager.RemainingTime}, " +
        //                $"State={manager.CurrentState}, " +
        //                $"LastAppliedUtc={manager.LastAppliedUtc:O}");
        //        }
        //    }
        //}


        // 3. Clock Rollback
        //{
        //    DateTime currentUtc = DateTime.UtcNow;

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 5f,
        //        lastAppliedUtcTicks = currentUtc.AddSeconds(10).Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    DateTime snapshotUtc =
        //        new DateTime(snapshot.lastAppliedUtcTicks, DateTimeKind.Utc);

        //    Debug.Log(
        //        $"[Clock Rollback BEFORE APPLY] " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"SnapshotUtc={snapshotUtc:O}, " +
        //        $"Snapshot > Current={snapshotUtc > currentUtc}");

        //    bool applyResult = manager.ApplySnapshot(snapshot);

        //    Debug.Log(
        //        $"[Clock Rollback AFTER APPLY] " +
        //        $"Apply={applyResult}, " +
        //        $"Manager.LastAppliedUtc={manager.LastAppliedUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"Manager.LastAppliedUtc > Current={manager.LastAppliedUtc > currentUtc}");

        //    if (applyResult)
        //    {
        //        bool restoreResult = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Clock Rollback AFTER RESTORE] " +
        //            $"Restore={restoreResult}, " +
        //            $"Remaining={manager.RemainingTime}, " +
        //            $"State={manager.CurrentState}, " +
        //            $"LastAppliedUtc={manager.LastAppliedUtc:O}");
        //    }
        //}

        // 4. Duplicate Restore

        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 23, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 30f,
        //        lastAppliedUtcTicks = snapshotUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool firstResult = manager.Restore(currentUtc);
        //        if ( firstResult)
        //        {
        //            float firstRemainingTime = manager.RemainingTime;
        //            DateTime firstLastAppliedUtc = manager.LastAppliedUtc;
        //            RemainingTimeTradeState firstState = manager.CurrentState;

        //            Debug.Log($"[Debug Log] [Duplicate Restore] First Restore \n" +
        //                $"Remaining Time : {firstRemainingTime}\n" +
        //                $"Last Applied UTC : {firstLastAppliedUtc}\n" +
        //                $"Current State : {firstState}");
        //        }

        //        bool secondResult = manager.Restore(currentUtc);
        //        if (secondResult)
        //        {
        //            float secondRemainingTime = manager.RemainingTime;
        //            DateTime secondLastAppliedUtc = manager.LastAppliedUtc;
        //            RemainingTimeTradeState secondState = manager.CurrentState;

        //            Debug.Log($"[Debug Log] [Duplicate Restore] Second Restore \n" +
        //                $"Remaining Time : {secondRemainingTime}\n" +
        //                $"Last Applied UTC : {secondLastAppliedUtc}\n" +
        //                $"Current State : {secondState}");
        //        }
        //    }
        //}

        // 5. Prepare Restore
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 0f,
        //        lastAppliedUtcTicks = snapshotUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Prepare,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        float beforeRemaining = manager.RemainingTime;
        //        DateTime beforeUtc = manager.LastAppliedUtc;
        //        RemainingTimeTradeState beforeState = manager.CurrentState;

        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Prepare Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {beforeRemaining} -> {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {beforeUtc:O} -> {manager.LastAppliedUtc:O}\n" +
        //            $"State: {beforeState} -> {manager.CurrentState}");
        //    }
        //}

        // 6. Complete Restore
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 0f,
        //        lastAppliedUtcTicks = snapshotUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Completed,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        float beforeRemaining = manager.RemainingTime;
        //        DateTime beforeUtc = manager.LastAppliedUtc;
        //        RemainingTimeTradeState beforeState = manager.CurrentState;

        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {beforeRemaining} -> {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {beforeUtc:O} -> {manager.LastAppliedUtc:O}\n" +
        //            $"State: {beforeState} -> {manager.CurrentState}");
        //    }
        //}

        // 7. Invalid Data ApplySnapshot
        // 1) null
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = null;

        //    bool restoreResult = manager.ApplySnapshot(snapshot);

        //if (restoreResult)
        //{
        //    bool result = manager.Restore(currentUtc);

        //    Debug.Log(
        //        $"[Null Snapshot Test]\n" +
        //        $"Restore={result}\n" +
        //        $"Remaining:{manager.RemainingTime}\n" +
        //        $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //        $"State: {manager.CurrentState}");
        //}
        //    else
        //    {
        //        Debug.Log("[Null Snapshot Test] Null Erorr");
        //    }
        //}

        // 2) remainingTime NaN
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = float.NaN,
        //        lastAppliedUtcTicks = snapshotUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Nan RemainingTime Test] RemainingTime is Nan");
        //    }
        //}

        //// 3) remainingTime Infinity
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = float.PositiveInfinity,
        //        lastAppliedUtcTicks = snapshotUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Positive Infinity RemainingTime Test] Positive Infinity RemainingTime");
        //    }
        //}
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = float.NegativeInfinity,
        //        lastAppliedUtcTicks = snapshotUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Negative Infinity RemainingTime Test] Negative Infinity RemainingTime");
        //    }
        //}

        // 4) remainingTime < 0
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = -1f,
        //        lastAppliedUtcTicks = snapshotUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Negative RemainingTime Test] Negative RemainingTime");
        //    }
        //}

        // 5) remainingTime > duration
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = manager.Duration + 1f,
        //        lastAppliedUtcTicks = snapshotUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[remainingTime > duration Test] remainingTime > duration");
        //    }
        //}

        // 6) lastAppliedUtcTicks < DateTime.MinValue.Ticks
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    long moreMinTicks = DateTime.MinValue.Ticks - 1;

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 5f,
        //        lastAppliedUtcTicks = moreMinTicks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[lastAppliedUtcTicks < DateTime.MinValue.Ticks Test] lastAppliedUtcTicks < DateTime.MinValue.Ticks");
        //    }
        //}

        // 7) lastAppliedUtcTicks > DateTime.MaxValue.Ticks
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    long moreMaxTicks = DateTime.MaxValue.Ticks + 1;

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 5f,
        //        lastAppliedUtcTicks = moreMaxTicks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[lastAppliedUtcTicks > DateTime.MaxValue.Ticks Test] lastAppliedUtcTicks > DateTime.MaxValue.Ticks");
        //    }
        //}

        // 8) Undefind currentState
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 5f,
        //        lastAppliedUtcTicks = currentUtc.Ticks,
        //        currentState = 999,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Undefind currentState Test] Undefind currentState");
        //    }
        //}

        // 9) Prepare remainingTime != 0
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 5f,
        //        lastAppliedUtcTicks = currentUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Prepare,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Prepare remainingTime != 0 Test] Prepare remainingTime != 0");
        //    }
        //}

        // 10) Traveling remainingTime <= 0
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 0f,
        //        lastAppliedUtcTicks = currentUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Traveling remainingTime <= 0 Test] Traveling remainingTime <= 0");
        //    }
        //}

        // 11) Traveling remainingTime > duration
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = manager.Duration + 1f,
        //        lastAppliedUtcTicks = currentUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Traveling,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Traveling remainingTime > duration Test] Traveling remainingTime > duration");
        //    }
        //}

        // 12) 1. Completed remainingTime != 0
        //{
        //    DateTime snapshotUtc = new DateTime(2026, 9, 24, 7, 0, 0, DateTimeKind.Utc);
        //    DateTime currentUtc = snapshotUtc.AddSeconds(10);

        //    RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        //    {
        //        remainingTime = 1f,
        //        lastAppliedUtcTicks = currentUtc.Ticks,
        //        currentState = (int)RemainingTimeTradeState.Completed,
        //    };

        //    if (manager.ApplySnapshot(snapshot))
        //    {
        //        bool result = manager.Restore(currentUtc);

        //        Debug.Log(
        //            $"[Complete Restore Test]\n" +
        //            $"Restore={result}\n" +
        //            $"Remaining: {manager.RemainingTime}\n" +
        //            $"LastAppliedUtc: {manager.LastAppliedUtc:O}\n" +
        //            $"State: {manager.CurrentState}");
        //    }
        //    else
        //    {
        //        Debug.Log("[Completed remainingTime != 0 Test] Completed remainingTime != 0");
        //    }
        //}
    }

    private void PrintDebugLog()
    {
        Debug.Log($"[Debug Log] Remaining Time : {manager.RemainingTime}");
        Debug.Log($"[Debug Log] Last Applied UTC : {manager.LastAppliedUtc}");
        Debug.Log($"[Debug Log] Current State : {manager.CurrentState}");
    }
}
