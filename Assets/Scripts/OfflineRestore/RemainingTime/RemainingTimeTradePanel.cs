using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemainingTimeTradePanel : MonoBehaviour
{
    [SerializeField] private RemainingTimeTradeManager manager;
    [SerializeField] private TMP_Text currentTradeState;
    [SerializeField] private TMP_Text remainingTimeText;
    [SerializeField] private Slider progressBar;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindAnyObjectByType<RemainingTimeTradeManager>();
        }
    }

    private void Update()
    {
        if (manager == null)
            return;

        if (currentTradeState != null)
            currentTradeState.text = manager.CurrentStateString;

        if (remainingTimeText != null)
            remainingTimeText.SetText("Remaining: {0:1} s", manager.RemainingTime);

        if (progressBar != null)
        {
            // Prepare와 Completed는 모두 남은 시간이 0이므로 상태로 구분한다.
            float progress = 0f;
            if (manager.CurrentState == RemainingTimeTradeState.Completed)
                progress = 1f;
            else if (manager.CurrentState == RemainingTimeTradeState.Traveling && manager.Duration > 0f)
                progress = Mathf.Clamp01(manager.Progress);

            progressBar.SetValueWithoutNotify(progress);
        }
    }
}
