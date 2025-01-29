using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
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

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                // Look for the Physics.Raycast call
                if (codeInstructions[i].opcode == OpCodes.Call && codeInstructions[i].operand as MethodInfo == raycastMethod)
                {
                    // Replace the Physics.Raycast call with CustomRaycast
                    codeInstructions[i].operand = customRaycastMethod;
                    Debug.Log("Replaced Physics.Raycast with CustomRaycast");
                }
            }

            return codeInstructions;
        }
        public static bool CustomRaycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
        {
            var controller = Patch_vp_FPWeapon.FPWeapon.transform;
            ray = new Ray(controller.position, controller.forward);
            Vector3 halfExtents = Vector3.one; 

;
            bool hit = Physics.BoxCast(ray.origin, halfExtents, ray.direction, out hitInfo, Quaternion.identity, 10f);

            Vector3 centerAddition = (Vector3.forward * 5).MultipliedBy(controller.forward);
            Vector3 center = controller.position + centerAddition;
            
            Collider[] colliders = Physics.OverlapBox(center, new Vector3(4f, 4f, 5f), controller.rotation, -1, QueryTriggerInteraction.Collide);
            Collider select = null;

            foreach (Collider collider in colliders)
            {
                UIActivator uiActivator = null;
                SlimeGateActivator slimeGateActivator = null;
                TreasurePod treasurePod = null;
                TechActivator techActivator = null;
                GadgetInteractor gadgetInteractor = null;
                GadgetSite gadgetSite = null;
                
                
                GameObject gameObject = hitInfo.collider.gameObject;
                
                uiActivator = gameObject.GetComponent<UIActivator>();
                slimeGateActivator = gameObject.GetComponent<SlimeGateActivator>();
                treasurePod = gameObject.GetComponent<TreasurePod>();
                techActivator = gameObject.GetComponent<TechActivator>();
                gadgetInteractor = gameObject.GetComponentInParent<GadgetInteractor>();
                gadgetSite = gameObject.GetComponentInParent<GadgetSite>();

                if (uiActivator || slimeGateActivator || treasurePod || techActivator != null || gadgetInteractor != null|| gadgetSite)
                {
                    select = collider;
                    break;
                }
            }
            
            hitInfo.SetField("m_Collider", select);
            
            return select != null;
        }
    }
}