using UnityEngine;

namespace SRVR
{
    public static class RectTransformExtension
    {
        public static void SetAnchoredPosition2D(this RectTransform rectTransform, float x, float y)
        {
            rectTransform.anchoredPosition = new Vector2(x, y);
        }
        public static T Safe<T>(this T obj)
        {
            if (obj is Object @object)
            {
                if (@object)
                    return obj;
            }
            return default ;
        }
    }
}