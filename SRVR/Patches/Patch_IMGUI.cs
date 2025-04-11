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
                else
                    ResetGUILayoutGroup(layoutCache.topLevel);

                if (layoutCache.windows == null)
                    layoutCache.windows = new GUILayoutGroup();
                else
                    ResetGUILayoutGroup(layoutCache.windows);

                GUILayoutUtility.current.topLevel = layoutCache.topLevel;
                GUILayoutUtility.current.layoutGroups.Clear();
                GUILayoutUtility.current.layoutGroups.Push(GUILayoutUtility.current.topLevel);
                GUILayoutUtility.current.windows = layoutCache.windows;
            }
            else
            {
                GUILayoutUtility.current.topLevel = layoutCache.topLevel;
                GUILayoutUtility.current.layoutGroups = layoutCache.layoutGroups;
                GUILayoutUtility.current.windows = layoutCache.windows;
            }

            return false;
        }

        internal static void ResetGUILayoutGroup(GUILayoutGroup layoutGroup)
        {
            layoutGroup.entries.Clear();
            layoutGroup.isVertical = true;
            layoutGroup.resetCoords = false;
            layoutGroup.spacing = 0f;
            layoutGroup.sameSize = true;
            layoutGroup.isWindow = false;
            layoutGroup.windowID = -1;
            layoutGroup.m_Cursor = 0;
            layoutGroup.m_StretchableCountX = 100;
            layoutGroup.m_StretchableCountY = 100;
            layoutGroup.m_UserSpecifiedWidth = false;
            layoutGroup.m_UserSpecifiedHeight = false;
            layoutGroup.m_ChildMinWidth = 100f;
            layoutGroup.m_ChildMaxWidth = 100f;
            layoutGroup.m_ChildMinHeight = 100f;
            layoutGroup.m_ChildMaxHeight = 100f;
            layoutGroup.m_MarginLeft = 0;
            layoutGroup.m_MarginRight = 0;
            layoutGroup.m_MarginTop = 0;
            layoutGroup.m_MarginBottom = 0;
            layoutGroup.rect = new Rect(0f, 0f, 0f, 0f);
            layoutGroup.consideredForMargin = true;
            layoutGroup.m_Style = GUIStyle.none;
            layoutGroup.stretchWidth = 1;
            layoutGroup.stretchHeight = 0;
            layoutGroup.minWidth = 0f;
            layoutGroup.maxWidth = 0f;
            layoutGroup.minHeight = 0f;
            layoutGroup.maxHeight = 0f;
        }
    }
}
