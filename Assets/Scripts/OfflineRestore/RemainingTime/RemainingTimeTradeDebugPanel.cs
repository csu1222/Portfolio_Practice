using UnityEngine;
using System;

public class RemainingTimeTradeDebugPanel : MonoBehaviour
{
    [SerializeField] private RemainingTimeTradeManager manager;
    [SerializeField] private RemainingTimeSaveService saveService;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindAnyObjectByType<RemainingTimeTradeManager>();
        }

        if (saveService == null)
        {
            saveService = FindAnyObjectByType<RemainingTimeSaveService>();
        }
    }

    public void TradeStartButton()
    {
        manager.TradeStart();
    }

    public void SaveButton()
    {
        saveService.Save(manager.CreateSnapshot());
    }

    public void LoadButton()
    {
        RemainingTimeSaveData snapshot = saveService.Load();
        if (snapshot != null)
        {
            manager.ApplySnapshot(snapshot);
        }
    }

    public void RestoreButton()
    {
        RemainingTimeSaveData snapshot = saveService.Load();
        if (snapshot != null)
        {
            if (manager.ApplySnapshot(snapshot))
            {
                manager.Restore(DateTime.UtcNow);
            }
        }
    }
}
