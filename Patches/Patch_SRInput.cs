using System.Collections.Generic;
using HarmonyLib;
using InControl;
using MonomiPark.SlimeRancher;
using MonomiPark.SlimeRancher.Persist;
using Valve.VR;

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
        
    }
}