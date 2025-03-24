using SRVR.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace SRVR.Components
{
    public class PosHMD : MonoBehaviour
    {
        public vp_FPCamera baseCam;

        private SteamVR_Events.Action newPosesAction;

        const int HMD_INDEX = (int)OpenVR.k_unTrackedDeviceIndex_Hmd;

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            if (poses.Length <= HMD_INDEX || !poses[HMD_INDEX].bDeviceIsConnected || !poses[HMD_INDEX].bPoseIsValid)
                return;

            SteamVR_Utils.RigidTransform pose = new SteamVR_Utils.RigidTransform(poses[HMD_INDEX].mDeviceToAbsoluteTracking);

            Vector3 eulerAngles = pose.rot.eulerAngles;
            baseCam.Transform.localRotation = Quaternion.Euler(eulerAngles.x, 0, eulerAngles.z);
            baseCam.Parent.localRotation = Quaternion.Euler(0, eulerAngles.y + Patch_vp_FPInput.adjustmentDegrees, 0);

            Vector3 pos = pose.pos;
            var difference = pos - Patch_vp_FPInput.HMDPosition;
            Vector3 rotatedPos = Quaternion.Euler(0, Patch_vp_FPInput.adjustmentDegrees, 0) * difference;
            rotatedPos.y = 0;
            baseCam.Parent.position += rotatedPos;
            baseCam.transform.position = baseCam.Parent.position + new Vector3(0, pos.y, 0);

            Patch_vp_FPInput.HMDPosition = pose.pos;
        }

        public void OnEnable() => newPosesAction.enabled = true;
        public void OnDisable() => newPosesAction.enabled = false;

        public PosHMD() => newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
    }
}
