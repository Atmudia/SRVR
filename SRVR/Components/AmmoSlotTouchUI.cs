using SRVR.Components;
using UnityEngine;

namespace SRVR
{
    public class AmmoSlotTouchUI : MonoBehaviour, TechActivator
    {
        public int slotIDX;
        private Ammo _ammo;

        public void Awake() => _ammo = SceneContext.Instance.PlayerState.Ammo;

        public void Activate()
        {
            if (_ammo.SetAmmoSlot(slotIDX) && HandManager.Instance?.vacuumer)
            {
                WeaponVacuum vacuumer = HandManager.Instance.vacuumer;
                vacuumer.PlayTransientAudio(vacuumer.vacAmmoSelectCue);
                vacuumer.vacAnimator.SetTrigger(vacuumer.animSwitchSlotsId);
            }
        }

        public GameObject GetCustomGuiPrefab() => null;
    }
}