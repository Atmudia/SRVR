using SRVR.Patches;
using System;
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
        }
    }
     public class PosHMD : MonoBehaviour
     {
         public SteamVR_Action_Pose Pose;
         public void LateUpdate()
         {
             // // // this.transform.localRotation =  UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
             // this.transform.localPosition =  UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
             // this.transform.localRotation =  UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
         }
     }

    public class PosHand : MonoBehaviour
    {
        public void LateUpdate()
        {
            // TODO: cache
            // need to figure out how to know if input device has changed first
            InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
                transform.position = transform.parent.position + (Quaternion.AngleAxis(Patch_vp_FPInput.adjustmentDegrees, Vector3.up) * pos);
            if (rightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
                transform.rotation = Quaternion.Euler(rot.eulerAngles + (Vector3.up * Patch_vp_FPInput.adjustmentDegrees));
        }
    }
}