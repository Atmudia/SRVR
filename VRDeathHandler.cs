using System.Collections;
using HarmonyLib;
using SRVR.Components;
using SRVR.Patches;
using UnityEngine;

namespace SRVR
{
    public class VRDeathHandler
    {
        public static void EventAdd()
        {
            LockOnDeath.Instance.onLockChanged += OnLockChanged;
        }

        private const float DURATION = .5f;

        private static IEnumerator ShrinkController(Transform obj)
        {
            float time = 0f;
            
            while (obj.transform.localScale.x > 0f && obj.transform.localScale.y > 0f && obj.transform.localScale.z > 0f)
            {
                
                time += Time.deltaTime;
                float lerp = time / DURATION;
                
                obj.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, lerp);
                
                yield return null;
            }
        }
        private static IEnumerator GrowController(Transform obj)
        {
            float time = 0f;
            
            while (obj.transform.localScale is { x: < 1f, y: < 1f } && obj.transform.localScale.z < 1f)
            {
                
                time += Time.deltaTime;
                float lerp = time / DURATION;
                
                obj.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, lerp);
                
                yield return null;
            }
        }
        
        private static void OnLockChanged(bool locked)
        {
            if (locked)
            {
                SceneContext.Instance.StartCoroutine(ShrinkController(HandManager.Instance.FPWeapon.parent));
                SceneContext.Instance.StartCoroutine(ShrinkController(HandManager.Instance.FPInteract));
            }
            else
            {
                SceneContext.Instance.StartCoroutine(GrowController(HandManager.Instance.FPWeapon.parent));
                SceneContext.Instance.StartCoroutine(GrowController(HandManager.Instance.FPInteract));
            }
        }
    }
}