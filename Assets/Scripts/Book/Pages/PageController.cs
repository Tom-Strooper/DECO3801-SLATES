using UnityEngine;

namespace Slates.Book.Pages
{
    [RequireComponent(typeof(RectTransform))]
    public class PageController : MonoBehaviour
    {
        private const float PageZOffset = 0.01f;

        public virtual void Initialise()
        {
            RectTransform rect = GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            rect.transform.localPosition = -Vector3.forward * PageZOffset;
        }
    }
}
