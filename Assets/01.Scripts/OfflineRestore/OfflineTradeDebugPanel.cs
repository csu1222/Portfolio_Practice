using System;
using UnityEngine;

public class OfflineTradeDebugPanel : MonoBehaviour
{
    [SerializeField] OfflineTradeManager offlineTradeManager;
    [SerializeField] OfflineSaveService offlineSaveService;

    private void Awake()
    {
        if (offlineTradeManager == null)
        {
            offlineTradeManager = FindAnyObjectByType<OfflineTradeManager>();
        }
        if (offlineSaveService == null)
        {
            offlineSaveService = FindAnyObjectByType<OfflineSaveService>();
        }
    }

    public void SaveButton()
    {
        Debug.Log("Save Button");
        offlineSaveService.Save(offlineTradeManager.CreateSnapShot());
    }

    public void LoadButton()
    {
        Debug.Log("Load Button");
        OfflineSaveData loadedData = offlineSaveService.Load();
        if (loadedData != null)
        {
            offlineTradeManager.ApplySnapShot(loadedData);
        }
    }

    public void ResetButton()
    {
        Debug.Log("Reset Button");
        offlineTradeManager.Reset();
    }

    public void RestoreButton()
    {

        Debug.Log("Restore Button");

        OfflineSaveData loadedData = offlineSaveService.Load();

        if (offlineTradeManager.ApplySnapShot(loadedData))
            offlineTradeManager.Restore(DateTime.UtcNow);

    }

}
