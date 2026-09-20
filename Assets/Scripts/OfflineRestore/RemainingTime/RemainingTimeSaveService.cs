using UnityEngine;

public class RemainingTimeSaveService : MonoBehaviour
{
    private RemainingTimeSaveData saveData;

    public bool Save(RemainingTimeSaveData snapshot)
    {
        if (snapshot == null)
            return false;

        RemainingTimeSaveData snapshotData = new RemainingTimeSaveData();

        snapshotData.remainingTime = snapshot.remainingTime;
        snapshotData.lastAppliedUtcTicks = snapshot.lastAppliedUtcTicks;
        snapshotData.currentState = snapshot.currentState;

        saveData = snapshotData;

        return true;
    }

    public RemainingTimeSaveData Load()
    {
        if (saveData == null)
            return null;

        RemainingTimeSaveData snapshot = new RemainingTimeSaveData();
        snapshot.remainingTime = saveData.remainingTime;
        snapshot.lastAppliedUtcTicks = saveData.lastAppliedUtcTicks;
        snapshot.currentState = saveData.currentState;

        return snapshot;
    }
}
