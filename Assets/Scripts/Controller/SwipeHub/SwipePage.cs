using UnityEngine;

namespace JinGroup.UI.SwipeHub
{
    public enum SwipePageKind
    {
        Shop = 0,
        Home = 1,
        Team = 2,
    }

    /// <summary>
    /// One full-screen page in the swipe hub strip. Pages stay active so neighbors stay visible while dragging.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SwipePage : MonoBehaviour
    {
        [SerializeField] private SwipePageKind pageKind = SwipePageKind.Home;

        public SwipePageKind PageKind => pageKind;
        public RectTransform Rect { get; private set; }

        private void Awake()
        {
            CacheRect();
        }

        public void CacheRect()
        {
            if (Rect == null)
                Rect = transform as RectTransform;
        }

        public void SetPageKind(SwipePageKind kind)
        {
            pageKind = kind;
        }
    }
}
