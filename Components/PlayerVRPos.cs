using SRVR.Patches;
using UnityEngine;

namespace SRVR.Components
{
    public class PlayerVRPos : MonoBehaviour
    {
        private CharacterController _player;

        private void Awake()
        {
            _player = transform.parent.GetComponent<CharacterController>();
        }

        void LateUpdate()
        {
            if (_player)
            {
                _player.center = Vector3.up + new Vector3(Patch_vp_FPInput.HMDPosition.x, 0, Patch_vp_FPInput.HMDPosition.z);
            }
        }
    }
}