using UnityEngine;

public class AbsoluteUtcSaveService : MonoBehaviour
{
    private AbsoluteUtcSaveData saveData;

    public bool Save(AbsoluteUtcSaveData snapShot)
    {
        if (snapShot == null)
        {
            Debug.Log($"[AbsoluteUtc][SaveService][Save] Null Snapshot. Save false");
            return false;
        }

        AbsoluteUtcSaveData snapShotData = new AbsoluteUtcSaveData()
        {
            startUtcTicks = snapShot.startUtcTicks,
            expectedEndUtcTicks = snapShot.expectedEndUtcTicks,
            currentState = snapShot.currentState,
        };

        saveData = snapShotData;

        return true;
    }

    public AbsoluteUtcSaveData Load()
    {
        if (saveData == null)
        {
            Debug.Log($"[AbsoluteUtc][SaveService][Load] Null SaveData. Load false");
            return null;
        }

        AbsoluteUtcSaveData snapShot = new AbsoluteUtcSaveData()
        {
            startUtcTicks = saveData.startUtcTicks,
            expectedEndUtcTicks = saveData.expectedEndUtcTicks,
            currentState = saveData.currentState,
        };

        return snapShot;
    }
}
