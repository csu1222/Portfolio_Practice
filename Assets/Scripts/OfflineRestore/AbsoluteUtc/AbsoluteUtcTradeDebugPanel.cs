using System;
using UnityEngine;

public class AbsoluteUtcTradeDebugPanel : MonoBehaviour
{
    [SerializeField] private AbsoluteUtcTradeManager manager;
    [SerializeField] private AbsoluteUtcSaveService saveService;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindAnyObjectByType<AbsoluteUtcTradeManager>();
        }

        if (saveService == null)
        {
            saveService = FindAnyObjectByType<AbsoluteUtcSaveService>();
        }
    }

    public void TradeStartButton()
    {
        manager.TradeStart();
    }

    public void SaveButton()
    {
        AbsoluteUtcSaveData snapshot = manager.CreateSnapshot();
        saveService.Save(snapshot);
    }

    public void LoadButton()
    {
        AbsoluteUtcSaveData snapshot = saveService.Load();
        if (snapshot != null)
        {
            manager.ApplySnapshot(snapshot);
        }
        PrintDebugLog();
    }

    public void RestoreButton()
    {
        // 1. Happy Path — Traveling 유지
        AbsoluteUtcSaveData snapshot = saveService.Load();
        DateTime currentUtc = DateTime.UtcNow;
        if (snapshot != null)
        {
            if (manager.ApplySnapshot(snapshot))
                manager.Restore(currentUtc);
        }

        // 2.Offline Complete
        //{
        //    DateTime startUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime expectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState state = AbsoluteUtcTradeState.Traveling;
        //    DateTime currentUtc = new DateTime(2026, 10, 22, 12, 1, 10, DateTimeKind.Utc);

        //    AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = startUtc.Ticks,
        //        expectedEndUtcTicks = expectedEndUtc.Ticks,
        //        currentState = (int)state,
        //    };


        //    if (manager.ApplySnapshot(snapshot))
        //    {

        //        bool result = manager.Restore(currentUtc);
        //        Debug.Log(
        //        $"[Offline Complete Test] " +
        //        $"Restore={result}, " +
        //        $"StartUtc={manager.StartUtc:O}, " +
        //        $"ExpectedEndUtc={manager.ExpectedEndUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"State={manager.CurrentState}, " +
        //        $"Remaining={manager.CalculateRemainingTime(currentUtc)}, " +
        //        $"Progress={manager.CalculateProgressRatio(currentUtc)}");
        //    }

        //}

        // 3.Clock Rollback — CurrentUtc < StartUtc
        //{
        //    DateTime startUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime expectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState state = AbsoluteUtcTradeState.Traveling;
        //    DateTime currentUtc = new DateTime(2026, 10, 22, 11, 59, 0, DateTimeKind.Utc);

        //    AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = startUtc.Ticks,
        //        expectedEndUtcTicks = expectedEndUtc.Ticks,
        //        currentState = (int)state,
        //    };


        //    if (manager.ApplySnapshot(snapshot))
        //    {

        //        bool result = manager.Restore(currentUtc);
        //        Debug.Log(
        //        $"[Clock Rollback Test][CurrentUtc < StartUtc]" +
        //        $"Restore={result}, " +
        //        $"StartUtc={manager.StartUtc:O}, " +
        //        $"ExpectedEndUtc={manager.ExpectedEndUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"State={manager.CurrentState}, " +
        //        $"Remaining={manager.CalculateRemainingTime(currentUtc)}, " +
        //        $"Progress={manager.CalculateProgressRatio(currentUtc)}");
        //    }
        //}

        // 4.CurrentUtc == StartUtc 경계
        //{
        //    DateTime startUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime expectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState state = AbsoluteUtcTradeState.Traveling;
        //    DateTime currentUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);

        //    AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = startUtc.Ticks,
        //        expectedEndUtcTicks = expectedEndUtc.Ticks,
        //        currentState = (int)state,
        //    };


        //    if (manager.ApplySnapshot(snapshot))
        //    {

        //        bool result = manager.Restore(currentUtc);
        //        Debug.Log(
        //        $"[CurrentUtc == StartUtc Test]" +
        //        $"Restore={result}, " +
        //        $"StartUtc={manager.StartUtc:O}, " +
        //        $"ExpectedEndUtc={manager.ExpectedEndUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"State={manager.CurrentState}, " +
        //        $"Remaining={manager.CalculateRemainingTime(currentUtc)}, " +
        //        $"Progress={manager.CalculateProgressRatio(currentUtc)}");
        //    }
        //}

        // 5.CurrentUtc == ExpectedEndUtc 경계
        //{
        //    DateTime startUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime expectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState state = AbsoluteUtcTradeState.Traveling;
        //    DateTime currentUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);

        //    AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = startUtc.Ticks,
        //        expectedEndUtcTicks = expectedEndUtc.Ticks,
        //        currentState = (int)state,
        //    };


        //    if (manager.ApplySnapshot(snapshot))
        //    {

        //        bool result = manager.Restore(currentUtc);
        //        Debug.Log(
        //        $"[CurrentUtc == ExpectedEndUtc Test]" +
        //        $"Restore={result}, " +
        //        $"StartUtc={manager.StartUtc:O}, " +
        //        $"ExpectedEndUtc={manager.ExpectedEndUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"State={manager.CurrentState}, " +
        //        $"Remaining={manager.CalculateRemainingTime(currentUtc)}, " +
        //        $"Progress={manager.CalculateProgressRatio(currentUtc)}");
        //    }
        //}

        // 6.Duplicate Restore
        //{
        //    DateTime startUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime expectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState state = AbsoluteUtcTradeState.Traveling;
        //    DateTime currentUtc = new DateTime(2026, 10, 22, 12, 0, 30, DateTimeKind.Utc);

        //    AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = startUtc.Ticks,
        //        expectedEndUtcTicks = expectedEndUtc.Ticks,
        //        currentState = (int)state,
        //    };


        //    if (manager.ApplySnapshot(snapshot))
        //    {

        //        bool resultFirst = manager.Restore(currentUtc);
        //        Debug.Log(
        //        $"[Duplicate Restore Test][First]" +
        //        $"Restore={resultFirst}, " +
        //        $"StartUtc={manager.StartUtc:O}, " +
        //        $"ExpectedEndUtc={manager.ExpectedEndUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"State={manager.CurrentState}, " +
        //        $"Remaining={manager.CalculateRemainingTime(currentUtc)}, " +
        //        $"Progress={manager.CalculateProgressRatio(currentUtc)}");

        //        bool resultSecond = manager.Restore(currentUtc);
        //        Debug.Log(
        //        $"[Duplicate Restore Test][Second]" +
        //        $"Restore={resultSecond}, " +
        //        $"StartUtc={manager.StartUtc:O}, " +
        //        $"ExpectedEndUtc={manager.ExpectedEndUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"State={manager.CurrentState}, " +
        //        $"Remaining={manager.CalculateRemainingTime(currentUtc)}, " +
        //        $"Progress={manager.CalculateProgressRatio(currentUtc)}");
        //    }
        //}

        // 7.Prepare Restore
        //{
        //    DateTime startUtc = DateTime.MinValue;
        //    DateTime expectedEndUtc = DateTime.MinValue;
        //    AbsoluteUtcTradeState state = AbsoluteUtcTradeState.Prepare;
        //    DateTime currentUtc = new DateTime(2026, 10, 22, 12, 0, 30, DateTimeKind.Utc);

        //    AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = startUtc.Ticks,
        //        expectedEndUtcTicks = expectedEndUtc.Ticks,
        //        currentState = (int)state,
        //    };


        //    if (manager.ApplySnapshot(snapshot))
        //    {

        //        bool result = manager.Restore(currentUtc);
        //        Debug.Log(
        //        $"[Prepare Restore Test]" +
        //        $"Restore={result}, " +
        //        $"StartUtc={manager.StartUtc:O}, " +
        //        $"ExpectedEndUtc={manager.ExpectedEndUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"State={manager.CurrentState}, " +
        //        $"Remaining={manager.CalculateRemainingTime(currentUtc)}, " +
        //        $"Progress={manager.CalculateProgressRatio(currentUtc)}");

        //    }
        //}

        // 8.Completed Restore
        //{
        //    DateTime startUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime expectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState state = AbsoluteUtcTradeState.Completed;
        //    DateTime currentUtc = new DateTime(2026, 10, 22, 12, 1, 30, DateTimeKind.Utc);

        //    AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = startUtc.Ticks,
        //        expectedEndUtcTicks = expectedEndUtc.Ticks,
        //        currentState = (int)state,
        //    };


        //    if (manager.ApplySnapshot(snapshot))
        //    {

        //        bool result = manager.Restore(currentUtc);
        //        Debug.Log(
        //        $"[Completed Restore Test]" +
        //        $"Restore={result}, " +
        //        $"StartUtc={manager.StartUtc:O}, " +
        //        $"ExpectedEndUtc={manager.ExpectedEndUtc:O}, " +
        //        $"CurrentUtc={currentUtc:O}, " +
        //        $"State={manager.CurrentState}, " +
        //        $"Remaining={manager.CalculateRemainingTime(currentUtc)}, " +
        //        $"Progress={manager.CalculateProgressRatio(currentUtc)}");

        //    }
        //}

        // ---------------------------- null → StartUtc Tick 범위 → ExpectedEndUtc Tick 범위 → Undefined State → Prepare/Traveling/Completed
        // Restore Test
        // 1. Null Snapshot
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;

        //    AbsoluteUtcSaveData InvalidSnapshot = null;

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][Null Snapshot]\n" +
        //        $"ValidApplyResult = {validApplyResult}" + 
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

        // 2. startUtcTicks < DateTime.MinValue.Ticks
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;


        //    DateTime invalidStartUtc = DateTime.MinValue;
        //    DateTime invalidExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState invalidState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData InvalidSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = invalidStartUtc.Ticks - 1,
        //        expectedEndUtcTicks = invalidExpectedEndUtc.Ticks,
        //        currentState = (int)invalidState,
        //    };

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][startUtcTicks < DateTime.MinValue.Ticks]\n" +
        //        $"ValidApplyResult = {validApplyResult}\n" +
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

        // 3. startUtcTicks > DateTime.MaxValue.Ticks
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;


        //    DateTime invalidStartUtc = DateTime.MaxValue;
        //    DateTime invalidExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState invalidState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData InvalidSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = invalidStartUtc.Ticks + 1,
        //        expectedEndUtcTicks = invalidExpectedEndUtc.Ticks,
        //        currentState = (int)invalidState,
        //    };

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][startUtcTicks > DateTime.MaxValue.Ticks]\n" +
        //        $"ValidApplyResult = {validApplyResult}\n" +
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

        // 4. expectedEndUtcTicks < DateTime.MinValue.Ticks
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;


        //    DateTime invalidStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime invalidExpectedEndUtc = DateTime.MinValue;
        //    AbsoluteUtcTradeState invalidState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData InvalidSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = invalidStartUtc.Ticks,
        //        expectedEndUtcTicks = invalidExpectedEndUtc.Ticks - 1,
        //        currentState = (int)invalidState,
        //    };

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][expectedEndUtcTicks < DateTime.MinValue.Ticks]\n" +
        //        $"ValidApplyResult = {validApplyResult}\n" +
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

        // 5. expectedEndUtcTicks > DateTime.MaxValue.Ticks
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;


        //    DateTime invalidStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime invalidExpectedEndUtc = DateTime.MaxValue;
        //    AbsoluteUtcTradeState invalidState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData InvalidSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = invalidStartUtc.Ticks,
        //        expectedEndUtcTicks = invalidExpectedEndUtc.Ticks + 1,
        //        currentState = (int)invalidState,
        //    };

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][expectedEndUtcTicks > DateTime.MaxValue.Ticks]\n" +
        //        $"ValidApplyResult = {validApplyResult}\n" +
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

        // 6. Undefined CurrentState
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;


        //    DateTime invalidStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime invalidExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    //AbsoluteUtcTradeState invalidState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData InvalidSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = invalidStartUtc.Ticks,
        //        expectedEndUtcTicks = invalidExpectedEndUtc.Ticks,
        //        currentState = 999,
        //    };

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][Undefined CurrentState]\n" +
        //        $"ValidApplyResult = {validApplyResult}\n" +
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

        // 7. Prepare Invariant
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;


        //    DateTime invalidStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime invalidExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState invalidState = AbsoluteUtcTradeState.Prepare;

        //    AbsoluteUtcSaveData InvalidSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = invalidStartUtc.Ticks,
        //        expectedEndUtcTicks = invalidExpectedEndUtc.Ticks,
        //        currentState = (int)invalidState,
        //    };

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][Prepare Invariant]\n" +
        //        $"ValidApplyResult = {validApplyResult}\n" +
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

        // 8. Treveling Invariant
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;


        //    DateTime invalidStartUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    DateTime invalidExpectedEndUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState invalidState = AbsoluteUtcTradeState.Traveling;

        //    AbsoluteUtcSaveData InvalidSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = invalidStartUtc.Ticks,
        //        expectedEndUtcTicks = invalidExpectedEndUtc.Ticks,
        //        currentState = (int)invalidState,
        //    };

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][Treveling Invariant]\n" +
        //        $"ValidApplyResult = {validApplyResult}\n" +
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

        // 9. Completed Invariant
        //{
        //    DateTime validStartUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    DateTime validExpectedEndUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData validSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = validStartUtc.Ticks,
        //        expectedEndUtcTicks = validExpectedEndUtc.Ticks,
        //        currentState = (int)validState,
        //    };

        //    bool validApplyResult = manager.ApplySnapshot(validSnapshot);

        //    DateTime firstStartUtc = manager.StartUtc;
        //    DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        //    AbsoluteUtcTradeState firstState = manager.CurrentState;


        //    DateTime invalidStartUtc = new DateTime(2026, 10, 22, 12, 1, 0, DateTimeKind.Utc);
        //    DateTime invalidExpectedEndUtc = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        //    AbsoluteUtcTradeState invalidState = AbsoluteUtcTradeState.Completed;

        //    AbsoluteUtcSaveData InvalidSnapshot = new AbsoluteUtcSaveData()
        //    {
        //        startUtcTicks = invalidStartUtc.Ticks,
        //        expectedEndUtcTicks = invalidExpectedEndUtc.Ticks,
        //        currentState = (int)invalidState,
        //    };

        //    bool invalidApplyResult = manager.ApplySnapshot(InvalidSnapshot);

        //    Debug.Log(
        //        $"[ApplySnapshot Test][Completed Invariant]\n" +
        //        $"ValidApplyResult = {validApplyResult}\n" +
        //        $"InvalidApplyResult = {invalidApplyResult}\n" +
        //        $"StartUtc = {firstStartUtc:O} -> {manager.StartUtc:O}\n" +
        //        $"ExpectedEndUtc = {firstExpectedEndUtc:O} -> {manager.ExpectedEndUtc:O}\n" +
        //        $"State = {firstState} -> {manager.CurrentState}");
        //}

    }

    private void PrintDebugLog()
    {
        DateTime currentUtc = DateTime.UtcNow;

        Debug.Log($"[Debug Log] Start UTC : {manager.StartUtc}");
        Debug.Log($"[Debug Log] Expected End UTC : {manager.ExpectedEndUtc}");
        Debug.Log($"[Debug Log] Current UTC : {currentUtc}");
        Debug.Log($"[Debug Log] Current State : {manager.CurrentStateString}");
    }
}
