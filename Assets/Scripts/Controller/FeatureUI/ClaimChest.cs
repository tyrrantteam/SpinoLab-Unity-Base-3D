using System;
using System.Collections;
using JinGroup.Base.LoadData;
using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JinGroup.Controller.Feature
{
    public class ClaimChest : EffectBase
    {
        public Image spriteChest;

        [LabelText("Sprite Rương Mở (Group 1)")] [SerializeField]
        private Sprite openChestSprite;

        private Sprite _initialSprite;

        [Title("Cài Đặt Rung Lắc Ban Đầu")] [LabelText("Thời Gian Rung (giây)")] [SerializeField]
        private float duration = 2.0f;

        [LabelText("Góc Lắc (Độ)")] [SerializeField]
        private float shakeAngle = 12.0f;

        [LabelText("Tần Số Lắc (Vibrato)")] [SerializeField]
        private int frequency = 14;

        [LabelText("Độ Đàn Hồi (Elasticity)"), Range(0f, 1f)] [SerializeField]
        private float elasticity = 0.8f;

        [Title("Anim Nhún Bắn Item (Squash & Stretch)")] [LabelText("Scale Nhún Bẹt (Tăng X, Giảm Y)")] [SerializeField]
        private Vector3 squashScale = new Vector3(1.2f, 0.82f, 1f);

        [LabelText("Scale Bóp Vọt (Giảm X, Tăng Y)")] [SerializeField]
        private Vector3 stretchScale = new Vector3(0.85f, 1.2f, 1f);

        [LabelText("Thời Gian Nhún Bẹt (giây)")] [SerializeField]
        private float squashDuration = 0.2f;

        [LabelText("Thời Gian Bóp Bắn (giây)")] [SerializeField]
        private float stretchDuration = 0.15f;

        [LabelText("Thời Gian Hồi Vị (giây)")] [SerializeField]
        private float recoverDuration = 0.35f;

        [Title("Sự Kiện")] [LabelText("Khi Rung Xong")]
        public UnityEvent onShakeCompleted;

        public event Action OnCompleted;

        [SerializeField] private GameObject spreadVFX;

        private Vector3      _initialLocalEulerAngles;
        private Vector3      _initialScale;
        private MotionHandle _rotHandle;
        private MotionHandle _scaleHandle;
        private Coroutine    _recoilRoutine;

        private void Awake()
        {
            _initialLocalEulerAngles = transform.localEulerAngles;
            _initialScale            = transform.localScale;
            if (spriteChest == null)
            {
                spriteChest = GetComponent<Image>();
            }

            if (spriteChest != null)
            {
                spriteChest.SetNativeSize();
                _initialSprite            = spriteChest.sprite;
                spriteChest.raycastTarget = false;
            }
        }

        /// <summary>
        /// Bỏ qua animation rung lắc/nhún và đưa rương về trạng thái mở ngay lập tức.
        /// </summary>
        public void SetOpenedInstant()
        {
            _rotHandle.TryCancel();
            _scaleHandle.TryCancel();
            if (_recoilRoutine != null)
            {
                StopCoroutine(_recoilRoutine);
                _recoilRoutine = null;
            }

            ResetRotation();
            ResetScale();

            if (spriteChest != null && openChestSprite != null)
            {
                if (spreadVFX != null)
                    spreadVFX.gameObject.SetActive(true);
                spriteChest.sprite = openChestSprite;
                spriteChest.SetNativeSize();
            }
        }

        [Button("Test Rung Lắc", ButtonSizes.Medium), GUIColor(0.3f, 0.8f, 0.3f)]
        public override void Play()
        {
            Stop();
            _initialLocalEulerAngles = transform.localEulerAngles;
            _initialScale            = transform.localScale;
            ResetSprite();

            float dampingRatio = 1f - elasticity;

            // Rung lắc xoay góc nghiêng Z sang 2 bên (giữ nguyên 100% vị trí, không di chuyển)
            _rotHandle = LMotion.Punch.Create(Vector3.zero, new Vector3(0f, 0f, shakeAngle), duration)
                                .WithFrequency(frequency)
                                .WithDampingRatio(dampingRatio)
                                .WithOnComplete(() =>
                                {
                                    ResetRotation();
                                    OnCompleted?.Invoke();
                                    onShakeCompleted?.Invoke();
                                })
                                .BindToLocalEulerAngles(transform);
        }

        /// <summary>
        /// Nhún bẹt ngang tích lực rồi bóp X tăng Y vươn cao chuẩn bị bắn item ra (mượt mà, tự nhiên).
        /// </summary>
        public IEnumerator Shrank()
        {
            if (spriteChest != null && openChestSprite != null)
            {
                spreadVFX.gameObject.SetActive(true);
                spriteChest.sprite = openChestSprite;
                spriteChest.SetNativeSize();
                HapticManager.Instance?.PlayHapticMediumImpact();
            }

            Vector3 baseScale = _initialScale == Vector3.zero ? Vector3.one : _initialScale;
            Vector3 targetSquash = new Vector3(baseScale.x * squashScale.x, baseScale.y * squashScale.y, baseScale.z);
            Vector3 targetStretch = new Vector3(baseScale.x * stretchScale.x, baseScale.y * stretchScale.y, baseScale.z);

            // 1. Nhún bẹt ngang ra tích lực (chuyển động êm ái với Ease.InOutQuad)
            _scaleHandle.TryCancel();
            _scaleHandle = LMotion.Create(transform.localScale, targetSquash, squashDuration)
                                  .WithEase(Ease.InOutQuad)
                                  .BindToLocalScale(transform);
            yield return new WaitForSeconds(squashDuration);

            // 2. Bóp X lại, tăng Y (bắn vọt lên với quán tính mượt Ease.OutQuad) và GIỮ nguyên trạng thái này
            _scaleHandle.TryCancel();
            _scaleHandle = LMotion.Create(transform.localScale, targetStretch, stretchDuration)
                                  .WithEase(Ease.OutQuad)
                                  .BindToLocalScale(transform);
            yield return new WaitForSeconds(stretchDuration);
        }

        /// <summary>
        /// Đàn hồi nảy nhẹ nhàng về kích thước chuẩn ban đầu sau khi đã bắn xong toàn bộ item.
        /// </summary>
        public void ReboundToOriginal()
        {
            Vector3 baseScale = _initialScale == Vector3.zero ? Vector3.one : _initialScale;
            _scaleHandle.TryCancel();
            _scaleHandle = LMotion.Create(transform.localScale, baseScale, recoverDuration)
                                  .WithEase(Ease.OutBack)
                                  .BindToLocalScale(transform);
        }

        /// <summary>
        /// Test chạy toàn bộ chuỗi: Co bóp -> Chờ 0.5s -> Hồi vị
        /// </summary>
        [Button("Test Co Bóp & Hồi Vị", ButtonSizes.Medium), GUIColor(0.2f, 0.7f, 1f)]
        public void PlayShootRecoil()
        {
            if (_recoilRoutine != null)
                StopCoroutine(_recoilRoutine);

            _recoilRoutine = StartCoroutine(TestSequence());
        }

        private IEnumerator TestSequence()
        {
            yield return Shrank();
            ReboundToOriginal();
            _recoilRoutine = null;
        }

        public override void Stop()
        {
            _rotHandle.TryCancel();
            _scaleHandle.TryCancel();
            if (_recoilRoutine != null)
            {
                StopCoroutine(_recoilRoutine);
                _recoilRoutine = null;
            }

            ResetRotation();
            ResetScale();
            ResetSprite();
            if (spreadVFX != null)
                spreadVFX.gameObject.SetActive(false);
        }

        private void ResetRotation()
        {
            transform.localEulerAngles = _initialLocalEulerAngles;
        }

        private void ResetScale()
        {
            if (_initialScale != Vector3.zero)
                transform.localScale = _initialScale;
        }

        private void ResetSprite()
        {
            if (spriteChest != null && _initialSprite != null)
            {
                spriteChest.sprite = _initialSprite;
            }
        }
    }
}