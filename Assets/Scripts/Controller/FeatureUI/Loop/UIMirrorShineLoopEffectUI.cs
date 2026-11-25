using System;
using JinGroup.Common.Effect;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Controller.Feature
{
    /// <summary>
    /// Hiệu ứng lóe sáng tráng gương rọi vào UI dạng lặp (Loop Effect).
    /// Hỗ trợ SpriteAtlas, 9-slice, RectMask2D, Stencil Mask.
    /// Có thể tự tính range theo RectTransform hoặc cấu hình thủ công.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class UIMirrorShineLoopEffectUI : EffectBaseLoop
    {
        private static readonly int ShineColorId          = Shader.PropertyToID("_ShineColor");
        private static readonly int ShineBrightnessId     = Shader.PropertyToID("_ShineBrightness");
        private static readonly int ShineWidthId          = Shader.PropertyToID("_ShineWidth");
        private static readonly int ShineSoftnessId       = Shader.PropertyToID("_ShineSoftness");
        private static readonly int ShineAngleId          = Shader.PropertyToID("_ShineAngle");
        private static readonly int GlossBoostId          = Shader.PropertyToID("_GlossBoost");
        private static readonly int EnableSecondaryBeamId = Shader.PropertyToID("_EnableSecondaryBeam");
        private static readonly int SecondaryOffsetId     = Shader.PropertyToID("_SecondaryOffset");
        private static readonly int SecondaryWidthId      = Shader.PropertyToID("_SecondaryWidth");
        private static readonly int SecondaryIntensityId  = Shader.PropertyToID("_SecondaryIntensity");
        private static readonly int AutoPlayId            = Shader.PropertyToID("_AutoPlay");
        private static readonly int ManualProgressId      = Shader.PropertyToID("_ManualProgress");
        private static readonly int SweepRangeId          = Shader.PropertyToID("_SweepRange");
        private static readonly int UseUVModeId           = Shader.PropertyToID("_UseUVMode");

        private const string ShaderName = "UI/MirrorShine";
        private const float InvisibleProgress = -999f;

        [Title("Shine Appearance")]
        [ColorUsage(true, true)]
        [LabelText("Shine Color (HDR)")]
        [Tooltip("Màu sắc và độ rực rỡ của tia sáng (hỗ trợ HDR bloom). Mặc định trắng tinh khiết.")]
        public Color shineColor = Color.white;

        [Range(0f, 5f)]
        [LabelText("Brightness")]
        [Tooltip("Cường độ sáng tổng thể của vệt lóe sáng.")]
        public float brightness = 1.8f;

        [Range(1f, 300f)]
        [LabelText("Beam Width (Pixels)")]
        [Tooltip("Độ rộng của vệt sáng chính (tính theo pixel UI).")]
        public float beamWidth = 35f;

        [Range(0.01f, 1f)]
        [LabelText("Edge Softness")]
        [Tooltip("Độ mượt/mềm ở 2 bên rìa tia sáng (càng nhỏ càng sắc nét).")]
        public float softness = 0.45f;

        [Range(-180f, 180f)]
        [LabelText("Angle (Degrees)")]
        [Tooltip("Góc nghiêng của tia sáng (mặc định 35 độ).")]
        public float angle = 35f;

        [Range(0f, 5f)]
        [LabelText("Mirror Gloss Boost")]
        [Tooltip("Tăng cường độ sáng chói ở lõi tâm tia sáng tạo cảm giác tráng gương kim loại.")]
        public float glossBoost = 1.2f;

        [Title("Secondary Accent Beam (Vệt phản chiếu phụ)")]
        [LabelText("Enable Secondary")]
        [Tooltip("Bật thêm 1 vệt sáng mảnh song song chạy kèm (tạo hiệu ứng gương vát cạnh).")]
        public bool enableSecondaryBeam = true;

        [ShowIf("enableSecondaryBeam")]
        [Range(-200f, 200f)]
        [LabelText("Secondary Offset (Pixels)")]
        [Tooltip("Khoảng cách lệch của vệt phụ so với vệt chính.")]
        public float secondaryOffset = -25f;

        [ShowIf("enableSecondaryBeam")]
        [Range(1f, 150f)]
        [LabelText("Secondary Width (Pixels)")]
        [Tooltip("Độ dày của vệt sáng phụ.")]
        public float secondaryWidth = 12f;

        [ShowIf("enableSecondaryBeam")]
        [Range(0f, 2f)]
        [LabelText("Secondary Intensity")]
        [Tooltip("Độ sáng của vệt phụ tương đối so với vệt chính.")]
        public float secondaryIntensity = 0.5f;

        [Title("Sweep Range & Size")]
        [LabelText("Auto Calculate Range")]
        [Tooltip("Tự động tính quãng đường quét tia sáng dựa vào kích thước RectTransform.")]
        public bool autoCalculateRange = true;

        [HideIf("autoCalculateRange")]
        [Range(50f, 2000f)]
        [LabelText("Custom Sweep Range (Pixels)")]
        public float customSweepRange = 350f;

        [Title("Loop Timing")]
        [LabelText("Delay Between Loops (Seconds)")]
        [Tooltip("Thời gian nghỉ (giây) sau khi quét xong 1 lần trước khi quét tiếp.")]
        public float delayBetweenLoops = 2f;

        private Graphic       _graphic;
        private RectTransform _rt;
        private Material      _instancedMaterial;
        private MotionHandle  _motionHandle;
        private MotionHandle  _delayHandle;
        private int           _currentLoop;
        private bool          _isMaterialSetup;

        public Graphic TargetGraphic => _graphic != null ? _graphic : (_graphic = GetComponent<Graphic>());
        public RectTransform RT => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());

        protected override void Awake()
        {
            EnsureMaterialInstance();
            base.Awake();
        }

        private void OnValidate()
        {
            if (_instancedMaterial != null)
            {
                UpdateMaterialStaticProperties();
            }
        }

        private void EnsureMaterialInstance()
        {
            if (_isMaterialSetup && _instancedMaterial != null) return;

            Graphic g = TargetGraphic;
            if (g == null) return;

            Material baseMat = g.material;
            if (baseMat != null && baseMat.shader != null && baseMat.shader.name == ShaderName)
            {
                _instancedMaterial = new Material(baseMat);
            }
            else
            {
                Shader shader = Shader.Find(ShaderName);
                if (shader != null)
                {
                    _instancedMaterial = new Material(shader);
                }
                else
                {
                    Debug.LogWarning($"[UIMirrorShineLoopEffectUI] Không tìm thấy shader '{ShaderName}'!");
                    return;
                }
            }

            _instancedMaterial.name = $"{g.name}_MirrorShine_Instance";
            // Tắt auto play trên shader để script điều khiển chính xác bằng LitMotion
            _instancedMaterial.SetFloat(AutoPlayId, 0f);
            _instancedMaterial.SetFloat(UseUVModeId, 0f);
            g.material = _instancedMaterial;

            UpdateMaterialStaticProperties();
            _isMaterialSetup = true;
        }

        private void UpdateMaterialStaticProperties()
        {
            if (_instancedMaterial == null) return;

            _instancedMaterial.SetColor(ShineColorId, shineColor);
            _instancedMaterial.SetFloat(ShineBrightnessId, brightness);
            _instancedMaterial.SetFloat(ShineWidthId, beamWidth);
            _instancedMaterial.SetFloat(ShineSoftnessId, softness);
            _instancedMaterial.SetFloat(ShineAngleId, angle);
            _instancedMaterial.SetFloat(GlossBoostId, glossBoost);
            _instancedMaterial.SetFloat(EnableSecondaryBeamId, enableSecondaryBeam ? 1f : 0f);
            _instancedMaterial.SetFloat(SecondaryOffsetId, secondaryOffset);
            _instancedMaterial.SetFloat(SecondaryWidthId, secondaryWidth);
            _instancedMaterial.SetFloat(SecondaryIntensityId, secondaryIntensity);

            float sweepRange = customSweepRange;
            if (autoCalculateRange && RT != null)
            {
                Rect rect = RT.rect;
                float diagonal = Mathf.Sqrt(rect.width * rect.width + rect.height * rect.height);
                sweepRange = (diagonal * 0.5f) + beamWidth + Mathf.Abs(secondaryOffset) + 30f;
            }
            _instancedMaterial.SetFloat(SweepRangeId, Mathf.Max(sweepRange, 50f));
        }

        public override void PrepareInitialState()
        {
            EnsureMaterialInstance();
            CancelMotion();
            SetProgress(InvisibleProgress);
        }

        public override void Play()
        {
            EnsureMaterialInstance();
            CancelMotion();
            _currentLoop = 0;
            PlayCycle();
        }

        private void PlayCycle()
        {
            CancelMotion();
            UpdateMaterialStaticProperties();

            if (duration <= 0f)
            {
                SetProgress(1f);
                OnCycleCompleted();
                return;
            }

            SetProgress(0f);
            _motionHandle = LMotion.Create(0f, 1f, duration)
                .WithOnComplete(OnCycleCompleted)
                .Bind(SetProgress);
        }

        private void OnCycleCompleted()
        {
            SetProgress(InvisibleProgress);
            _currentLoop++;

            // Kiểm tra số lần lặp
            if (LoopCount >= 0 && _currentLoop >= LoopCount)
            {
                return;
            }

            // Nghỉ delay trước chu kỳ tiếp theo
            if (delayBetweenLoops > 0f)
            {
                _delayHandle = LMotion.Create(0f, 1f, delayBetweenLoops)
                    .WithOnComplete(PlayCycle)
                    .RunWithoutBinding();
            }
            else
            {
                PlayCycle();
            }
        }

        private void SetProgress(float progress)
        {
            if (_instancedMaterial != null)
            {
                _instancedMaterial.SetFloat(ManualProgressId, progress);
            }
        }

        public override void CancelMotion()
        {
            _motionHandle.TryCancel();
            _delayHandle.TryCancel();
        }

        public override void Stop()
        {
            CancelMotion();
            SetProgress(InvisibleProgress);
        }

        private void OnDestroy()
        {
            CancelMotion();
            if (_instancedMaterial != null)
            {
                if (TargetGraphic != null && TargetGraphic.material == _instancedMaterial)
                {
                    TargetGraphic.material = null;
                }
                DestroyImmediate(_instancedMaterial);
                _instancedMaterial = null;
            }
        }
    }
}
