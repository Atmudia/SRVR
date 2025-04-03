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

        [HarmonyPatch(typeof(UITemplates), nameof(UITemplates.GetButtonIcon)), HarmonyPrefix]
        public static bool GetButtonIcon(UITemplates __instance, string keyStr, ref bool iconFound, ref Sprite __result)
        {
            if (!SteamVR_Actions.slimecontrols.interact.active)
                return true;
            switch (keyStr)
            {
                case "RightTrigger":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.shoot[SteamVR_Input_Sources.Any],  out __result);
                    break;
                }
                case "LeftTrigger":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.vac[SteamVR_Input_Sources.Any],  out __result);
                    break;
                }
                case "Action1":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.jump[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "LeftStickButton":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.sprint[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "Action3":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.interact[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "Start":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.pause[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "RightStickButton":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.radar[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "DPadRight":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.map[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "DPadUp":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.slimepedia[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "LeftBumper":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.prevslot[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "RightBumper":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.nextslot[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                case "DPadDown":
                {
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.gadgetmode[SteamVR_Input_Sources.Any], out __result);
                    break;
                }
                default:
                {
                    return true;
                }
                    
            }
            return !iconFound;
        }
        

        
        
    }
}