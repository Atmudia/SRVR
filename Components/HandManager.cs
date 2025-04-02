using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace SRVR.Components
{
    public class HandManager : SRSingleton<HandManager>
    {
        public GameObject leftController;
        public GameObject rightController;

        public GameObject leftHand;
        public GameObject rightHand;
        public GameObject leftHandModel;
        public GameObject rightHandModel;

        public PosHand leftHandPositioner;
        public PosHand rightHandPositioner;
        public PickupVacuumable leftPickuper;
        public PickupVacuumable rightPickuper;

        public WeaponVacuum vacuumer;
        public Transform FPWeapon;
        public Transform FPInteract;
        public GameObject UI;

        public XRNode dominantHand;

        public bool vacShown = true;
        public Dictionary<Vacuumable, PickupVacuumable> heldVacuumables = new Dictionary<Vacuumable, PickupVacuumable>();
        public Vacuumable gunVacuumable;

        public void OnEnable()
        {
            InputTracking.nodeAdded += UpdateHandStates;
            InputTracking.nodeRemoved += UpdateHandStates;
        }
        public void OnDisable()
        {
            InputTracking.nodeAdded -= UpdateHandStates;
            InputTracking.nodeRemoved -= UpdateHandStates;
        }

        public void UpdateVacVisibility() => SetVacVisibility(vacShown);
        public void SetVacVisibility(bool showVac)
        {
            vacShown = showVac;

            if (FPWeapon)
            {
                FPWeapon.gameObject.SetActive(VRInput.Mode == SRInput.InputMode.DEFAULT && showVac);
                UI.SetActive(FPWeapon.gameObject.activeSelf);
                vacuumer.enabled = FPWeapon.gameObject.activeSelf;
            }
            
            leftHand.SetActive(VRInput.Mode != SRInput.InputMode.DEFAULT || dominantHand == XRNode.RightHand || !showVac);
            rightHand.SetActive(VRInput.Mode != SRInput.InputMode.DEFAULT || dominantHand == XRNode.LeftHand || !showVac);
        }

        public void UpdateHandStates() => UpdateHandStates(default);

        // this implementation deals with a weird edge case that someone has multiple left or right controllers
        private void UpdateHandStates(XRNodeState state)
        {
            List<XRNodeState> states = new List<XRNodeState>();
            InputTracking.GetNodeStates(states);

            bool leftAvailable = states.Any(x => x.nodeType == XRNode.LeftHand);
            bool rightAvailable = states.Any(x => x.nodeType == XRNode.RightHand);

            leftController.SetActive(leftAvailable);
            rightController.SetActive(rightAvailable);

            leftHandPositioner.deviceIndex = 9999;
            rightHandPositioner.deviceIndex = 9999;

            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                switch (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(i))
                {
                    case ETrackedControllerRole.LeftHand:
                        leftHandPositioner.deviceIndex = i;
                        break;
                    case ETrackedControllerRole.RightHand:
                        rightHandPositioner.deviceIndex = i;
                        break;
                }
            }

            if (FPWeapon)
            {
                dominantHand = VRConfig.SWITCH_HANDS ? XRNode.LeftHand : XRNode.RightHand;
                if ((dominantHand == XRNode.LeftHand && !leftAvailable) || (dominantHand == XRNode.RightHand && !rightAvailable))
                {
                    EntryPoint.ConsoleInstance.LogWarning("Dominant hand is unavailable. Switching hands ...");
                    dominantHand = dominantHand == XRNode.LeftHand ? XRNode.RightHand : XRNode.LeftHand;
                }

                SetVacHand();
            }
        }

        private void SetVacHand()
        {
            if (!FPWeapon)
            {
                EntryPoint.ConsoleInstance.LogWarning("Attempted to set vac hand before vac exists. Skipping ...");
                return;
            }

            if (dominantHand != XRNode.LeftHand && dominantHand != XRNode.RightHand)
            {
                EntryPoint.ConsoleInstance.LogWarning($"Dominant hand set to {dominantHand}, which is an invalid state! Cannot set vac hand.");
                return;
            }

            if (VRInput.Mode != SRInput.InputMode.PAUSE)
            {
                leftHand.SetActive(dominantHand == XRNode.RightHand);
                rightHand.SetActive(dominantHand == XRNode.LeftHand);
            }

            FPWeapon.SetParent(dominantHand == XRNode.LeftHand ? leftController.transform : rightController.transform, false);
            FPWeapon.localScale = new Vector3(dominantHand == XRNode.LeftHand ? -12.5f : 12.5f, 12.5f, 12.5f);
            FPWeapon.localPosition = new Vector3(dominantHand == XRNode.LeftHand ? 0.175f : -0.175f, 0.3f, -0.1f);
            FPWeapon.localRotation = Quaternion.Euler(45, 0, 0);

            UI.transform.SetParent(dominantHand == XRNode.LeftHand ? leftController.transform : rightController.transform, false);
            UI.transform.localRotation = dominantHand == XRNode.LeftHand ? Quaternion.Euler(34, 15f, 11) : Quaternion.Euler(34, -15, -11);
            UI.transform.localPosition = new Vector3(0f, 0.2563f, 0.1197f);

            UpdateVacVisibility();
        }
    }
}
