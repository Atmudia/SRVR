using System.Collections.Generic;
using HarmonyLib;
using InControl;
using MonomiPark.SlimeRancher;
using MonomiPark.SlimeRancher.Persist;
using SRVR.Components;
using UnityEngine;
using Valve.VR;

namespace SRVR.Patches
{
    [HarmonyPatch]
    internal static class Patch_Input
    {
        private static readonly int UILayer = LayerMask.NameToLayer("UI");
        private static readonly int WeaponLayer = LayerMask.NameToLayer("Weapon");

        [HarmonyPatch(typeof(SRInput), nameof(SRInput.SetInputMode), typeof(SRInput.InputMode)), HarmonyPrefix]
        public static void SetInputMode(SRInput.InputMode mode)
        {
            if (!EntryPoint.EnabledVR)
                return;

            EntryPoint.ConsoleInstance.Log("SetInputMode: " + mode);
            VRInput.Mode = mode;

            // Activate or deactivate SteamVR action based on mode
            if (mode == SRInput.InputMode.DEFAULT)
            {
                // Activate the action if mode is DEFAULT
                SteamVR_Actions.slimecontrols.Activate();
            }
            else
            {
                // Deactivate the action if mode is not DEFAULT
                SteamVR_Actions.slimecontrols.Deactivate();
            }
            if (mode == SRInput.InputMode.PAUSE)
            {
                SteamVR_Actions.ui.Activate();

                HandManager.Instance?.leftPickuper?.Drop(false);
                HandManager.Instance?.rightPickuper?.Drop(false);

                if (HandManager.Instance?.leftHandModel)
                {
                    HandManager.Instance.leftHandModel.layer = UILayer;
                    HandManager.Instance.rightHandModel.layer = UILayer;
                }

                VRInput.repauseDelay = VRInput.REPAUSE_DELAY;
                HandManager.Instance?.UpdateVacVisibility();
            }
            else
            {
                SteamVR_Actions.ui.Deactivate();

                if (HandManager.Instance?.leftHandModel)
                {
                    HandManager.Instance.leftHandModel.layer = WeaponLayer;
                    HandManager.Instance.rightHandModel.layer = WeaponLayer;
                }

                VRInput.repauseDelay = VRInput.REPAUSE_DELAY;
                HandManager.Instance?.UpdateVacVisibility();
            }
            if (Levels.IsLevel(Levels.WORLD))
            {
                SceneContext.Instance?.PopupDirector?.currPopup?.Safe()?.GetProperty<GameObject>("gameObject").Safe()?.SetActive(mode != SRInput.InputMode.PAUSE);
                SceneContext.Instance?.TutorialDirector?.currPopup?.Safe()?.gameObject.Safe()?.SetActive(mode != SRInput.InputMode.PAUSE);
                SceneContext.Instance?.AchievementsDirector?.currPopup?.Safe()?.gameObject.Safe()?.SetActive(mode != SRInput.InputMode.PAUSE);
                SceneContext.Instance?.AchievementsDirector?.currPopup?.Safe()?.gameObject.Safe()?.SetActive(mode != SRInput.InputMode.PAUSE);

            }
        }

        
        [HarmonyPatch(typeof(SavedProfile), nameof(SavedProfile.PullBindings)), HarmonyPrefix]
        public static bool PullBindings(ref BindingsV05 bindings)
        {
            if (!EntryPoint.EnabledVR)
                return true;
            bindings = CachedBindings;
            return false;
        }

        public static BindingsV05 CachedBindings;
        [HarmonyPatch(typeof(SavedProfile), nameof(SavedProfile.PushBindings)), HarmonyPrefix]
        public static bool PushBindings(BindingsV05 bindingsData, InputDirector inputDir)
        {
            if (!EntryPoint.EnabledVR)
                return true;
            CachedBindings = bindingsData;
            inputDir.ResetKeyMouseDefaults();
            inputDir.ResetGamepadDefaults();
            return false;
        }

        [HarmonyPatch(typeof(InputDirector), "GetActiveDeviceString"), HarmonyPrefix]
        public static bool ActiveDeviceStringPatch(string actionStr, ref string __result)
        {
            // will be overwritten if not in VR
            __result = actionStr;
            return !EntryPoint.EnabledVR;
        }


        [HarmonyPatch(typeof(InputDirector), nameof(InputDirector.GetDefaultDeviceIcon)), HarmonyPrefix]
        public static bool GetButtonIcon(InputDirector __instance, string actionStr, ref bool iconFound, ref Sprite __result)
        {
            if (!EntryPoint.EnabledVR)
                return true;

            iconFound = VRInput.GetVRButton(actionStr, out __result);
            return !iconFound;
        }
    }
}