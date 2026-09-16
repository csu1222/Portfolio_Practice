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

    public void StartButton()
    {
        Debug.Log("Start Buton");
        offlineTradeManager.TradeStart();
    }

    public void SaveButton()
    {
        Debug.Log("Save Button");
        offlineSaveService.Save(offlineTradeManager.CreateSnapShot());

        Debug.Log($"[DebugPanel] Manger.LastAppliedUtc.Ticks : {offlineTradeManager.LastAppliedUtc.Ticks}\n" +
            $"[DebugPanel] SaveData.lastAppliedUtcTicks : {offlineSaveService.CurrentSaveData.lastAppliedUtcTicks}");
    }

    public void LoadButton()
    {
        Debug.Log("Load Button");
        OfflineSaveData loadedData = offlineSaveService.Load();
        if (loadedData != null)
        {
            offlineTradeManager.ApplySnapShot(loadedData);
        }


        Debug.Log($"[DebugPanel] Load After SaveData \n" +
            $"[DebugPanel] Elapsed : {offlineSaveService.CurrentSaveData.elapsed}\n" +
            $"[DebugPanel] State : {(OfflineTradeState)offlineSaveService.CurrentSaveData.currentState}\n" +
            $"[DebugPanel] LastAppliedUtc : {offlineSaveService.CurrentSaveData.lastAppliedUtcTicks}\n");


        Debug.Log($"[DebugPanel] Load After RuntimeData \n" +
            $"[DebugPanel] Elapsed : {offlineTradeManager.Elapsed}\n" +
            $"[DebugPanel] State : {offlineTradeManager.CurrentState}\n" +
            $"[DebugPanel] LastAppliedUtc : {offlineTradeManager.LastAppliedUtc.Ticks}\n");
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

        // null test
        //loadedData = null;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        // -1 test
        //loadedData.elapsed = -1;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //elapsed > duration test
        //loadedData.elapsed = 61;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //elapsed = Nan test
        //loadedData.elapsed = float.NaN;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //elapsed = Infinit test
        //loadedData.elapsed = float.PositiveInfinity;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //state invalid test
        //loadedData.currentState = 999;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //prepare and elapsed = 1 test
        //loadedData.currentState = 0;
        //loadedData.elapsed = 1;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //traveling and elapsed = 60 test
        //loadedData.currentState = 1;
        //loadedData.elapsed = 60;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //complete and elapsed < 60  test
        //loadedData.currentState = 2;
        //loadedData.elapsed = 30;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //lastAppliedUtcTicks < MinValue  test
        //loadedData.lastAppliedUtcTicks = DateTime.MinValue.Ticks - 1;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);

        //lastAppliedUtcTicks > MaxValue  test
        //loadedData.lastAppliedUtcTicks = DateTime.MaxValue.Ticks + 1;
        //if (offlineTradeManager.ApplySnapShot(loadedData))
        //    offlineTradeManager.Restore(DateTime.UtcNow);
    }

}
