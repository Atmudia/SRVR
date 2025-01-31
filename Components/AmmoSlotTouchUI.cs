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
        private Image frameImg;
        private Ammo ammo;

        private void Awake()
        {
            frameImg = slotObject.Find("Ammo Slot/Frame").GetComponent<Image>();
            ammo = SceneContext.Instance.PlayerState.Ammo;
        }

        public void Update()
        {
            transform.position = slotObject.position;
            frameImg.color = ammo.selectedAmmoIdx == slotIDX ? Color.yellow : Color.white;
        }
        
        
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Left Hand")
            {
                ammo.selectedAmmoIdx = slotIDX;
            }
        }
    }
    
}