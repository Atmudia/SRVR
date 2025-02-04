using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(Vacuumable))]
    public class Patch_Vacuumable
    {
        static Dictionary<Vacuumable, Vector3> originalScale = new Dictionary<Vacuumable, Vector3>();
        
        [HarmonyPrefix, HarmonyPatch("SetHeld")]
        public static bool OverwriteSetHeld(Vacuumable __instance, bool held)
        {
            if (!held)
            {
                if (!originalScale.ContainsKey(__instance))
                    return true;
                
                __instance.held = false;
                
                __instance.UpdateLayer();
                
                __instance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                
                __instance.transform.parent = null;
                
                __instance.transform.localScale = originalScale[__instance];
                originalScale.Remove(__instance);
            }
            else
            {
                __instance.held = true;
            
                __instance.delaunch();
            
                __instance.SetLayerRecursively(LayerMask.NameToLayer("Held"), true);

                if (!originalScale.ContainsKey(__instance))
                    originalScale.Add(__instance, __instance.transform.lossyScale);
                
                __instance.transform.SetParent(Patch_vp_FPWeapon.FPWeapon.Find("bone_vac/Scaler"));
                __instance.transform.localScale = Vector3.one * 3.45f;
                __instance.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
                __instance.transform.localPosition = Vector3.right * 3f;
                
                __instance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }

            return false;
        }
    }
}