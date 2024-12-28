using System;
using UnityEngine;
using Valve.VR;

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
}