using SRVR.Patches;
using UnityEngine;
using UnityEngine.XR;

namespace SRVR.Components
{
    public class PosHand : MonoBehaviour
    {
        public XRNode hand;
        public void LateUpdate()
        {
            // TODO: cache
            // need to figure out how to know if input device has changed first
            InputDevice rightHand = InputDevices.GetDeviceAtXRNode(hand);

            if (rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
            {
                var rotHMDPos = (Quaternion.AngleAxis(Patch_vp_FPInput.adjustmentDegrees, Vector3.up) * Patch_vp_FPInput.HMDPosition);
                rotHMDPos.y = 0;
                transform.position = transform.parent.position + (Quaternion.AngleAxis(Patch_vp_FPInput.adjustmentDegrees, Vector3.up) * pos) - rotHMDPos;           
            }
            if (rightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
                transform.rotation = Quaternion.Euler(rot.eulerAngles + (Vector3.up * Patch_vp_FPInput.adjustmentDegrees));
        }
    }
}