using NUnit.Framework;
using UnityEngine;
using System;

public class AbsoluteUtcTradeManagerTests
{
    private GameObject gameObject;
    private AbsoluteUtcTradeManager manager;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        manager = gameObject.AddComponent<AbsoluteUtcTradeManager>();
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
        DateTime validStartUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        DateTime validExpectedEndUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Traveling;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_NullSnapshot_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = null;

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_StartUtcTicksLessThanMinValue_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = DateTime.MinValue.Ticks - 1,
            expectedEndUtcTicks = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc).Ticks,
            currentState = (int)AbsoluteUtcTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_StartUtcTicksGreaterThanMaxValue_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = DateTime.MaxValue.Ticks + 1,
            expectedEndUtcTicks = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc).Ticks,
            currentState = (int)AbsoluteUtcTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_ExpectedEndUtcTicksLessThanMinValue_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc).Ticks,
            expectedEndUtcTicks = DateTime.MinValue.Ticks - 1,
            currentState = (int)AbsoluteUtcTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_ExpectedEndUtcTicksGreaterThanMaxValue_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc).Ticks,
            expectedEndUtcTicks = DateTime.MaxValue.Ticks + 1,
            currentState = (int)AbsoluteUtcTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_InvalidCurrentState_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc).Ticks,
            expectedEndUtcTicks = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc).Ticks,
            currentState = 999
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_PrepareStateWithAssignedUtc_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc).Ticks,
            expectedEndUtcTicks = DateTime.MinValue.Ticks,
            currentState = (int)AbsoluteUtcTradeState.Prepare
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_TravelingStateWithInvalidUtcOrder_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc).Ticks,
            expectedEndUtcTicks = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc).Ticks,
            currentState = (int)AbsoluteUtcTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void ApplySnapshot_CompletedStateWithInvalidUtcOrder_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        SetValidRuntime(manager);

        DateTime validStartUtc = manager.StartUtc;
        DateTime validExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState validState = manager.CurrentState;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc).Ticks,
            expectedEndUtcTicks = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc).Ticks,
            currentState = (int)AbsoluteUtcTradeState.Completed
        };

        // Act
        bool result = manager.ApplySnapshot(snapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
    }

    [Test]
    public void Restore_TravelingWithValidCurrentUtc_ReturnsTrueAndKeepsTraveling()
    {
        // Arrange
        DateTime validStartUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        DateTime validExpectedEndUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Traveling;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        DateTime currentUtc = new DateTime(2026, 10, 23, 12, 0, 30, DateTimeKind.Utc);


        // Act
        bool applyResult = manager.ApplySnapshot(snapshot);

        bool restoreResult = manager.Restore(currentUtc);

        // Assert

        Assert.IsTrue(applyResult);
        Assert.IsTrue(restoreResult);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState); 
        Assert.AreEqual(0.5f, manager.CalculateProgressRatio(currentUtc));
        Assert.AreEqual(30f, manager.CalculateRemainingTime(currentUtc));
    }

    [Test]
    public void Restore_CurrentUtcAfterExpectedEndUtc_ReturnsTrueAndCompletesTrade()
    {
        // Arrange
        DateTime validStartUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        DateTime validExpectedEndUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Traveling;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        DateTime currentUtc = new DateTime(2026, 10, 23, 12, 1, 10, DateTimeKind.Utc);


        // Act
        bool applyResult = manager.ApplySnapshot(snapshot);

        bool restoreResult = manager.Restore(currentUtc);

        // Assert

        Assert.IsTrue(applyResult);
        Assert.IsTrue(restoreResult);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(AbsoluteUtcTradeState.Completed, manager.CurrentState); 
        Assert.AreEqual(1f, manager.CalculateProgressRatio(currentUtc));
        Assert.AreEqual(0f, manager.CalculateRemainingTime(currentUtc));
    }

    [Test]
    public void Restore_CurrentUtcBeforeStartUtc_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        DateTime validStartUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        DateTime validExpectedEndUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Traveling;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        DateTime currentUtc = new DateTime(2026, 10, 23, 11, 59, 50, DateTimeKind.Utc);


        // Act
        bool applyResult = manager.ApplySnapshot(snapshot);

        bool restoreResult = manager.Restore(currentUtc);

        // Assert

        Assert.IsTrue(applyResult);
        Assert.IsFalse(restoreResult);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState); 
    }

    [Test]
    public void Restore_CurrentUtcEqualsStartUtc_ReturnsTrueAndStartsAtZeroProgress()
    {
        // Arrange
        DateTime validStartUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        DateTime validExpectedEndUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Traveling;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        DateTime currentUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);


        // Act
        bool applyResult = manager.ApplySnapshot(snapshot);

        bool restoreResult = manager.Restore(currentUtc);

        // Assert

        Assert.IsTrue(applyResult);
        Assert.IsTrue(restoreResult);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(validState, manager.CurrentState);
        Assert.AreEqual(0f, manager.CalculateProgressRatio(currentUtc));
        Assert.AreEqual(manager.Duration, manager.CalculateRemainingTime(currentUtc));
    }

    [Test]
    public void Restore_CurrentUtcEqualsExpectedEndUtc_ReturnsTrueAndCompletesTrade()
    {
        // Arrange
        DateTime validStartUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        DateTime validExpectedEndUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Traveling;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        DateTime currentUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);


        // Act
        bool applyResult = manager.ApplySnapshot(snapshot);

        bool restoreResult = manager.Restore(currentUtc);

        // Assert

        Assert.IsTrue(applyResult);
        Assert.IsTrue(restoreResult);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(AbsoluteUtcTradeState.Completed, manager.CurrentState);
        Assert.AreEqual(1f, manager.CalculateProgressRatio(currentUtc));
        Assert.AreEqual(0f, manager.CalculateRemainingTime(currentUtc));
    }

    [Test]
    public void Restore_SameCurrentUtcTwice_ReturnsSameResultWithoutAccumulation()
    {
        // Arrange
        DateTime validStartUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        DateTime validExpectedEndUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Traveling;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        DateTime currentUtc = new DateTime(2026, 10, 23, 12, 0, 30, DateTimeKind.Utc);

        // Act
        bool applyResult = manager.ApplySnapshot(snapshot);

        bool firstRestoreResult = manager.Restore(currentUtc);

        DateTime firstStartUtc = manager.StartUtc;
        DateTime firstExpectedEndUtc = manager.ExpectedEndUtc;
        AbsoluteUtcTradeState firstState = manager.CurrentState;
        float firstProgressRatio = manager.CalculateProgressRatio(currentUtc);

        bool secondRestoreResult = manager.Restore(currentUtc);
        float secondProgressRatio = manager.CalculateProgressRatio(currentUtc);

        // Assert

        Assert.IsTrue(applyResult);
        Assert.IsTrue(firstRestoreResult);
        Assert.IsTrue(secondRestoreResult);
        
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(AbsoluteUtcTradeState.Traveling, manager.CurrentState);

        Assert.AreEqual(firstStartUtc, manager.StartUtc);
        Assert.AreEqual(firstExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(firstState, manager.CurrentState);

        Assert.AreEqual(0.5f, firstProgressRatio);
        Assert.AreEqual(0.5f, secondProgressRatio);
    }

    [Test]
    public void Restore_PrepareState_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        DateTime validStartUtc = DateTime.MinValue;
        DateTime validExpectedEndUtc = DateTime.MinValue;
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Prepare;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        DateTime currentUtc = new DateTime(2026, 10, 23, 12, 0, 30, DateTimeKind.Utc);

        // Act
        bool applyResult = manager.ApplySnapshot(snapshot);

        bool restoreResult = manager.Restore(currentUtc);

        // Assert

        Assert.IsTrue(applyResult);
        Assert.IsFalse(restoreResult);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(AbsoluteUtcTradeState.Prepare, manager.CurrentState);
    }

    [Test]
    public void Restore_CompletedState_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        DateTime validStartUtc = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        DateTime validExpectedEndUtc = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc);
        AbsoluteUtcTradeState validState = AbsoluteUtcTradeState.Completed;

        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = validStartUtc.Ticks,
            expectedEndUtcTicks = validExpectedEndUtc.Ticks,
            currentState = (int)validState
        };

        DateTime currentUtc = new DateTime(2026, 10, 23, 12, 1, 30, DateTimeKind.Utc);

        // Act
        bool applyResult = manager.ApplySnapshot(snapshot);

        bool restoreResult = manager.Restore(currentUtc);

        // Assert

        Assert.IsTrue(applyResult);
        Assert.IsFalse(restoreResult);
        Assert.AreEqual(validStartUtc, manager.StartUtc);
        Assert.AreEqual(validExpectedEndUtc, manager.ExpectedEndUtc);
        Assert.AreEqual(AbsoluteUtcTradeState.Completed, manager.CurrentState);
    }

    private void SetValidRuntime(AbsoluteUtcTradeManager manager)
    {
        AbsoluteUtcSaveData validRuntimeData = new AbsoluteUtcSaveData()
        {
            startUtcTicks = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc).Ticks,
            expectedEndUtcTicks = new DateTime(2026, 10, 23, 12, 1, 0, DateTimeKind.Utc).Ticks,
            currentState = (int)AbsoluteUtcTradeState.Traveling
        };

        bool result = manager.ApplySnapshot(validRuntimeData);
        Assert.IsTrue(result);
    }
}
