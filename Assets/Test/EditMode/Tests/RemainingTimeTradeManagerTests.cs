using NUnit.Framework;
using UnityEngine;
using System;

public class RemainingTimeTradeManagerTests
{
    private GameObject gameObject;
    private RemainingTimeTradeManager manager;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        manager = gameObject.AddComponent<RemainingTimeTradeManager>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ApplySnapshot_ValidTravelingSnapshot_AppliesRuntimeState()
    {
        // Arrange

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(5f, manager.RemainingTime);
        Assert.AreEqual(RemainingTimeTradeState.Traveling, manager.CurrentState);
        Assert.AreEqual(snapshot.lastAppliedUtcTicks, manager.LastAppliedUtc.Ticks);
    }

    [Test]
    public void ApplySnapshot_NullSnapshot_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        RemainingTimeSaveData snapshot = null;

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_RemainingTimeIsNaN_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = float.NaN,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_RemainingTimeIsPositiveInfinity_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = float.PositiveInfinity,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_RemainingTimeIsNegativeInfinity_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = float.NegativeInfinity,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_RemainingTimeLessThanZero_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = -1f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_RemainingTimeGreaterThanDuration_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = manager.Duration + 1f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_LastAppliedUtcLessThanDateTimeMinValue_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        long lessThanDateTimeMinValue = DateTime.MinValue.Ticks - 1;

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = lessThanDateTimeMinValue,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_LastAppliedUtcGreaterThanDateTimeMaxValue_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        long greaterThanDateTimeMaxValue = DateTime.MaxValue.Ticks + 1;

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = greaterThanDateTimeMaxValue,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_InvalidCurrentState_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = 999
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_PrepareAndRemainingTimeNotZero_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Prepare
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_TravelingAndRemainingTimeLessThanZeroOrEqual_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 0f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_CompletedAndRemainingTimeNotZero_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeRemainingTime = manager.RemainingTime;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState validRuntimeCurrentState = manager.CurrentState;

        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Completed
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeRemainingTime, manager.RemainingTime);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);
    }

    [Test]
    public void Restore_TravelingWithValidCurrentUtc_DecreasesRemainingTime()
    {
        // Arrange
        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        DateTime currentUtc = snapshotUtc.AddSeconds(3);

        // Act
        bool applySnapshotResult = manager.ApplySnapshot(snapshot);

        bool restoreSnapshotResult = manager.Restore(currentUtc);

        // Assert
        Assert.IsTrue(applySnapshotResult);
        Assert.IsTrue(restoreSnapshotResult); 
        Assert.AreEqual(2f, manager.RemainingTime);
        Assert.AreEqual(currentUtc, manager.LastAppliedUtc);
        Assert.AreEqual(RemainingTimeTradeState.Traveling, manager.CurrentState);
    }

    [Test]
    public void Restore_ClockRollback_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        DateTime currentUtc = snapshotUtc.AddSeconds(-3);

        // Act
        bool applySnapshotResult = manager.ApplySnapshot(snapshot);

        float beforeRemainingTime = manager.RemainingTime;
        DateTime beforeLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState beforeState = manager.CurrentState;

        bool restoreSnapshotResult = manager.Restore(currentUtc);

        float afterRemainingTime = manager.RemainingTime;
        DateTime afterLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState afterState = manager.CurrentState;

        // Assert
        Assert.IsTrue(applySnapshotResult);
        Assert.IsFalse(restoreSnapshotResult); 
        Assert.AreEqual(beforeRemainingTime, afterRemainingTime);
        Assert.AreEqual(beforeLastAppliedUtc, afterLastAppliedUtc);
        Assert.AreEqual(beforeState, afterState);
    }

    [Test]
    public void Restore_SameCurrentUtcTwice_DoesNotChangeRuntimeAgain()
    {
        // Arrange
        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 30f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        DateTime currentUtc = snapshotUtc.AddSeconds(10);

        // Act
        bool applySnapshotResult = manager.ApplySnapshot(snapshot);

        bool firstRestoreSnapshotResult = manager.Restore(currentUtc);

        float firstRemainingTime = manager.RemainingTime;
        DateTime firstLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState firstState = manager.CurrentState;

        bool secondRestoreSnapshotResult = manager.Restore(currentUtc);

        float secondRemainingTime = manager.RemainingTime;
        DateTime secondLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState secondState = manager.CurrentState;

        // Assert
        Assert.IsTrue(applySnapshotResult);
        Assert.IsTrue(firstRestoreSnapshotResult);
        Assert.AreEqual(20f, firstRemainingTime);
        Assert.AreEqual(currentUtc, firstLastAppliedUtc);
        Assert.AreEqual(RemainingTimeTradeState.Traveling, firstState);

        Assert.IsTrue(secondRestoreSnapshotResult); 
        Assert.AreEqual(firstRemainingTime, secondRemainingTime);
        Assert.AreEqual(firstLastAppliedUtc, secondLastAppliedUtc);
        Assert.AreEqual(firstState, secondState);
    }

    [Test]
    public void Restore_OfflineElapsedExceedsRemainingTime_ClampsToZeroAndCompletes()
    {
        // Arrange
        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 5f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        DateTime currentUtc = snapshotUtc.AddSeconds(10);

        // Act
        bool applySnapshotResult = manager.ApplySnapshot(snapshot);

        bool restoreSnapshotResult = manager.Restore(currentUtc);

        // Assert
        Assert.IsTrue(applySnapshotResult);
        Assert.IsTrue(restoreSnapshotResult);
        Assert.AreEqual(0f, manager.RemainingTime);
        Assert.AreEqual(currentUtc, manager.LastAppliedUtc);
        Assert.AreEqual(RemainingTimeTradeState.Completed, manager.CurrentState);
    }

    [Test]
    public void Restore_PrepareState_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 0f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Prepare
        };

        DateTime currentUtc = snapshotUtc.AddSeconds(10);

        // Act
        bool applySnapshotResult = manager.ApplySnapshot(snapshot);

        float appliedRemainingTime = manager.RemainingTime;
        DateTime appliedLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState appliedState = manager.CurrentState;

        bool restoreSnapshotResult = manager.Restore(currentUtc);

        // Assert
        Assert.IsTrue(applySnapshotResult);
        Assert.IsFalse(restoreSnapshotResult);
        Assert.AreEqual(appliedRemainingTime, manager.RemainingTime);
        Assert.AreEqual(appliedLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(appliedState, manager.CurrentState);
    }

    [Test]
    public void Restore_CompleteState_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        DateTime snapshotUtc =
            new DateTime(2026, 9, 30, 6, 0, 0, DateTimeKind.Utc);

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData()
        {
            remainingTime = 0f,
            lastAppliedUtcTicks = snapshotUtc.Ticks,
            currentState = (int)RemainingTimeTradeState.Completed
        };

        DateTime currentUtc = snapshotUtc.AddSeconds(10);

        // Act
        bool applySnapshotResult = manager.ApplySnapshot(snapshot);

        float appliedRemainingTime = manager.RemainingTime;
        DateTime appliedLastAppliedUtc = manager.LastAppliedUtc;
        RemainingTimeTradeState appliedState = manager.CurrentState;

        bool restoreSnapshotResult = manager.Restore(currentUtc);

        // Assert
        Assert.IsTrue(applySnapshotResult);
        Assert.IsFalse(restoreSnapshotResult);
        Assert.AreEqual(appliedRemainingTime, manager.RemainingTime);
        Assert.AreEqual(appliedLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(appliedState, manager.CurrentState);
    }

    private void SetValidRuntime(RemainingTimeTradeManager manager)
    {
        RemainingTimeSaveData validRuntimeData = new RemainingTimeSaveData()
        {
            remainingTime = 10f,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)RemainingTimeTradeState.Traveling
        };

        bool result = manager.ApplySnapshot(validRuntimeData);
        Assert.IsTrue(result);
    }
}
