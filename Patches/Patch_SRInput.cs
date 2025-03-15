using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InControl;
using MonomiPark.SlimeRancher;
using MonomiPark.SlimeRancher.Persist;
using UnityEngine;
using Valve.VR;
using Object = UnityEngine.Object;

namespace SRVR.Patches
{
    [HarmonyPatch]
    public static class Patch_SRInput
    {
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
                if (Patch_vp_FPWeapon.FPWeapon)
                {
                    Patch_vp_FPWeapon.FPWeapon.gameObject.SetActive(false);
                    Patch_vp_FPWeapon.FPWeapon.parent.Find("Right Hand").gameObject.SetActive(true);
                }

                // Activate the action if mode is DEFAULT
                SteamVR_Actions.slimecontrols.Activate();
            }
            else
            {
                if (Patch_vp_FPWeapon.FPWeapon)
                {
                    Patch_vp_FPWeapon.FPWeapon.gameObject.SetActive(true);
                    Patch_vp_FPWeapon.FPWeapon.parent.Find("Right Hand").gameObject.SetActive(false);

                }
                // Deactivate the action if mode is not DEFAULT
                SteamVR_Actions.slimecontrols.Deactivate();
            }
            
        }

        
        [HarmonyPatch(typeof(SavedProfile), nameof(SavedProfile.PullBindings)), HarmonyPrefix]
        public static bool PullBindings(ref BindingsV05 bindings, IEnumerable<PlayerAction> actions)
        {
            bindings = CachedBindings;
            return false;
        }

        public static BindingsV05 CachedBindings;
        [HarmonyPatch(typeof(SavedProfile), nameof(SavedProfile.PushBindings)), HarmonyPrefix]
        public static bool PushBindings(BindingsV05 bindingsData, InputDirector inputDir)
        {
            CachedBindings = bindingsData;
            inputDir.ResetKeyMouseDefaults();
            inputDir.ResetGamepadDefaults();
            return false;
        }

        [HarmonyPatch(typeof(UITemplates), nameof(UITemplates.GetButtonIcon)), HarmonyPrefix]
        public static bool GetButtonIcon(UITemplates __instance, InputDeviceStyle inputDevice, string keyStr, ref bool iconFound, ref Sprite __result)
        {
            if (!EntryPoint.EnabledVR)
                return true;
            if (!SteamVR_Actions.slimecontrols.interact.active)
                return true;
            switch (keyStr)
            {
                case "Action1":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.jump[SteamVR_Input_Sources.Any], out __result);
                    break;
                case "Action2":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.pulse[SteamVR_Input_Sources.Any], out __result);
                    break;
                case "Action3":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.interact[SteamVR_Input_Sources.Any], out __result);
                    break;
                case "DPadRight":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.map[SteamVR_Input_Sources.Any], out __result);
                    break;
                case "LeftStickButton":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.sprint[SteamVR_Input_Sources.Any], out __result);
                    break;
                case "LeftBumper":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.nextslot[SteamVR_Input_Sources.Any], out __result);
                    break;
                case "LeftTrigger":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.vac[SteamVR_Input_Sources.Any], out __result);
                    break;
                case "RightBumper":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.prevslot[SteamVR_Input_Sources.Any], out __result);
                    break;
                case "RightTrigger":
                    iconFound = VRInput.GetVRButton(__instance, SteamVR_Actions.slimecontrols.shoot[SteamVR_Input_Sources.Any], out __result);
                    break;
                default:
                {
                    return true;
                }
                    
            }
            return !iconFound;
        }
        

        
        
    }
}