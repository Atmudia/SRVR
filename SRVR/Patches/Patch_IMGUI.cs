using HarmonyLib;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(GUILayoutUtility), "Begin")]
    internal static class Patch_IMGUI
    {
        public static bool Prefix(GUILayoutUtility __instance, int instanceID)
        {
            GUILayoutUtility.LayoutCache layoutCache = GUILayoutUtility.SelectIDList(instanceID, false);
            if (Event.current.type == EventType.Layout)
            {
                if (layoutCache.topLevel == null)
                    layoutCache.topLevel = new GUILayoutGroup();
                if (layoutCache.windows == null)
                    layoutCache.windows = new GUILayoutGroup();

                layoutCache.topLevel.entries.Clear();
                layoutCache.windows.entries.Clear();

                GUILayoutUtility.current.topLevel = layoutCache.topLevel;
                GUILayoutUtility.current.layoutGroups.Clear();
                GUILayoutUtility.current.layoutGroups.Push(GUILayoutUtility.current.topLevel);

                return false;
            }
            GUILayoutUtility.current.topLevel = layoutCache.topLevel;
            GUILayoutUtility.current.layoutGroups = layoutCache.layoutGroups;
            GUILayoutUtility.current.windows = layoutCache.windows;

            return false;
        }
    }
}
