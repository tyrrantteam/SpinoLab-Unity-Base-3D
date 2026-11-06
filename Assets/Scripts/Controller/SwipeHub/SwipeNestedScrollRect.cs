using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JinGroup.UI.SwipeHub
{
    /// <summary>
    /// Vertical shop list that yields horizontal drags to <see cref="SwipePageHub"/>.
    /// </summary>
    public class SwipeNestedScrollRect : ScrollRect
    {
        [SerializeField] private SwipePageHub hub;
        [SerializeField] private float directionLockPixels = 15f;

        private bool _directionLocked;
        private bool _routeToParent;

        protected override void Awake()
        {
            base.Awake();
            if (hub == null)
                hub = GetComponentInParent<SwipePageHub>();
        }

        public void SetHub(SwipePageHub pageHub)
        {
            hub = pageHub;
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (hub != null)
                hub.OnInitializePotentialDrag(eventData);
            else
                ForwardToParent(eventData, ExecuteEvents.initializePotentialDrag);

            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _directionLocked = false;
            _routeToParent = false;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!LockDirectionIfNeeded(eventData))
                return;

            if (_routeToParent)
                ForwardDrag(eventData);
            else
                base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_directionLocked)
            {
                if (_routeToParent)
                    ForwardEndDrag(eventData);
                else
                    base.OnEndDrag(eventData);
            }

            _directionLocked = false;
            _routeToParent = false;
        }

        private bool LockDirectionIfNeeded(PointerEventData eventData)
        {
            if (_directionLocked)
                return true;

            var delta = eventData.position - eventData.pressPosition;
            if (delta.sqrMagnitude < directionLockPixels * directionLockPixels)
                return false;

            _directionLocked = true;
            _routeToParent = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);

            if (_routeToParent)
                ForwardBeginDrag(eventData);
            else
                base.OnBeginDrag(eventData);

            return true;
        }

        private void ForwardBeginDrag(PointerEventData eventData)
        {
            if (hub != null)
                hub.OnBeginDrag(eventData);
            else
                ForwardToParent(eventData, ExecuteEvents.beginDragHandler);
        }

        private void ForwardDrag(PointerEventData eventData)
        {
            if (hub != null)
                hub.OnDrag(eventData);
            else
                ForwardToParent(eventData, ExecuteEvents.dragHandler);
        }

        private void ForwardEndDrag(PointerEventData eventData)
        {
            if (hub != null)
                hub.OnEndDrag(eventData);
            else
                ForwardToParent(eventData, ExecuteEvents.endDragHandler);
        }

        private void ForwardToParent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> function)
            where T : IEventSystemHandler
        {
            var parent = transform.parent;
            if (parent != null)
                ExecuteEvents.ExecuteHierarchy(parent.gameObject, eventData, function);
        }
    }
}
