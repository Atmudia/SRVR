using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(UIDetector))]
    public static class Patch_UIDetector
    {
        [HarmonyTranspiler, HarmonyPatch(nameof(UIDetector.Update))]
        static IEnumerable<CodeInstruction> Update(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            var raycastMethod = AccessTools.Method(typeof(Physics), nameof(Physics.Raycast), new[] {
                typeof(Ray),
                typeof(RaycastHit).MakeByRefType(),
                typeof(float)
            });

            // Define the replacement method
            var customRaycastMethod = AccessTools.Method(typeof(Patch_UIDetector), nameof(CustomRaycast));

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                // Look for the Physics.Raycast call
                if (codeInstructions[i].opcode == OpCodes.Call && codeInstructions[i].operand as MethodInfo == raycastMethod)
                {
                    // Replace the Physics.Raycast call with CustomRaycast
                    codeInstructions[i].operand = customRaycastMethod;
                    Debug.Log("Replaced Physics.Raycast with CustomRaycast");
                }
            }

            return codeInstructions;
        }
        public static bool CustomRaycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
        {
            
            var controller = Patch_vp_FPWeapon.FPInteract;
            ray = new Ray(controller.position, controller.forward);
            Vector3 halfExtents = Vector3.one * 1.5f; 
            
            // Change the max distance if it doesnt seem right ingame.
            bool hit = Physics.BoxCast(ray.origin, halfExtents, ray.direction, out hitInfo, Quaternion.identity, .75f);

            return hit;
        }
    }
}