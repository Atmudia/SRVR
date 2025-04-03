using DG.Tweening;
using SRVR.Components;
using UnityEngine;

namespace SRVR
{
    public class VRDeathHandler
    {
        public static void EventAdd()
        {
            LockOnDeath.Instance.onLockChanged += OnLockChanged;
        }

        private const float DURATION = .5f;

        private static void ShrinkController(Transform obj)
        {
            obj.Safe()?.DOScale(Vector3.zero, DURATION).SetEase(Ease.InOutSine);
        }
        private static void GrowController(Transform obj)
        {
            if (obj == null) return;
            obj.Safe()?.DOScale(Vector3.one, DURATION).SetEase(Ease.InOutSine);
        }
        
        private static void OnLockChanged(bool locked)
        {
            if (locked)
            {
                ShrinkController(HandManager.Instance.FPWeapon.parent);
                ShrinkController(HandManager.Instance.FPInteract);
            }
            else
            {
                GrowController(HandManager.Instance.FPWeapon.parent);
                GrowController(HandManager.Instance.FPInteract);
            }
        }
    }
}