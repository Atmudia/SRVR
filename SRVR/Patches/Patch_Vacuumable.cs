using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SRVR.Components;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(Vacuumable))]
    internal class Patch_Vacuumable
    {
        public static bool doNotParent = false;
        public static Dictionary<Vacuumable, Vector3> originalScale = new Dictionary<Vacuumable, Vector3>();

        [HarmonyPatch("isCaptive")]
        [HarmonyPostfix]
        public static void IsCaptivePostfix(Vacuumable __instance, ref bool __result) => __result = __result || (HandManager.Instance?.heldVacuumables?.ContainsKey(__instance) ?? false);

        [HarmonyPatch("SetCaptive")]
        [HarmonyPrefix]
        public static void SetCaptivePrefix(Vacuumable __instance, Joint toJoint)
        {
            if (toJoint == null && HandManager.Instance.heldVacuumables.TryGetValue(__instance, out PickupVacuumable pickuper))
                pickuper.Drop(false);
        }

        [HarmonyPatch("SetHeld")]
        [HarmonyPrefix]
        public static void SetHeldPrefix(Vacuumable __instance, bool held)
        {
            if (__instance.held == held)
            {
                doNotParent = false;
                return;
            }

            if (held)
            {
                originalScale[__instance] = __instance.transform.localScale;

                if (!doNotParent)
                {
                    __instance.transform.SetParent(HandManager.Instance.FPWeapon.Find("bone_vac/Scaler")); // this is necessary to make the object stay attached even when you pause the game
                    __instance.transform.localScale *= 0.646875f;
                    __instance.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
                    __instance.transform.localPosition = Vector3.right * 3f;
                }
                
                __instance.body.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                __instance.transform.parent = null;
                __instance.transform.localScale = originalScale[__instance];
                __instance.body.constraints = RigidbodyConstraints.None;

                originalScale.Remove(__instance); // not technically needed, but for the sake of not leaking memory, this good
                HandManager.Instance.heldVacuumables.Remove(__instance);
            }
            doNotParent = false;
        }

        // weapon camera shouldn't be activated; this prevents that
        [HarmonyPatch("SetHeld")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetHeldTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instr = new List<CodeInstruction>(instructions);
            int index = instr.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand is MethodBase b && b.Name == "SetActive");

            if (index != -1)
            {
                instr[index - 1].opcode = OpCodes.Ldc_I4_0;
                instr[index - 1].operand = null;
                instr.RemoveAt(index - 2);
            }

            return instr;
        }

        [HarmonyPatch("UpdateMaterialsHeldState")]
        [HarmonyPrefix]
        public static bool UpdateMaterialsHeldStatePatch() => false;
    }
}