using SRVR.Patches;
using UnityEngine;
using Valve.VR;

namespace SRVR.Components
{
    public class PosHand : MonoBehaviour
    {
        public uint deviceIndex = 9999;

        private SteamVR_Events.Action newPosesAction;

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            if (deviceIndex == 9999)
                return;

            if (poses.Length <= deviceIndex || !poses[deviceIndex].bDeviceIsConnected || !poses[deviceIndex].bPoseIsValid)
                return;

            SteamVR_Utils.RigidTransform pose = new SteamVR_Utils.RigidTransform(poses[deviceIndex].mDeviceToAbsoluteTracking);

            var rotHMDPos = (Quaternion.AngleAxis(Patch_vp_FPInput.AdjustmentDegrees, Vector3.up) * Patch_vp_FPInput.HMDPosition);
            rotHMDPos.y = 0;
            transform.position = transform.parent.position + (Quaternion.AngleAxis(Patch_vp_FPInput.AdjustmentDegrees, Vector3.up) * pose.pos) - rotHMDPos + (Vector3.up * VRConfig.HEIGHT_ADJUSTMENT);

            transform.rotation = Quaternion.Euler(pose.rot.eulerAngles + (Vector3.up * Patch_vp_FPInput.AdjustmentDegrees));
        }

        public void OnEnable() => newPosesAction.enabled = true;
        public void OnDisable() => newPosesAction.enabled = false;

        public PosHand() => newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
    }
}