using DataAccount;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FreeHeartTimeText : MonoBehaviour
{
    [SerializeField] private Text timeTxt;

    private int _shownTotalSecs = -1;

    private void Awake()
    {
        if (timeTxt == null)
            timeTxt = GetComponent<Text>();
    }

    private void OnEnable()
    {
        _shownTotalSecs = -1;
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (timeTxt == null)
            return;

        var data = DataAccountPlayer.PlayerResourceData;
        if (data == null || !data.IsInfiniteHeartActive())
            return;

        long remainingMs = data.GetRemainingRecoverySeconds();
        int totalSecs = Mathf.Max(0, Mathf.CeilToInt(remainingMs / 1000f));
        if (totalSecs == _shownTotalSecs)
            return;

        _shownTotalSecs = totalSecs;
        int hours = totalSecs / 3600;
        int mins = (totalSecs % 3600) / 60;
        int secs = totalSecs % 60;
        timeTxt.text = $"{hours:D2}:{mins:D2}:{secs:D2}";
    }
}
