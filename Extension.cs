using UnityEngine;

namespace SRVR
{
    public static class RectTransformExtension
    {
        public static void SetAnchoredPosition2D(this RectTransform rectTransform, float x, float y)
        {
            rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
}