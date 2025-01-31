using HarmonyLib;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(LoadingUI))]
    public static class Patch_LoadingUI
    {
        [HarmonyPrefix, HarmonyPatch(nameof(LoadingUI.OnEnable))]
        public static void OnEnable()
        {
            //FIX
        }
    }
}