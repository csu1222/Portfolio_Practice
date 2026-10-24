using PlasticGui;
using System;
using UnityEngine;

public enum AbsoluteUtcTradeState
{
    Prepare = 0,
    Traveling = 1,
    Completed = 2,
}
/*
 Prepare
→ StartUtc = DateTime.MinValue
→ ExpectedEndUtc = DateTime.MinValue

Traveling
→ StartUtc 유효
→ ExpectedEndUtc 유효
→ StartUtc < ExpectedEndUtc

Completed
→ StartUtc 유효
→ ExpectedEndUtc 유효
→ StartUtc < ExpectedEndUtc
 */

public class AbsoluteUtcTradeManager : MonoBehaviour
{
    [SerializeField] private float duration = 60f;

    private DateTime startUtc = DateTime.MinValue;
    private DateTime expectedEndUtc = DateTime.MinValue;
    private AbsoluteUtcTradeState currentState;

    public float Duration => duration;
    public DateTime StartUtc => startUtc;
    public DateTime ExpectedEndUtc => expectedEndUtc;
    public AbsoluteUtcTradeState CurrentState => currentState;
    public string CurrentStateString => currentState.ToString();

    private void Update()
    {
        if (currentState == AbsoluteUtcTradeState.Traveling)
        {
            DateTime currentUtc = DateTime.UtcNow;

            if (currentUtc >= expectedEndUtc)
            {
                Debug.Log($"[AbsolutUtc][TradeManager][Update] Trade Complete");

                currentState = AbsoluteUtcTradeState.Completed;
            }
        }
    }

    public void TradeStart()
    {
        if (currentState == AbsoluteUtcTradeState.Prepare)
        {
            DateTime currentUtc = DateTime.UtcNow;

            startUtc = currentUtc;
            expectedEndUtc = startUtc.AddSeconds(duration);
            currentState = AbsoluteUtcTradeState.Traveling;
        }
        else if (currentState == AbsoluteUtcTradeState.Traveling)
        {
            // 이미 무역 진행중 
            Debug.Log($"[AbsoluteUtc][TradeManager][TradeStart] Already Traveling");
        }
        else if (currentState == AbsoluteUtcTradeState.Completed)
        {
            startUtc = DateTime.MinValue;
            expectedEndUtc = DateTime.MinValue;
            currentState = AbsoluteUtcTradeState.Prepare;
        }
    }

    public bool Restore(DateTime currentUtc)
    {
        if (currentState == AbsoluteUtcTradeState.Prepare)
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][Restore] Prepare. Restore false");
            return false;
        }
        else if (currentState == AbsoluteUtcTradeState.Traveling)
        {
            if (currentUtc < startUtc)
            {
                Debug.Log($"[AbsoluteUtc][TradeManager][Restore] Invalid Time. Restore false");
                return false;
            }

            if (startUtc <= currentUtc && currentUtc < expectedEndUtc)
            {
                float restoreRatio = CalculateProgressRatio(currentUtc) * 100;
                Debug.Log($"[AbsoluteUtc][TradeManager][Restore] Already traveling. Restore ");
                Debug.Log($"[AbsoluteUtc][TradeManager][Restore] Restore Ratio : {restoreRatio} % ");

                return true;
            }

            if (currentUtc >= expectedEndUtc)
            {
                float restoreRatio = CalculateProgressRatio(currentUtc) * 100;
                Debug.Log($"[AbsoluteUtc][TradeManager][Restore] Traveling Ended. Restore ");
                Debug.Log($"[AbsoluteUtc][TradeManager][Restore] Restore Ratio : {restoreRatio} % ");

                currentState = AbsoluteUtcTradeState.Completed;

                return true;
            }

        }
        else if (currentState == AbsoluteUtcTradeState.Completed)
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][Restore] Complete. Restore false");
            return false;
        }
        return false;
    }

    public AbsoluteUtcSaveData CreateSnapshot()
    {
        AbsoluteUtcSaveData snapshot = new AbsoluteUtcSaveData();

        snapshot.startUtcTicks = startUtc.Ticks;
        snapshot.expectedEndUtcTicks = expectedEndUtc.Ticks;
        snapshot.currentState = (int)currentState;

        return snapshot;
    }

    public bool ApplySnapshot(AbsoluteUtcSaveData snapshot)
    {
        if (snapshot == null)
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] Null Snapshot. ApplySnapshot false");
            return false;
        }

        if (snapshot.startUtcTicks < DateTime.MinValue.Ticks)
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] StartUtc less than DateTime MinValue. ApplySnapshot false");
            return false;
        }

        if (snapshot.startUtcTicks > DateTime.MaxValue.Ticks)
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] StartUtc Greater than DateTime MaxValue. ApplySnapshot false");
            return false;
        }

        if (snapshot.expectedEndUtcTicks < DateTime.MinValue.Ticks)
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] Expected End Utc less than DateTime MinValue. ApplySnapshot false");
            return false;
        }

        if (snapshot.expectedEndUtcTicks > DateTime.MaxValue.Ticks)
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] Expected End Utc Greater than DateTime MaxValue. ApplySnapshot false");
            return false;
        }

        if (!Enum.IsDefined(typeof(AbsoluteUtcTradeState), snapshot.currentState))
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] Current State is undefined. ApplySnapshot false");
            return false;
        }

        if (snapshot.currentState == (int)AbsoluteUtcTradeState.Prepare && 
            (snapshot.startUtcTicks != DateTime.MinValue.Ticks || 
            snapshot.expectedEndUtcTicks != DateTime.MinValue.Ticks))
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] Prepare State boundary condition is invalid. ApplySnapshot false");
            return false;
        }

        if (snapshot.currentState == (int)AbsoluteUtcTradeState.Traveling && 
            (snapshot.startUtcTicks <= DateTime.MinValue.Ticks || 
            snapshot.expectedEndUtcTicks <= DateTime.MinValue.Ticks ||
            snapshot.startUtcTicks >= snapshot.expectedEndUtcTicks))
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] Traveling State boundary condition is invalid. ApplySnapshot false");
            return false;
        }

        if (snapshot.currentState == (int)AbsoluteUtcTradeState.Completed && 
            (snapshot.startUtcTicks <= DateTime.MinValue.Ticks || 
            snapshot.expectedEndUtcTicks <= DateTime.MinValue.Ticks ||
            snapshot.startUtcTicks >= snapshot.expectedEndUtcTicks))
        {
            Debug.Log($"[AbsoluteUtc][TradeManager][ApplySnapshot] Completed State boundary condition is invalid. ApplySnapshot false");
            return false;
        }

        startUtc = new DateTime(snapshot.startUtcTicks, DateTimeKind.Utc);
        expectedEndUtc = new DateTime(snapshot.expectedEndUtcTicks, DateTimeKind.Utc);
        currentState = (AbsoluteUtcTradeState)snapshot.currentState;

        return true;
    }

    public float CalculateElapsedTime(DateTime currentUtc)
    {
        if (currentState == AbsoluteUtcTradeState.Prepare)
            return 0f;

        double elapsedSeconds = (currentUtc - startUtc).TotalSeconds;

        return Mathf.Clamp((float)elapsedSeconds, 0f, duration);
    }

    public float CalculateRemainingTime(DateTime currentUtc)
    {
        if (currentState == AbsoluteUtcTradeState.Prepare)
            return 0f;

        double remainingSeconds = (expectedEndUtc - currentUtc).TotalSeconds;

        return Mathf.Clamp((float)remainingSeconds, 0f, duration);
    }

    public float CalculateProgressRatio(DateTime currentUtc)
    {
        if (currentState == AbsoluteUtcTradeState.Prepare)
            return 0f;

        if (duration <= 0f)
            return 0f;

        float elapsed = CalculateElapsedTime(currentUtc);

        return Mathf.Clamp01(elapsed / duration);
    }
}
