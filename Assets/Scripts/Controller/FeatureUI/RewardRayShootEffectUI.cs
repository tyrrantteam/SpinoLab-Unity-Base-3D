using System;
using System.Collections;
using System.Collections.Generic;
using Base.Core.Sound;
using JinGroup.Module.Reward;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public class RewardRayShootEffectUI : MonoBehaviour
    {
        [Title("Cài Đặt Vị Trí Xuất Phát")]
        [Tooltip("Vị trí xuất phát (Rương đồ). Nếu để trống sẽ tự tìm RuongDo")]
        [SerializeField] private RectTransform posSpawnItem;

        [Title("Cài Đặt Hướng Bắn & Các Hàng")]
        [LabelText("Số Tia Mỗi Hàng (Cột)")]
        [SerializeField] private int raysPerRow = 3;

        [LabelText("Góc Cách Nhau Giữa Các Tia (Độ)")]
        [SerializeField] private float rayAngleStep = 25f;

        [LabelText("Góc Gốc (90 = Thẳng đứng)")]
        [SerializeField] private float baseAngle = 90f;

        [LabelText("Khoảng Cách Bắn Hàng Đầu (px)")]
        [SerializeField] private float flyDistance = 450f;

        [LabelText("Khoảng Cách Từng Tia (px)")]
        [Tooltip("Tùy chỉnh khoảng cách riêng cho từng tia từ trái qua phải (Tia 1, Tia 2, Tia 3...). Nếu để trống hoặc phần tử nào <= 0 thì sẽ lấy flyDistance mặc định.")]
        [SerializeField] private List<float> customRayDistances = new List<float>();

        [LabelText("Khoảng Cách Giữa Các Hàng (px)")]
        [SerializeField] private float rowSpacing = 120f;

        [LabelText("Khoảng Cách Bắn Tối Thiểu (px)")]
        [SerializeField] private float minFlyDistance = 150f;

        [LabelText("Thời Gian Bay (giây)")]
        [SerializeField] private float flyDuration = 0.6f;

        [LabelText("Ease Bay Vị Trí")]
        [SerializeField] private Ease flyEase = Ease.OutCubic;

        [LabelText("Thời Gian Phóng To Icon (giây)")]
        [SerializeField] private float scalePopDuration = 0.35f;

        [LabelText("Ease Phóng To Icon")]
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        [LabelText("Chờ Rương Rung (giây)")]
        [SerializeField] private float delayBeforeShoot = 2.0f;

        [LabelText("Độ Trễ Giữa Các Lần Bắn (giây)")]
        [SerializeField] private float staggerDelay = 0.1f;

        [LabelText("Thời Gian Ẩn Phần Thưởng (giây)")]
        [SerializeField] private float hideDuration = 0.25f;

        private readonly List<MotionHandle> _handles = new();
        private Coroutine _shootRoutine;
        private List<ModuleRewardController> _currentRewardItems;
        private RectTransform _currentHolderReward;
        private Vector3[] _targetScales;
        private Action _onComplete;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        // ─── Public API ───────────────────────────────────────────────────────────

        public void PlayShootRewards(List<ModuleRewardController> rewardItems, RectTransform holderReward, Action onComplete = null)
        {
            StopAllEffects();
            _currentRewardItems = rewardItems;
            _currentHolderReward = holderReward;
            _onComplete = onComplete;
            _isPlaying = true;

            if (rewardItems != null && rewardItems.Count > 0)
            {
                _targetScales = new Vector3[rewardItems.Count];
                for (int i = 0; i < rewardItems.Count; i++)
                {
                    if (rewardItems[i] != null)
                    {
                        var ls = rewardItems[i].transform.localScale;
                        _targetScales[i] = ls != Vector3.zero ? ls : Vector3.one;
                    }
                }
            }

            _shootRoutine = StartCoroutine(ShootRoutine(rewardItems, holderReward, onComplete));
        }

        public void StopAllEffects()
        {
            if (_shootRoutine != null)
            {
                StopCoroutine(_shootRoutine);
                _shootRoutine = null;
            }
            CancelHandles();
            _isPlaying = false;
            _onComplete = null;

            GetRuongDo()?.Stop();
        }

        /// <summary>
        /// Bỏ qua animation bắn: đặt tất cả item ngay vào vị trí đích, hiện rương đã mở.
        /// </summary>
        public void SkipEffect()
        {
            if (!_isPlaying && _shootRoutine == null)
                return;

            if (_shootRoutine != null)
            {
                StopCoroutine(_shootRoutine);
                _shootRoutine = null;
            }
            CancelHandles();
            GetRuongDo()?.SetOpenedInstant();

            if (_currentRewardItems != null && _currentRewardItems.Count > 0)
            {
                Vector2 startPos = GetOriginPosition(_currentHolderReward);
                int safeRaysPerRow = Mathf.Max(1, raysPerRow);

                for (int i = 0; i < _currentRewardItems.Count; i++)
                {
                    Vector2 targetPos = ComputeTargetPosition(i, startPos, safeRaysPerRow);
                    var reward = _currentRewardItems[i];
                    if (reward == null) continue;

                    var rt = reward.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = targetPos;
                        Vector3 destScale = (_targetScales != null && i < _targetScales.Length && _targetScales[i] != Vector3.zero)
                            ? _targetScales[i]
                            : Vector3.one;
                        rt.localScale = destScale;
                    }
                    reward.gameObject.SetActive(true);
                }

                SoundManager.Instance.PlaySound(SoundType.RewardShowItem);
            }

            _isPlaying = false;
            var cb = _onComplete;
            _onComplete = null;
            cb?.Invoke();
        }

        /// <summary>
        /// Hiệu ứng ẩn toàn bộ phần thưởng và rương đồ (thu nhỏ về Vector3.zero) khi người chơi tap claim.
        /// </summary>
        public void PlayHideRewards(Action onComplete = null)
        {
            if (_currentRewardItems == null || _currentRewardItems.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            CancelHandles();
            if (_shootRoutine != null)
            {
                StopCoroutine(_shootRoutine);
                _shootRoutine = null;
            }

            // Đếm tổng số đối tượng cần ẩn (items + rương nếu đang hiện)
            int total = 0;
            for (int i = 0; i < _currentRewardItems.Count; i++)
            {
                if (_currentRewardItems[i] != null && _currentRewardItems[i].gameObject.activeSelf)
                    total++;
            }

            RuongDo ruongDo = GetRuongDo();
            bool ruongDoVisible = ruongDo != null && ruongDo.gameObject.activeSelf && ruongDo.transform.localScale != Vector3.zero;
            if (ruongDoVisible) total++;

            if (total == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int completed = 0;
            void OnOneDone()
            {
                completed++;
                if (completed >= total)
                    onComplete?.Invoke();
            }

            // Thu nhỏ rương đồ cùng với items
            if (ruongDoVisible)
            {
                var ruongRT = ruongDo.GetComponent<RectTransform>();
                if (ruongRT != null)
                {
                    MotionHandle ruongHandle = LMotion.Create(ruongRT.localScale, Vector3.zero, hideDuration)
                                                      .WithEase(Ease.InBack)
                                                      .WithOnComplete(() =>
                                                      {
                                                          ruongDo.Stop();
                                                          OnOneDone();
                                                      })
                                                      .BindToLocalScale(ruongRT);
                    _handles.Add(ruongHandle);
                }
                else
                {
                    ruongDo.Stop();
                    OnOneDone();
                }
            }

            // Thu nhỏ từng item
            for (int i = 0; i < _currentRewardItems.Count; i++)
            {
                var reward = _currentRewardItems[i];
                if (reward == null || !reward.gameObject.activeSelf) continue;

                var rt = reward.GetComponent<RectTransform>();
                if (rt != null)
                {
                    var capturedReward = reward;
                    MotionHandle handle = LMotion.Create(rt.localScale, Vector3.zero, hideDuration)
                                                 .WithEase(Ease.InBack)
                                                 .WithOnComplete(() =>
                                                 {
                                                     capturedReward.gameObject.SetActive(false);
                                                     OnOneDone();
                                                 })
                                                 .BindToLocalScale(rt);
                    _handles.Add(handle);
                }
                else
                {
                    reward.gameObject.SetActive(false);
                    OnOneDone();
                }
            }
        }

        // ─── Private / Coroutines ─────────────────────────────────────────────────

        private IEnumerator ShootRoutine(List<ModuleRewardController> rewardItems, RectTransform holderReward, Action onComplete)
        {
            if (rewardItems == null || rewardItems.Count == 0)
            {
                _isPlaying = false;
                onComplete?.Invoke();
                yield break;
            }

            RuongDo ruongDo = GetRuongDo();
            if (posSpawnItem == null && ruongDo != null)
                posSpawnItem = ruongDo.GetComponent<RectTransform>();

            ruongDo?.Play();

            Vector2 startPos = GetOriginPosition(holderReward);
            int totalCount = rewardItems.Count;
            int safeRaysPerRow = Mathf.Max(1, raysPerRow);

            if (_targetScales == null || _targetScales.Length != totalCount)
                _targetScales = new Vector3[totalCount];

            for (int i = 0; i < totalCount; i++)
            {
                if (rewardItems[i] != null)
                {
                    var rt = rewardItems[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        if (_targetScales[i] == Vector3.zero)
                            _targetScales[i] = rt.localScale != Vector3.zero ? rt.localScale : Vector3.one;
                        rt.anchoredPosition = startPos;
                        rt.localScale = Vector3.zero;
                    }
                    rewardItems[i].gameObject.SetActive(false);
                }
            }

            // 1. Chờ rương đồ rung lắc xong
            if (delayBeforeShoot > 0f)
                yield return new WaitForSeconds(delayBeforeShoot);
            else
                yield return null;

            // 2. RƯƠNG CO BÓP (nhún bẹt tích lực rồi vươn cao chuẩn bị bắn)
            if (ruongDo != null)
            {
                yield return StartCoroutine(ruongDo.Shrank());
                ruongDo.ReboundToOriginal();
            }

            // 3. BẮN TOÀN BỘ ITEMS THEO CÁC TIA (giữa trước, rồi luân phiên trái phải)
            for (int i = 0; i < totalCount; i++)
            {
                Vector2 targetPos = ComputeTargetPosition(i, startPos, safeRaysPerRow);

                var reward = rewardItems[i];
                if (reward != null)
                {
                    var rt = reward.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = startPos;
                        rt.localScale = Vector3.zero;
                        reward.gameObject.SetActive(true);

                        SoundManager.Instance.PlaySound(SoundType.RewardShowItem);

                        MotionHandle posHandle = LMotion.Create(startPos, targetPos, flyDuration)
                                                       .WithEase(flyEase)
                                                       .BindToAnchoredPosition(rt);
                        _handles.Add(posHandle);

                        MotionHandle scaleHandle = LMotion.Create(Vector3.zero, _targetScales[i], scalePopDuration)
                                                          .WithEase(scaleEase)
                                                          .BindToLocalScale(rt);
                        _handles.Add(scaleHandle);
                    }
                }

                if (staggerDelay > 0f)
                    yield return new WaitForSeconds(staggerDelay);
            }

            float remainingWait = Mathf.Max(flyDuration, scalePopDuration);
            if (remainingWait > 0f)
                yield return new WaitForSeconds(remainingWait);

            _shootRoutine = null;
            _isPlaying = false;
            var cb = _onComplete;
            _onComplete = null;
            cb?.Invoke();
        }
        /// Tính toán khoảng cách bay của một tia dựa trên index cột và hàng.
        public float CalculateRayDistance(int colIndex, int rowIndex)
        {
            float baseDist = flyDistance;
            if (customRayDistances != null && colIndex < customRayDistances.Count && customRayDistances[colIndex] > 0f)
            {
                baseDist = customRayDistances[colIndex];
            }
            return Mathf.Max(minFlyDistance, baseDist - (rowIndex * rowSpacing));
        }

        private Vector2 GetOriginPosition(RectTransform holderRT)
        {
            if (posSpawnItem != null && holderRT != null)
            {
                Vector3 worldPos = posSpawnItem.position;
                Canvas canvas = holderRT.GetComponentInParent<Canvas>();
                Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;

                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(holderRT, screenPoint, cam, out Vector2 localPoint);
                return localPoint;
            }
            return Vector2.zero;
        }

        private void CancelHandles()
        {
            for (int i = 0; i < _handles.Count; i++)
                _handles[i].TryCancel();
            _handles.Clear();
        }

        private RuongDo GetRuongDo()
        {
            RuongDo rd = posSpawnItem != null ? posSpawnItem.GetComponent<RuongDo>() : null;
            return rd != null ? rd : GetComponentInChildren<RuongDo>();
        }

        /// <summary>
        /// Thứ tự cột trong một hàng: giữa trước, rồi luân phiên trái / phải.
        /// </summary>
        private static int GetShootColumnIndex(int orderInRow, int columnCount)
        {
            int center = (columnCount - 1) / 2;
            if (orderInRow == 0)
                return center;

            int pair = (orderInRow + 1) / 2;
            return (orderInRow % 2 == 1) ? center - pair : center + pair;
        }

        /// <summary>
        /// Tính vị trí đích (anchoredPosition) cho item tại index <paramref name="index"/> trong lưới tia bắn.
        /// </summary>
        private Vector2 ComputeTargetPosition(int index, Vector2 startPos, int safeRaysPerRow)
        {
            int rowIndex = index / safeRaysPerRow;
            int orderInRow = index % safeRaysPerRow;
            int colIndex = GetShootColumnIndex(orderInRow, safeRaysPerRow);
            float startAngle = baseAngle + ((safeRaysPerRow - 1) * 0.5f) * rayAngleStep;
            float angleDeg = startAngle - (colIndex * rayAngleStep);
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            float distance = CalculateRayDistance(colIndex, rowIndex);
            return startPos + dir * distance;
        }

        private void OnDisable()
        {
            StopAllEffects();
        }

        private void OnDestroy()
        {
            StopAllEffects();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = posSpawnItem != null ? posSpawnItem.position : transform.position;
            float scale = transform.lossyScale.x > 0 ? transform.lossyScale.x : 1f;
            int safeRaysPerRow = Mathf.Max(1, raysPerRow);
            float startAngle = baseAngle + ((safeRaysPerRow - 1) * 0.5f) * rayAngleStep;
            int previewRows = 3;

            Color[] rowColors = new Color[] { Color.yellow, Color.cyan, Color.green, Color.magenta };

            for (int r = 0; r < previewRows; r++)
            {
                Gizmos.color = rowColors[r % rowColors.Length];

                for (int c = 0; c < safeRaysPerRow; c++)
                {
                    float dist = CalculateRayDistance(c, r) * scale;
                    float angleDeg = startAngle - (c * rayAngleStep);
                    float rad = angleDeg * Mathf.Deg2Rad;
                    Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
                    Vector3 target = origin + dir * dist;

                    Gizmos.DrawLine(origin, target);
                    Gizmos.DrawWireSphere(target, 22f * scale);
                }
            }
        }
#endif
    }
}
