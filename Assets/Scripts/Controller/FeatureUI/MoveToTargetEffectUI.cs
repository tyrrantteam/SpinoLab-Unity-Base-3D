using System;
using Core.Pool;
using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public class MoveToTargetEffectUI : EffectBase
    {
        [LabelText("Delay")] public float delay = 0f;

        [LabelText("Duration")] public float duration = 0.5f;

        [LabelText("Move Curve")] public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [LabelText("Arc Height")] public float arcHeight = 120f;

        [LabelText("Arc Curve")] public AnimationCurve arcCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 1f),
            new Keyframe(1f, 0f));

        [LabelText("Disable On Complete")] public bool disableOnComplete = true;

        private RectTransform _rt;
        private MotionHandle  _handle;
        private Vector2       _destination;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        public void SetDestination(RectTransform uiTarget, Action onComplete = null)
        {
            RectTransform parent = RT.parent as RectTransform;
            if (parent == null)
            {
                _destination = uiTarget.anchoredPosition;
            }
            else
            {
                Vector3 worldPos = uiTarget.TransformPoint(Vector3.zero);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent,
                    RectTransformUtility.WorldToScreenPoint(null, worldPos),
                    null,
                    out _destination);
            }

            Play(onComplete);
        }

        public void SetDestination(Vector3 worldPosition, Action onComplete = null)
        {
            RectTransform parent = RT.parent as RectTransform;
            if (parent == null)
            {
                _destination = worldPosition;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent,
                    RectTransformUtility.WorldToScreenPoint(null, worldPosition),
                    null,
                    out _destination);
            }

            Play(onComplete);
        }

        public override void Play()
        {
            Play(null);
        }

        private void Play(Action onComplete)
        {
            _handle.TryCancel();

            RectTransform rt = RT;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 destination = _destination;

            _handle = LMotion.Create(0f, 1f, duration)
                             .WithDelay(delay)
                             .WithOnComplete(() =>
                             {
                                 onComplete?.Invoke();
                                 if (disableOnComplete && this != null)
                                     SmartPool.Instance.Despawn(gameObject);
                             })
                             .Bind(t =>
                             {
                                 float progress = moveCurve.Evaluate(t);
                                 Vector2 pos = Vector2.Lerp(startPos, destination, progress);
                                 pos.y += arcCurve.Evaluate(t) * arcHeight;
                                 rt.anchoredPosition = pos;
                             });
        }

        public override void Stop()
        {
            _handle.TryCancel();
        }

        private RectTransform RT => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());
    }
}