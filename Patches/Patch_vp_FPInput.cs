using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InControl;
using UnityEngine;
using UnityEngine.XR;
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
        public static float adjustmentDegrees = 0f;

        [HarmonyPrefix, HarmonyPatch(typeof(vp_FPCamera), nameof(vp_FPCamera.LateUpdate))]

        public static bool LateUpdate(vp_FPCamera __instance)
        {
            UnityEngine.XR.InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);

            if (head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
            {
                Vector3 eulerAngles = rot.eulerAngles;
                __instance.Transform.localRotation = Quaternion.Euler(eulerAngles.x, 0, eulerAngles.z);
                __instance.Parent.localRotation = Quaternion.Euler(0, eulerAngles.y + adjustmentDegrees, 0);
            }
            if (head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
            {
                // TODO: not great solution !! head position becomes unbound from body position, meaning moving physically doesn't ACTUALLY move the player,
                // it just looks like it does. fixing would require setting the camera position to just the y value here, then updating the
                // controller position with the x and z. HOWEVER: this would result in Collision Hell. to make it work well, you'd need to move the
                // controller until it collides with a wall, then move the camera the rest of the way. Implement This Later.
                __instance.transform.position = __instance.Parent.position + (Quaternion.AngleAxis(adjustmentDegrees, Vector3.up) * pos);
            }

            return false;
        }
    }

}
