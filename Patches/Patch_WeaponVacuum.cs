using HarmonyLib;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(WeaponVacuum))]
    public class Patch_WeaponVacuum
    {
        [HarmonyPrefix, HarmonyPatch("SetHeldRad")]
        public static bool DisableHeldRadius(WeaponVacuum __instance, float rad) => false;
        
        [HarmonyPrefix, HarmonyPatch("LateUpdate")]
        public static bool DisableLateUpdate(WeaponVacuum __instance) => false;
        
        [HarmonyPrefix, HarmonyPatch("ExpelHeld")]
        public static void FixShootHeld(WeaponVacuum __instance) => __instance.held.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
}
