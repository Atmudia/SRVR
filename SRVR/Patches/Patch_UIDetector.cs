using System.Collections.Generic;
using HarmonyLib;
using SRVR.Components;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(UIDetector))]
    internal static class Patch_UIDetector
    {
        [HarmonyTranspiler, HarmonyPatch(nameof(UIDetector.Update))]
        static IEnumerable<CodeInstruction> Update(IEnumerable<CodeInstruction> instructions)
        {
            if (!EntryPoint.EnabledVR)
                return instructions;
            return instructions.MethodReplacer(AccessTools.Method(typeof(Physics), nameof(Physics.Raycast), new[] {
                typeof(Ray),
                typeof(RaycastHit).MakeByRefType(),
                typeof(float)
            }), AccessTools.Method(typeof(Patch_UIDetector), nameof(CustomRaycast)));
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