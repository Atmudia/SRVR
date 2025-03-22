using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SRVR.Components;
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

            foreach (var instruction in codeInstructions)
            {
                // Look for the Physics.Raycast call
                if (instruction.opcode == OpCodes.Call && instruction.operand as MethodInfo == raycastMethod)
                {
                    // Replace the Physics.Raycast call with CustomRaycast
                    instruction.operand = customRaycastMethod;
                    Debug.Log("Replaced Physics.Raycast with CustomRaycast");
                }
            }

            return codeInstructions;
        }
        public static bool CustomRaycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
        {
            Vector3 startPoint = HandManager.Instance.FPInteract.position;
            Vector3 endPoint = HandManager.Instance.FPInteract.position + HandManager.Instance.FPInteract.forward;
            var capsuleCast = Physics.CapsuleCast(startPoint, endPoint, 0.3f, HandManager.Instance.FPInteract.forward, out hitInfo, 10);
            //
            return capsuleCast;
        }
    }
}