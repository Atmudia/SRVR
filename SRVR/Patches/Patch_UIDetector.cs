using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SRVR.Components;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(UIDetector))]
    internal static class Patch_UIDetector
    {
        [HarmonyPostfix, HarmonyPatch(nameof(UIDetector.Start))]
        public static void Start(UIDetector __instance)
        {
            __instance.weaponVac = HandManager.Instance?.vacuumer;
            __instance.fpInput = GameObject.Find("SimplePlayer")?.GetComponentInChildren<vp_FPInput>();
        }

        [HarmonyTranspiler, HarmonyPatch(nameof(UIDetector.Update))]
        static IEnumerable<CodeInstruction> Update(IEnumerable<CodeInstruction> instructions)
        {
            if (!EntryPoint.EnabledVR)
                return instructions;
           
            List<CodeInstruction> instr = new List<CodeInstruction>(instructions.MethodReplacer(AccessTools.Method(typeof(Physics), nameof(Physics.Raycast), new[] {
                typeof(Ray),
                typeof(RaycastHit).MakeByRefType(),
                typeof(float)
            }), AccessTools.Method(typeof(Patch_UIDetector), nameof(CustomRaycast))));

            instr.Insert(instr.FindIndex(x => x.operand is MethodBase mb && mb.Name == nameof(CustomRaycast)), new CodeInstruction(OpCodes.Ldarg_0));
            
            return instr;
        }
        public static bool CustomRaycast(Ray ray, out RaycastHit hitInfo, float maxDistance, UIDetector instance)
        {
            Vector3 startPoint = instance.transform.position;
            Vector3 endPoint = instance.transform.position + instance.transform.forward;

            var capsuleCast = Physics.Raycast(startPoint, instance.transform.forward, out hitInfo, 3, -1, QueryTriggerInteraction.Collide) &&
                (instance != HandManager.Instance?.dominantUIDetector || hitInfo.collider != HandManager.Instance?.pediaInteractable);
            
            return capsuleCast;
        }
    }
}