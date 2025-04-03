using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch]
    internal static class Patch_SlimeSubbehaviours
    {
        public static float GetReplacedFixedDeltaTime() => 0.025f;

        // find all methods in every SlimeSubbehaviour registered, and select all non-Update methods (places where fixedDeltaTime should NOT be)
        public static IEnumerable<MethodBase> TargetMethods() =>
            AccessTools.AllTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(SlimeSubbehaviour)))
            .Select(x => AccessTools.GetDeclaredMethods(x).Where(y => !y.Name.Contains("Update"))).SelectMany(x => x);

        // if the method actually does AddForce, that means fixedDeltaTime is probably used in bad math, so replace fixedDeltaTime with the proper value
        // because fixedDeltaTime is just a property, we can use one of the common transpilers in the Transpilers class to replace its value being obtained. neat!
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (!EntryPoint.EnabledVR)
                return instructions;
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            return codeInstructions.Any(x => x.operand is MethodBase mb && mb.Name.Contains("AddForce"))
                ? codeInstructions.MethodReplacer(AccessTools.PropertyGetter(typeof(Time), "fixedDeltaTime"),
                    AccessTools.Method(typeof(Patch_SlimeSubbehaviours), "GetReplacedFixedDeltaTime"))
                : codeInstructions;
        }
    }
}
