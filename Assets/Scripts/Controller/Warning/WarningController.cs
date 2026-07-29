using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WarningController : MonoBehaviour
{
    [Header("Thiết lập hiệu ứng")]
    [SerializeField] private Image warningImage;      // Ảnh đỏ phủ toàn màn hình
    [SerializeField] private float flashDuration = 0.9f;  // Thời gian 1 nhịp nháy
    [SerializeField] private int flashCount = 3;          // Số lần nháy
    [SerializeField] private float maxAlpha = 0.6f;       // Độ sáng tối đa của màu đỏ
    [SerializeField] private bool isPlaying = false;

    private void OnEnable()
    {
        PlayWarning();
    }
    public void PlayWarning()
    {
        if (isPlaying) return;
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        isPlaying = true;

        for (int i = 0; i < flashCount; i++)
        {
            // fade in
            warningImage.DOFade(maxAlpha, flashDuration / 2f);
            yield return new WaitForSeconds(flashDuration / 2f);

            // fade out
            warningImage.DOFade(0f, flashDuration / 2f);
            yield return new WaitForSeconds(flashDuration / 2f);
        }

        isPlaying = false;
        this.gameObject.SetActive(false);
    }
}
