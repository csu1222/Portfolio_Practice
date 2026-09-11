using System;
using UnityEngine;

public class OfflineSaveService : MonoBehaviour
{
    private OfflineSaveData currentSaveData;

    public OfflineSaveData CurrentSaveData => currentSaveData;

    // JSON 파일이 저장될 경로
    private string path;

    public OfflineSaveData Save(OfflineSaveData runtimeData)
    {
        if (runtimeData == null)
        {
            return null;
        }

        currentSaveData = runtimeData;

        Debug.Log($"Save Result \n" +
            $" Elapsed = {currentSaveData.elapsed}\n" +
            $"LastAppliedUtcTicks = {currentSaveData.lastAppliedUtcTicks.ToString()} \n" +
            $"CurrentState = {currentSaveData.currentState.ToString()}");


        return currentSaveData;
    }

    public OfflineSaveData Load()
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("Load failed: SaveData does not exist.");
            return null;
        }


        return currentSaveData;
    }
}
