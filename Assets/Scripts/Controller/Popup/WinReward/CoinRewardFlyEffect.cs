using System;
using System.Collections;
using System.Collections.Generic;
using Core.DesignPattern.Factory;
using Core.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Spawn đồng xu (id = "13" trong FactoryPoolData) từ vị trí hiện tại,
/// rải ngẫu nhiên quanh tâm rồi bay về goldBar của UIGameController.
/// Attach component này lên bất kỳ GameObject nào trong Popup Win.
/// </summary>
public class CoinRewardFlyEffect : MonoBehaviour
{
    [Title("Spawn Settings")]
    [LabelText("Coin Object ID")]
    [SerializeField] private string coinObjectId = "13";

    [LabelText("Spawn Count Min")]
    [SerializeField] private int spawnCountMin = 8;

    [LabelText("Spawn Count Max")]
    [SerializeField] private int spawnCountMax = 15;

    [Title("Spread Settings")]
    [LabelText("Spread Radius (world)")]
    [Tooltip("Bán kính tản ra trước khi bay về target (đơn vị world).")]
    [SerializeField] private float spreadRadius = 1.2f;

    [LabelText("Spread Duration")]
    [SerializeField] private float spreadDuration = 0.35f;

    [Title("Fly Settings")]
    [LabelText("Fly Duration")]
    [SerializeField] private float flyDuration = 0.55f;

    [LabelText("Fly Arc Height")]
    [Tooltip("Độ cao cung bay (world). 0 = đường thẳng.")]
    [SerializeField] private float flyArcHeight = 2.5f;

    [LabelText("Fly Delay Between Coins")]
    [Tooltip("Delay lệch nhau giữa các đồng xu để tạo cảm giác dồn dập.")]
    [SerializeField] private float staggerDelay = 0.04f;

    // ── Runtime ────────────────────────────────────────────────────────────
    private readonly List<GameObject> _spawnedCoins = new();
    private Coroutine _flyRoutine;

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Spawn coin từ pos zero, tản ra rồi bay về goldBar.
    /// </summary>
    [Button("▶ Test Play")]
    public void Play(Action onAllArrived = null)
    {
        if (_flyRoutine != null)
            StopCoroutine(_flyRoutine);

        Transform goldBarTransform = GetGoldBarTransform();
        if (goldBarTransform == null)
        {
            Debug.LogWarning("[CoinRewardFlyEffect] goldBar not found on UIGameController!");
            onAllArrived?.Invoke();
            return;
        }

        int count = Random.Range(spawnCountMin, spawnCountMax + 1);
        _flyRoutine = StartCoroutine(FlyRoutine(count, goldBarTransform, onAllArrived));
    }

    // ── Private ────────────────────────────────────────────────────────────

    private IEnumerator FlyRoutine(int count, Transform goldBarTarget, Action onAllArrived)
    {
        _spawnedCoins.Clear();

        int arrivedCount = 0;
        bool allDispatched = false;

        for (int i = 0; i < count; i++)
        {
            // 1. Spawn tại Vector3.zero (world)
            GameObject coin = FactoryDesignPattern.Instance.CreateGameObject(
                coinObjectId,
                Vector3.zero,
                Quaternion.identity);

            if (coin == null)
            {
                arrivedCount++;
                continue;
            }

            _spawnedCoins.Add(coin);

            // 2. Vị trí tản ngẫu nhiên quanh origin
            Vector3 spreadPos = GetRandomSpreadPosition();

            // 3. Lấy MoveWithCurve và setup
            MoveWithCurve mover = coin.GetComponent<MoveWithCurve>();
            if (mover == null)
            {
                Debug.LogWarning($"[CoinRewardFlyEffect] Coin id={coinObjectId} không có MoveWithCurve!");
                SmartPool.Instance.Despawn(coin);
                arrivedCount++;
                continue;
            }

            // Phase 1: tản ra từ zero → spreadPos (thẳng, nhanh)
            mover.SetPositions(Vector3.zero, spreadPos);
            mover.Play(spreadDuration, () =>
            {
                // Phase 2: từ spreadPos bay về goldBar (có cung)
                Vector3 mid = Vector3.Lerp(spreadPos, goldBarTarget.position, 0.5f) + Vector3.up * flyArcHeight;
                mover.SetArcMidOffset(mid - Vector3.Lerp(spreadPos, goldBarTarget.position, 0.5f));
                mover.SetFollowTarget(spreadPos, goldBarTarget);
                mover.Play(flyDuration, () =>
                {
                    if (coin != null)
                    {
                        coin.SetActive(false);
                    }
                    SmartPool.Instance.Despawn(coin);
                    arrivedCount++;
                    if (allDispatched && arrivedCount >= count)
                        onAllArrived?.Invoke();
                });
            });

            // Stagger nhỏ giữa các coin
            if (staggerDelay > 0f)
                yield return new WaitForSeconds(staggerDelay);
        }

        allDispatched = true;
        _flyRoutine = null;

        // Trường hợp tất cả đã về trước khi flag được set
        if (arrivedCount >= count)
            onAllArrived?.Invoke();
    }

    private Vector3 GetRandomSpreadPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float r = spreadRadius * Mathf.Sqrt(Random.value);
        return new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
    }

    private static Transform GetGoldBarTransform()
    {
        if (UIGameController.Instance == null) return null;
        return UIGameController.Instance.goldBar != null
            ? UIGameController.Instance.goldBar.transform
            : null;
    }
}
