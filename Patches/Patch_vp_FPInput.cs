using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InControl;
using UnityEngine;
using Valve.VR;

namespace SRVR.Patches
{

    [HarmonyPatch(typeof(InputDirector), nameof(InputDirector.UsingGamepad))]
    public static class Patch_InputDirector
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch()]
        public static class Patch_vp_FPInput
        {
            public static bool UsingVR()
            {
                if (EntryPoint.EnabledVR)
                    return true;
                return InputDirector.UsingGamepad();
            }

            [HarmonyTranspiler, HarmonyPatch(typeof(vp_FPInput), nameof(vp_FPInput.GetMouseLook))]

            static IEnumerable<CodeInstruction> GetMouseLook(IEnumerable<CodeInstruction> instructions)
            {
                var codeInstructions = instructions.ToList();
                foreach (var codeInstruction in codeInstructions)
                {
                    if (codeInstruction.Calls(AccessTools.Method(typeof(InputDirector),
                            nameof(InputDirector.UsingGamepad))))
                    {
                        codeInstruction.operand = AccessTools.Method(typeof(Patch_vp_FPInput), nameof(UsingVR));
                        EntryPoint.ConsoleInstance.Log("Changed: GetMouseLook ");
                    }
                }

                
                return codeInstructions;
            }

            private static float offset = 0f;
            [HarmonyPostfix, HarmonyPatch(typeof(vp_FPCamera), nameof(vp_FPCamera.LateUpdate))]
            
            public static void LateUpdate(vp_FPCamera __instance)
            {
                var hmdRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
                __instance.Transform.rotation = Quaternion.Slerp(__instance.Transform.rotation, hmdRotation, Time.deltaTime * 10f);
                
            }
        }
    
}
