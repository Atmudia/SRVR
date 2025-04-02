using HarmonyLib;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(WeaponVacuum))]
    public class Patch_WeaponVacuum
    {
        [HarmonyPrefix, HarmonyPatch(nameof(WeaponVacuum.SetHeldRad))]
        public static bool DisableSetHeldRad() => !EntryPoint.EnabledVR;
        
        [HarmonyPrefix, HarmonyPatch(nameof(WeaponVacuum.LateUpdate))]
        public static bool DisableLateUpdate() => !EntryPoint.EnabledVR;
        
        [HarmonyPrefix, HarmonyPatch(nameof(WeaponVacuum.ExpelHeld))]
        public static void FixExpelHeld(WeaponVacuum __instance) => __instance.held.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        [HarmonyPrefix, HarmonyPatch(nameof(WeaponVacuum.SetHeldRad))]
        public static void FixHeldRad(ref float rad) => rad *= 0.646875f;

        [HarmonyPostfix, HarmonyPatch(nameof(WeaponVacuum.SetHeldRad))]
        public static void FixHeldRadPositioning(WeaponVacuum __instance, float rad)
        {
            if (rad != 0)
                return;

            Vector3 localPosition = __instance.lockJoint.transform.localPosition;
            __instance.lockJoint.transform.localPosition = new Vector3(localPosition.x, 0.646875f, localPosition.z);
        }
    }
}
