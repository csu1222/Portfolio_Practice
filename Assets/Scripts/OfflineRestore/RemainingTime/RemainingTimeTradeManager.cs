using System;
using UnityEngine;


/*
 Prepare
¡æ remainingTime == 0

Traveling
¡æ 0 < remainingTime <= duration

Completed
¡æ remainingTime == 0
 */
public enum RemainingTimeTradeState
{
    Prepare = 0,
    Traveling = 1,
    Completed = 2,
}

public class RemainingTimeTradeManager : MonoBehaviour
{
    [SerializeField] private float duration = 60f;
    private float remainingTime = 0f;

    private DateTime lastAppliedUtc;

    private RemainingTimeTradeState currentState;

    public float Duration => duration;
    public float RemainingTime => remainingTime;
    public float Progress => (duration - remainingTime) / duration;
    public DateTime LastAppliedUtc => lastAppliedUtc;
    public RemainingTimeTradeState CurrentState => currentState;
    public string CurrentStateString => currentState.ToString();

    private void Update()
    {
        if (currentState == RemainingTimeTradeState.Traveling)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                currentState = RemainingTimeTradeState.Completed;
            }
        }
    }

    public void TradeStart()
    {
        if (currentState == RemainingTimeTradeState.Prepare)
        {
            Debug.Log($"[Trade Manager][TradeStart] Current State : {currentState}");
            remainingTime = duration;
            lastAppliedUtc = DateTime.UtcNow;
            currentState = RemainingTimeTradeState.Traveling;
            return;
        }

        if (currentState == RemainingTimeTradeState.Traveling)
        {
            Debug.Log($"[Trade Manager][TradeStart] Current State : {currentState}");
            return;
        }

        if (currentState == RemainingTimeTradeState.Completed)
        {
            Debug.Log($"[Trade Manager][TradeStart] Current State : {currentState}");
            lastAppliedUtc = DateTime.UtcNow;
            currentState = RemainingTimeTradeState.Prepare;
            return;
        }
    }

    public bool Restore(DateTime currentUtc)
    {
        if (currentState == RemainingTimeTradeState.Prepare)
        {
            Debug.Log($"[Trade Manager][Restore] Current State : {currentState}");
            return false;
        }

        if (currentState == RemainingTimeTradeState.Traveling)
        {
            Debug.Log($"[Trade Manager][Restore] LastAppliedUtc : {lastAppliedUtc},  CurrentUtc : {currentUtc}");
            if (lastAppliedUtc > currentUtc)
            {
                return false;
            }

            double offlineElapsed = (currentUtc - lastAppliedUtc).TotalSeconds;

            float newRemainingTime = Mathf.Clamp((float)(remainingTime - offlineElapsed), 0f, duration);

            if (newRemainingTime <= 0f)
            {
                currentState = RemainingTimeTradeState.Completed;
            }

            remainingTime = newRemainingTime;

            lastAppliedUtc = currentUtc;

            return true;
        }

        if (currentState == RemainingTimeTradeState.Completed)
        {
            Debug.Log($"[Trade Manager][Restore] Current State : {currentState}");
            return false; 
        }

        return false;
    }

    public bool ApplySnapshot(RemainingTimeSaveData snapshot)
    {
        if (snapshot == null)
            return false;

        if (float.IsNaN(snapshot.remainingTime) || float.IsInfinity(snapshot.remainingTime))
            return false;

        if (snapshot.remainingTime < 0 || snapshot.remainingTime > duration)
            return false;

        if (snapshot.lastAppliedUtcTicks < DateTime.MinValue.Ticks || snapshot.lastAppliedUtcTicks > DateTime.MaxValue.Ticks)
            return false;

        if (!Enum.IsDefined(typeof(RemainingTimeTradeState), (RemainingTimeTradeState)snapshot.currentState))
            return false;

        if (snapshot.currentState == (int)RemainingTimeTradeState.Prepare && snapshot.remainingTime != 0f)
            return false;

        if (snapshot.currentState == (int)RemainingTimeTradeState.Traveling && (snapshot.remainingTime <= 0f || snapshot.remainingTime > duration))
            return false;

        if (snapshot.currentState == (int)RemainingTimeTradeState.Completed && snapshot.remainingTime != 0f)
            return false;

        remainingTime = snapshot.remainingTime;
        DateTime snapshotAppliedUtc = new DateTime(snapshot.lastAppliedUtcTicks, DateTimeKind.Utc);
        lastAppliedUtc = snapshotAppliedUtc;
        currentState = (RemainingTimeTradeState)snapshot.currentState;

        return true;
    }

    public RemainingTimeSaveData CreateSnapshot()
    {
        RemainingTimeSaveData snapshot = new RemainingTimeSaveData();

        DateTime snapshotUtc = DateTime.UtcNow;
        lastAppliedUtc = snapshotUtc;

        snapshot.remainingTime = remainingTime;
        snapshot.lastAppliedUtcTicks = snapshotUtc.Ticks;
        snapshot.currentState = (int)currentState;

        return snapshot;
    }
}
