using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineTradePanel : MonoBehaviour
{
    [SerializeField] private OfflineTradeManager manager;

    [SerializeField] private TMP_Text currentTradeState;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Button tradeStartButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<OfflineTradeManager>();
        }

        if (currentTradeState == null)
        {
            Debug.LogWarning("Current Trade State Text Field is Null");
        }
        if (progressBar == null)
        {
            Debug.LogWarning("Progress Bar Field is Null");
        }
        if (tradeStartButton == null)
        {
            Debug.LogWarning("Tarde Start Button Field is Null");
        }
        if (saveButton == null)
        {
            Debug.LogWarning("Save Button Field is Null");
        }
        if (loadButton == null)
        {
            Debug.LogWarning("Load Button Field is Null");
        }
    }

    private void OnEnable()
    {
        tradeStartButton.onClick.AddListener(manager.TradeStart);
    }

    private void OnDisable()
    {
        tradeStartButton.onClick.RemoveListener(manager.TradeStart);
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
