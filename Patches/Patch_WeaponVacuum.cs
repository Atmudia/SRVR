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
    }
}
