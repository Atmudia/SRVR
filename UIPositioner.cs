using SRVR.Patches;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using static SECTR_AudioSystem;

namespace SRVR
{
    public class UIPositioner : MonoBehaviour
    {
        public void LateUpdate()
        {
            if (Camera.main == null) return;
            transform.position = Camera.main.transform.position + Camera.main.transform.forward;
            transform.rotation = Camera.main.transform.rotation;
            
            if (VRConfig.STATIC_UI_POSITION && !(DisableStaticPosition.Contains(gameObject.name) || IsInCategory(gameObject.name))) Destroy(this);
        }

        public static bool IsInCategory(string name)
        {
            foreach (var category in DisableStaticPositionForCategories)
            {
                if (name.Contains(category)) return true;
            }
            return false;
        }
        
        public static readonly string[] DisableStaticPosition = new[]
        {
            "TitleUI",
            "GlitchTerminalActivatorUI_Ammo(Clone)",
            "ExchangeOfflineUI(Clone)",
        };

        public static readonly string[] DisableStaticPositionForCategories = new[]
        {
            "Activate",
            "Popup",
        };
    }

    public class PosHand : MonoBehaviour
    {
        public float offset = 0f; // use this for when the position is going the wrong direction
        public XRNode hand;
        public void LateUpdate()
        {
            // TODO: cache
            // need to figure out how to know if input device has changed first
            InputDevice rightHand = InputDevices.GetDeviceAtXRNode(hand);

            if (rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
                transform.position = transform.parent.position + (Quaternion.AngleAxis(Patch_vp_FPInput.adjustmentDegrees, Vector3.up) * pos);
            
            if (rightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
                transform.rotation = Quaternion.Euler(rot.eulerAngles + (Vector3.up * Patch_vp_FPInput.adjustmentDegrees));
        }
    }
    public class PlayerVRPos : MonoBehaviour
    {
        void LateUpdate()
        {
            var player = transform.parent.GetComponent<CharacterController>();
            if (player != null)
            {
                player.center = Vector3.up + new Vector3(Patch_vp_FPInput.HMDPosition.x, 0, Patch_vp_FPInput.HMDPosition.z);
            }
        }
    }

    public class PediaInteract : MonoBehaviour
    {
        internal static GameObject pediaModel;
        public void OnCollisionEnter(Collision other)
        {
            Debug.Log(other.gameObject.name);
            if (other.gameObject.name == "Controller (Left)")
            {
                PediaDirector.Id pediaId = PediaDirector.Id.BASICS;
                PediaPopupUI objectOfType = FindObjectOfType<PediaPopupUI>();
                
                if (objectOfType)
                    pediaId = objectOfType.GetId();
                
                SceneContext.Instance.PediaDirector.ShowPedia(pediaId);
            }
        }
    }
}