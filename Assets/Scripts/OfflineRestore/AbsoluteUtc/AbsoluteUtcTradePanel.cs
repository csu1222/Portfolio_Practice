using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbsoluteUtcTradePanel : MonoBehaviour
{
    [SerializeField] private AbsoluteUtcTradeManager manager;
    [SerializeField] private TMP_Text currentTradeState;
    [SerializeField] private TMP_Text remainingTimeText;
    [SerializeField] private Slider progressBar;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindAnyObjectByType<AbsoluteUtcTradeManager>();
        }
    }

    private void Update()
    {
        if (manager == null)
            return;

        DateTime currentUtc = DateTime.UtcNow;

        if (currentTradeState != null)
            currentTradeState.text = manager.CurrentStateString;

        if (remainingTimeText != null)
            remainingTimeText.SetText("Remaining: {0:1} s", manager.CalculateRemainingTime(currentUtc));

        if (progressBar != null)
        {
            float progress = 0f;
            if (manager.CurrentState == AbsoluteUtcTradeState.Completed)
                progress = 1f;
            else if (manager.CurrentState == AbsoluteUtcTradeState.Traveling)
                progress = manager.CalculateProgressRatio(currentUtc);

            progressBar.SetValueWithoutNotify(progress);
        }
    }
}
