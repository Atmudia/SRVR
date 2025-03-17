using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch]
    public static class Patch_SlimeSubbehaviours
    {
        public static float GetReplacedFixedDeltaTime() => 0.025f;

        public static IEnumerable<MethodBase> TargetMethods() =>
            AccessTools.AllTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(SlimeSubbehaviour)))
            .Select(x => AccessTools.GetDeclaredMethods(x).Where(y => !y.Name.Contains("Update"))).SelectMany(x => x);

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
            instructions.Any(x => x.operand is MethodBase mb && mb.Name.Contains("AddForce")) ? Transpilers.MethodReplacer(instructions, AccessTools.PropertyGetter(typeof(Time), "fixedDeltaTime"), 
                AccessTools.Method(typeof(Patch_SlimeSubbehaviours), "GetReplacedFixedDeltaTime")) : instructions;
    }
}
