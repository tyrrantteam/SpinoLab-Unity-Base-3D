using System;
using System.Collections.Generic;
using Core.DesignPattern.Factory;
using Core.Pool;
using JinGroup.Base.LoadData;
using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace JinGroup.Controller.Feature
{
    public class SpawnCircleEffectUI : EffectBase
    {
        [LabelText("Spawn Parent")] public RectTransform spawnParent;

        [LabelText("Count")] public int count = 5;

        [LabelText("Radius")] public float radius = 100f;

        [LabelText("Spread Duration")] public float spreadDuration = 0.4f;

        [LabelText("Spread Ease")] public Ease spreadEase = Ease.OutBack;

        // [LabelText("Destination")] public RectTransform destination;

        private readonly List<RectTransform> _holderitemSpawned = new();
        private readonly List<MotionHandle>  _handles      = new();

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        [Button]
        public void OnSpawnBoosterUI(BoosterType type, Action onComplete = null, int spawnCount = 1)
        {
            var destination = BoosterManager.Instance.GetBoosterButton(type);
            if (destination == null)
            {
                onComplete?.Invoke();
                return;
            }

            destination.SetReceivingBooster(true);

            Action wrappedOnComplete = () =>
            {
                try
                {
                    onComplete?.Invoke();
                }
                finally
                {
                    if (destination != null)
                        destination.SetReceivingBooster(false);
                }
            };

            var data = !LoadResourceController.IsNull ? LoadResourceController.Instance.DataBoosterController() : null;
            Sprite boosterIcon = null;
            if (data != null)
            {
                var boosterData = data.GetDataBoosterByType(type);
                boosterIcon = boosterData.imgIconBooster != null ? boosterData.imgIconBooster : boosterData.imgMechanic;
            }

            PlayBoosterIcons(boosterIcon, spawnCount, destination.GetComponent<RectTransform>(), wrappedOnComplete);
        }

        private void PlayBoosterIcons(Sprite boosterIcon, int number, RectTransform destination, Action onComplete = null)
        {
            if (spawnParent == null)
            {
                Canvas canvas = destination != null ? destination.GetComponentInParent<Canvas>() : GetComponentInParent<Canvas>();
                if (canvas != null)
                    spawnParent = canvas.GetComponent<RectTransform>();
            }

            if (spawnParent == null || boosterIcon == null)
            {
                onComplete?.Invoke();
                return;
            }

            number = Mathf.Max(1, number);
            Vector2 center = spawnParent.rect.center;
            int flyCompletedCount = 0;

            for (int i = 0; i < number; i++)
            {
                var objectToView = FactoryDesignPattern.Instance.CreateObjectUI(ItemString.BOOSTER_UI, spawnParent);
                if (objectToView == null)
                {
                    flyCompletedCount++;
                    if (flyCompletedCount >= number)
                        onComplete?.Invoke();
                    continue;
                }

                objectToView.transform.SetAsLastSibling();
                var img = objectToView.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = boosterIcon;
                    var c = img.color;
                    c.a = 1f;
                    img.color = c;
                }

                Vector2 targetPos = GetRandomPositionInCircle(center);
                RectTransform rt = objectToView.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition = center;
                    _holderitemSpawned.Add(rt);
                }

                var effect = objectToView.GetComponent<MoveToTargetEffectUI>();

                MotionHandle handle = LMotion.Create(center, targetPos, spreadDuration)
                                             .WithEase(spreadEase)
                                             .WithOnComplete(() =>
                                             {
                                                 if (objectToView == null) return;
                                                 if (destination != null && effect != null)
                                                 {
                                                     effect.SetDestination(destination, () =>
                                                     {
                                                         flyCompletedCount++;
                                                         if (flyCompletedCount >= number)
                                                             onComplete?.Invoke();
                                                     });
                                                     return;
                                                 }

                                                 flyCompletedCount++;
                                                 if (flyCompletedCount >= number)
                                                     onComplete?.Invoke();
                                             })
                                             .BindToAnchoredPosition(rt);
                _handles.Add(handle);
            }
        }

        public void Play(GameObject item, int number, RectTransform destination, Action onComplete = null)
        {
            if (spawnParent == null) return;

            Vector2 center = spawnParent.rect.center;
            int flyCompletedCount = 0;

            for (int i = 0; i < number; i++)
            {
                Vector2 targetPos = GetRandomPositionInCircle(center);
                RectTransform rt = item.GetComponent<RectTransform>();
                var effect = item.GetComponent<MoveToTargetEffectUI>();
                rt.anchoredPosition = center;
                _holderitemSpawned.Add(rt);
                MotionHandle handle = LMotion.Create(center, targetPos, spreadDuration)
                                             .WithEase(spreadEase)
                                             .WithOnComplete(() =>
                                             {
                                                 if (item == null) return;
                                                 if (destination != null)
                                                 {
                                                     effect.SetDestination(destination, () =>
                                                     {
                                                         flyCompletedCount++;
                                                         if (flyCompletedCount >= number)
                                                             onComplete?.Invoke();
                                                     });
                                                     return;
                                                 }

                                                 flyCompletedCount++;
                                                 if (flyCompletedCount >= number)
                                                     onComplete?.Invoke();
                                             })
                                             .BindToAnchoredPosition(rt);
                _handles.Add(handle);
            }
        }

        public override void Play()
        {
        }

        public override void CancelMotion()
        {
            CancelHandles();
            ClearItems();
            if (!BoosterManager.IsNull)
                BoosterManager.Instance.ResetAllReceivingBoosterState();
        }

        public override void Stop()
        {
            CancelHandles();
            ClearItems();
            if (!BoosterManager.IsNull)
                BoosterManager.Instance.ResetAllReceivingBoosterState();
        }

        private void CancelHandles()
        {
            for (int i = 0; i < _handles.Count; i++)
                _handles[i].TryCancel();
            _handles.Clear();
        }

        private void ClearItems()
        {
            CancelHandles();
            for (int i = 0; i < _holderitemSpawned.Count; i++)
            {
                if (_holderitemSpawned[i] != null)
                    SmartPool.Instance.Despawn(_holderitemSpawned[i].gameObject);
            }

            _holderitemSpawned.Clear();
        }

        private Vector2 GetRandomPositionInCircle(Vector2 center)
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float r = radius * Mathf.Sqrt(Random.value);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            RectTransform rt = spawnParent != null ? spawnParent : GetComponent<RectTransform>();
            if (rt == null) return;

            Vector3 worldCenter = rt.TransformPoint(rt.rect.center);
            float worldRadius = radius * rt.lossyScale.x;

            Gizmos.color = new Color(1f, 0.85f, 0f, 0.9f);

            const int segments = 64;
            float step = 2f * Mathf.PI / segments;
            Vector3 prev = worldCenter + new Vector3(worldRadius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float a = step * i;
                Vector3 next = worldCenter + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * worldRadius;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }

            Gizmos.color = new Color(1f, 0.85f, 0f, 0.35f);
            Gizmos.DrawSphere(worldCenter, worldRadius * 0.04f);
        }
#endif
    }
}