using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SRVR.Components;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(QuicksilverEnergyGenerator))]
    public class Patch_QuicksilverEnergyGenerator
    {
        [HarmonyTranspiler, HarmonyPatch(nameof(QuicksilverEnergyGenerator.SetState))]
        public static IEnumerable<CodeInstruction> SetState(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var sInd = code.FindIndex(x => x.operand is MethodInfo { Name: "get_Instance" } m && m.DeclaringType == typeof(SRSingleton<SceneContext>));
            var eInd = code.FindIndex(sInd, x => x.operand is MethodInfo { Name: "SetQuicksilverEnergyGenerator" });
            var lbl = code[sInd].labels;
            code.RemoveRange(sInd,eInd - sInd + 1);
            code.InsertRange(sInd, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0) { labels = lbl },
                CodeInstruction.Call(typeof(Patch_QuicksilverEnergyGenerator), nameof(SetQuicksilverEnergyGeneratorAlternative))
            });
            return code;
        }
        public static void SetQuicksilverEnergyGeneratorAlternative(QuicksilverEnergyGenerator __instance) => HandManager.Instance.FPWeapon.GetComponentInChildren<VacDisplayTimer>().SetQuicksilverEnergyGenerator(__instance);

    }
}