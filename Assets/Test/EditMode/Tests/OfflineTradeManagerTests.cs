using NUnit.Framework;
using System;
using UnityEngine;

public class OfflineTradeManagerTests
{
    private GameObject gameObject;
    private OfflineTradeManager manager;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        manager = gameObject.AddComponent<OfflineTradeManager>();
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
        OfflineSaveData snapshot = new OfflineSaveData
        {
            elapsed = 20f,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapShot(snapshot);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(20f, manager.Elapsed);
        Assert.AreEqual(OfflineTradeState.Traveling, manager.CurrentState);
        Assert.AreEqual(snapshot.lastAppliedUtcTicks, manager.LastAppliedUtc.Ticks);

    }

    [Test]
    public void ApplySnapshot_NullSnapshot_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = null;

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_NegativeElapsed_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = -1f,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_InvalidState_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = 20f,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = 999
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_PrepareWithElapsed_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = 20f,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)OfflineTradeState.Prepare
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_TravelingAtDuration_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = manager.Duration,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_CompletedBeforeDuration_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = manager.Duration - 1f,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)OfflineTradeState.Completed
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_ElapsedIsFloatNan_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = float.NaN,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_ElapsedIsInfinity_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = float.PositiveInfinity,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_LastAppliedUtcTicksLessThanMinValue_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = 0f,
            lastAppliedUtcTicks = DateTime.MinValue.Ticks - 1,
            currentState = (int)OfflineTradeState.Prepare
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void ApplySnapshot_LastAppliedUtcTicksGreaterThanMaxValue_ReturnsFalse()
    {
        // Arrange
        SetValidRuntime(manager);

        float validRuntimeElapsed = manager.Elapsed;
        DateTime validRuntimeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState validRuntimeCurrentState = manager.CurrentState;

        OfflineSaveData invalidSnapshot = new OfflineSaveData
        {
            elapsed = 0f,
            lastAppliedUtcTicks = DateTime.MaxValue.Ticks + 1,
            currentState = (int)OfflineTradeState.Prepare
        };

        // Act
        bool result = manager.ApplySnapShot(invalidSnapshot);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(validRuntimeElapsed, manager.Elapsed);
        Assert.AreEqual(validRuntimeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(validRuntimeCurrentState, manager.CurrentState);

    }

    [Test]
    public void Restore_TravelingWithValidCurrentUtc_AdvancesElapsed()
    {
        // Arrange
        DateTime lastAppliedUtc = new DateTime(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);
        OfflineSaveData snapshot = new OfflineSaveData
        {
            elapsed = 10f,
            lastAppliedUtcTicks = lastAppliedUtc.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        Assert.IsTrue(manager.ApplySnapShot(snapshot));

        DateTime currentUtc = lastAppliedUtc.AddSeconds(5);

        // Act
        bool result = manager.Restore(currentUtc);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(15f, manager.Elapsed);
        Assert.AreEqual(currentUtc, manager.LastAppliedUtc);
        Assert.AreEqual(OfflineTradeState.Traveling, manager.CurrentState);
    }

    [Test]
    public void Restore_ClockRollback_ReturnsFalse()
    {
        // Arrange
        DateTime lastAppliedUtc = new DateTime(2026, 9, 16, 12, 0, 10, DateTimeKind.Utc);
        OfflineSaveData snapshot = new OfflineSaveData()
        {
            elapsed = 10f,
            lastAppliedUtcTicks = lastAppliedUtc.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        Assert.IsTrue(manager.ApplySnapShot(snapshot));

        DateTime currentUtc = lastAppliedUtc.AddSeconds(-5);

        // Act
        bool result = manager.Restore(currentUtc);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(snapshot.elapsed, manager.Elapsed);
        Assert.AreEqual(snapshot.lastAppliedUtcTicks, manager.LastAppliedUtc.Ticks);
        Assert.AreEqual(snapshot.currentState, (int)manager.CurrentState);
    }

    [Test]
    public void Restore_SameCurrentUtcTwice_DoesNotAdvanceElapsedAgain()
    {
        // Arrange
        DateTime lastAppliedUtc = new DateTime(2026, 9, 16, 12, 0, 10, DateTimeKind.Utc);
        OfflineSaveData snapshot = new OfflineSaveData()
        {
            elapsed = 10f,
            lastAppliedUtcTicks = lastAppliedUtc.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        Assert.IsTrue(manager.ApplySnapShot(snapshot));

        DateTime currentUtc = lastAppliedUtc.AddSeconds(5);

        // Act 1
        bool firstResult = manager.Restore(currentUtc);

        float firstElapsed = manager.Elapsed;
        DateTime firstLastAppliedUtc = manager.LastAppliedUtc;

        // Act 2
        bool secondResult = manager.Restore(currentUtc);

        // Assert
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);

        Assert.AreEqual(firstElapsed, manager.Elapsed);
        Assert.AreEqual(firstLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(OfflineTradeState.Traveling, manager.CurrentState);
    }

    [Test]
    public void Restore_ElapsedExceedsDuration_ClampsAndCompletes()
    {
        // Arrange
        DateTime lastAppliedUtc = new DateTime(2026, 9, 16, 12, 0, 10, DateTimeKind.Utc);
        OfflineSaveData snapshot = new OfflineSaveData()
        {
            elapsed = 10f,
            lastAppliedUtcTicks = lastAppliedUtc.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        Assert.IsTrue(manager.ApplySnapShot(snapshot));

        DateTime currentUtc = lastAppliedUtc.AddSeconds(manager.Duration);

        // Act
        bool result = manager.Restore(currentUtc);

        // Assert
        Assert.IsTrue(result);

        Assert.AreEqual(manager.Duration, manager.Elapsed);
        Assert.AreEqual(currentUtc, manager.LastAppliedUtc);
        Assert.AreEqual(OfflineTradeState.Completed, manager.CurrentState);
    }

    [Test]
    public void Restore_PrepareState_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        DateTime lastAppliedUtc = new DateTime(2026, 9, 16, 12, 0, 10, DateTimeKind.Utc);
        OfflineSaveData snapshot = new OfflineSaveData()
        {
            elapsed = 0f,
            lastAppliedUtcTicks = lastAppliedUtc.Ticks,
            currentState = (int)OfflineTradeState.Prepare
        };

        Assert.IsTrue(manager.ApplySnapShot(snapshot));

        float beforeElapsed = manager.Elapsed;
        DateTime beforeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState beforeState = manager.CurrentState;

        DateTime currentUtc = lastAppliedUtc.AddSeconds(5);

        // Act
        bool result = manager.Restore(currentUtc);

        // Assert
        Assert.IsFalse(result);

        Assert.AreEqual(beforeElapsed, manager.Elapsed);
        Assert.AreEqual(beforeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(beforeState, manager.CurrentState);
    }

    [Test]
    public void Restore_CompletedState_ReturnsFalseAndDoesNotChangeRuntime()
    {
        // Arrange
        DateTime lastAppliedUtc = new DateTime(2026, 9, 16, 12, 0, 10, DateTimeKind.Utc);
        OfflineSaveData snapshot = new OfflineSaveData()
        {
            elapsed = manager.Duration,
            lastAppliedUtcTicks = lastAppliedUtc.Ticks,
            currentState = (int)OfflineTradeState.Completed
        };

        Assert.IsTrue(manager.ApplySnapShot(snapshot));

        float beforeElapsed = manager.Elapsed;
        DateTime beforeLastAppliedUtc = manager.LastAppliedUtc;
        OfflineTradeState beforeState = manager.CurrentState;

        DateTime currentUtc = lastAppliedUtc.AddSeconds(5);

        // Act
        bool result = manager.Restore(currentUtc);

        // Assert
        Assert.IsFalse(result);

        Assert.AreEqual(beforeElapsed, manager.Elapsed);
        Assert.AreEqual(beforeLastAppliedUtc, manager.LastAppliedUtc);
        Assert.AreEqual(beforeState, manager.CurrentState);
    }

    private void SetValidRuntime(OfflineTradeManager manager)
    {
        OfflineSaveData validRuntimeData = new OfflineSaveData()
        {
            elapsed = 10f,
            lastAppliedUtcTicks = DateTime.UtcNow.Ticks,
            currentState = (int)OfflineTradeState.Traveling
        };

        bool result = manager.ApplySnapShot(validRuntimeData);
        Assert.IsTrue(result);
    }
}
