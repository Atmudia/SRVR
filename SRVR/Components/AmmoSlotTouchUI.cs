using SRVR.Patches;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace SRVR
{
    public class AmmoSlotTouchUI : MonoBehaviour
    {
        public int slotIDX;

        public Transform slotObject;
        private Image _frameImg;
        private Ammo _ammo;

        private void Awake()
        {
            _frameImg = slotObject.Find("Ammo Slot/Frame").GetComponent<Image>();
            _ammo = SceneContext.Instance.PlayerState.Ammo;
        }

        public void Update()
        {
            transform.position = slotObject.position;
            _frameImg.color = _ammo.selectedAmmoIdx == slotIDX ? Color.yellow : Color.white;
        }
        
        
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Hand")
            {
                _ammo.selectedAmmoIdx = slotIDX;
            }
        }
    }
    
}