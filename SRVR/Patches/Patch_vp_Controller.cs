using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace SRVR.Patches;

[HarmonyPatch]
internal static class Patch_vp_Controller
{
    public static IEnumerable<MethodInfo> TargetMethods()
    {
        yield return AccessTools.Method(typeof(vp_FPController), nameof(vp_FPController.FixedMove));
        yield return AccessTools.Method(typeof(vp_Controller), nameof(vp_Controller.StoreGroundInfo));
    }

    private static int WeaponMask = LayerMask.NameToLayer("Weapon");

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions.ToList();

        for (int i = codeInstructions.Count - 1; i >= 0; i--)
        {
            var instruction = codeInstructions[i];
            if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo { Name: "SphereCast" })
            {
                codeInstructions.Insert(i, CodeInstruction.Call(typeof(Patch_vp_Controller), nameof(ModifyLayerMask)));
                EntryPoint.ConsoleInstance.Log($"Patched Layer Mask before SphereCast at index {i}");
            }
        }

        return codeInstructions;
    }

    public static int ModifyLayerMask(int layerMask) => layerMask & ~(1 << WeaponMask);
}