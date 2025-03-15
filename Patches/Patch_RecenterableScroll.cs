using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(RecenterableScroll))]
    public static class Patch_RecenterableScroll
    {
        [HarmonyPrefix, HarmonyPatch(typeof(RecenterableScroll), nameof(RecenterableScroll.ScrollToItem))]
        public static bool ScrollToItem(RecenterableScroll __instance, RectTransform itemTransform)
        {
            var viewport = __instance.scroll.viewport ?? (RectTransform)__instance.scroll.content.parent ;
            var contentArea = __instance.scroll.content.rect;
            var targetArea = itemTransform.rect;
            var viewportArea = viewport.rect;
            var localMin = (Vector2)__instance.scroll.content.InverseTransformPoint(itemTransform.TransformPoint(targetArea.min)) - contentArea.min;
            var localMax = (Vector2)__instance.scroll.content.InverseTransformPoint(itemTransform.TransformPoint(targetArea.max)) - contentArea.min;
            var viewMin = (Vector2)__instance.scroll.content.InverseTransformPoint(viewport.TransformPoint(viewportArea.min)) - contentArea.min;
            var viewMax = (Vector2)__instance.scroll.content.InverseTransformPoint(viewport.TransformPoint(viewportArea.max)) - contentArea.min;
            var viewportSize = viewMax - viewMin;
            var contentSize = contentArea.size;
            for (int i = 0; i <= 1; i++)
                if (i == 0 ? __instance.scroll.horizontal : __instance.scroll.vertical)
                {
                    float result;
                    if (localMin[i] < viewMin[i])
                        result = localMin[i] / (contentSize[i] - viewportSize[i]);
                    else if (localMax[i] > viewMax[i])
                        result = (localMax[i] - viewportSize[i]) / (contentSize[i] - viewportSize[i]);
                    else
                        continue;
                    if (i == 0)
                        __instance.scroll.horizontalNormalizedPosition = Mathf.Clamp01(result);
                    else
                        __instance.scroll.verticalNormalizedPosition = Mathf.Clamp01(result);
                }
            return false;
        }
    }
}