using HarmonyLib;
namespace SRVR.Patches
{
    [HarmonyPatch(typeof(WeaponVacuum))]
    public class Patch_WeaponVacuum
    {
        [HarmonyPrefix, HarmonyPatch("SetHeldRad")]
        public static bool DisableHeldRadius(WeaponVacuum __instance, float rad) => false;
    }
}
