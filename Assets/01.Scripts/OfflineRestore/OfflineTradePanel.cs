using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineTradePanel : MonoBehaviour
{
    [SerializeField] private OfflineTradeManager manager;

    [SerializeField] private TMP_Text currentTradeState;
    [SerializeField] private Slider progressBar;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindAnyObjectByType<OfflineTradeManager>();
        }

        if (currentTradeState == null)
        {
            Debug.LogWarning("Current Trade State Text Field is Null");
        }
        if (progressBar == null)
        {
            Debug.LogWarning("Progress Bar Field is Null");
        }
    }


    private void Update()
    {
        if (currentTradeState != null)
        {
            currentTradeState.text = manager.CurrentStateString;
        }

        if (progressBar != null)
        {
            progressBar.value = manager.Progress;
        }

    }
}
