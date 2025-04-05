using HarmonyLib;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(CameraDisabler), "AddBlocker")]
    internal static class Patch_CameraDisabler
    {
        public static void Prefix(CameraDisabler __instance)
        {
            if (__instance.blockers.Count <= 0)
                __instance.standardMask = __instance.cam.cullingMask;
        }
    }
}
