using JinGroup.Common.Effect;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Hiệu ứng xoay 3D liên tục cho đồng xu / vật thể.
/// Mỗi trục X / Y / Z có thể bật/tắt riêng và cấu hình tốc độ độc lập (độ/giây).
/// Dùng LitMotion loop vô hạn — không tốn Update().
/// </summary>
public class RotateThreeDEfffect : EffectBase
{
    // ── Trục X ─────────────────────────────────────────────────────────────
    [FoldoutGroup("Rotate X")]
    [LabelText("Enable")]
    [SerializeField] private bool rotateX = false;

    [FoldoutGroup("Rotate X")]
    [LabelText("Speed (°/s)")]
    [EnableIf("rotateX")]
    [SerializeField] private float speedX = 0f;

    // ── Trục Y ─────────────────────────────────────────────────────────────
    [FoldoutGroup("Rotate Y")]
    [LabelText("Enable")]
    [SerializeField] private bool rotateY = true;

    [FoldoutGroup("Rotate Y")]
    [LabelText("Speed (°/s)")]
    [EnableIf("rotateY")]
    [SerializeField] private float speedY = 180f;

    // ── Trục Z ─────────────────────────────────────────────────────────────
    [FoldoutGroup("Rotate Z")]
    [LabelText("Enable")]
    [SerializeField] private bool rotateZ = false;

    [FoldoutGroup("Rotate Z")]
    [LabelText("Speed (°/s)")]
    [EnableIf("rotateZ")]
    [SerializeField] private float speedZ = 0f;

    // ── Tuỳ chọn chung ─────────────────────────────────────────────────────
    [BoxGroup("Options")]
    [LabelText("Random Start Angle")]
    [Tooltip("Bắt đầu từ góc ngẫu nhiên thay vì rotation hiện tại.")]
    [SerializeField] private bool randomStartAngle = false;

    // ── Runtime ────────────────────────────────────────────────────────────
    private MotionHandle _handleX;
    private MotionHandle _handleY;
    private MotionHandle _handleZ;

    private Quaternion _baseRotation;

    protected void Awake()
    {
        _baseRotation = transform.localRotation;
    }

    [Button("▶  Play")]
    public override void Play()
    {
        StopHandles();
        _baseRotation = transform.localRotation;

        Vector3 startEuler = randomStartAngle
            ? new Vector3(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f))
            : transform.localEulerAngles;

        transform.localEulerAngles = startEuler;

        _handleX = BuildAxisHandle(rotateX, speedX, startEuler.x, Axis.X);
        _handleY = BuildAxisHandle(rotateY, speedY, startEuler.y, Axis.Y);
        _handleZ = BuildAxisHandle(rotateZ, speedZ, startEuler.z, Axis.Z);
    }

    [Button("■  Stop")]
    public override void Stop()
    {
        StopHandles();
        transform.localRotation = _baseRotation;
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private enum Axis { X, Y, Z }

    /// <summary>Tạo một LitMotion handle xoay vô hạn cho một trục.</summary>
    private MotionHandle BuildAxisHandle(bool enabled, float speed, float startAngle, Axis axis)
    {
        if (!enabled || Mathf.Approximately(speed, 0f))
            return default;

        // Một vòng 360° mất bao nhiêu giây?
        float duration = 360f / Mathf.Abs(speed);
        float endAngle = startAngle + Mathf.Sign(speed) * 360f;

        return LMotion
            .Create(startAngle, endAngle, duration)
            .WithLoops(-1, LoopType.Restart)   // lặp vô hạn
            .Bind(angle =>
            {
                Vector3 e = transform.localEulerAngles;
                switch (axis)
                {
                    case Axis.X: transform.localEulerAngles = new Vector3(angle, e.y, e.z); break;
                    case Axis.Y: transform.localEulerAngles = new Vector3(e.x, angle, e.z); break;
                    case Axis.Z: transform.localEulerAngles = new Vector3(e.x, e.y, angle); break;
                }
            });
    }

    private void StopHandles()
    {
        _handleX.TryCancel();
        _handleY.TryCancel();
        _handleZ.TryCancel();
    }
}
