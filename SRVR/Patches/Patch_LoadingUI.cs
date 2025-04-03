using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(LoadingUI))]
    internal static class Patch_LoadingUI
    {
        internal static Sprite BackgroundSprite;
        [HarmonyPrefix, HarmonyPatch(nameof(LoadingUI.OnEnable))]
        public static void OnEnable(LoadingUI __instance)
        {
            if (!EntryPoint.EnabledVR) 
                return;
            var bg = __instance.transform.Find("Background").GetComponent<Image>();
            var shadow = __instance.transform.Find("AutoSaveTipPanel/Shadow RightCorner").gameObject;

            bg.color = new Color(0, 0, 0, 0.45f);
            bg.sprite = BackgroundSprite;
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 0.2f;
            
            shadow.Destroy();
        }
    }
}