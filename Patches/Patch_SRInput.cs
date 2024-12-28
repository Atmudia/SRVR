using System;
using HarmonyLib;
using Valve.VR;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(SRInput))]
    public static class Patch_SRInput
    {
        [HarmonyPatch(typeof(SRInput), nameof(SRInput.SetInputMode), typeof(SRInput.InputMode)), HarmonyPrefix]
        public static void SetInputMode(SRInput.InputMode mode)
        {
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
                if (Patch_HudUI.FPWeapon)
                {
                    Patch_HudUI.FPWeapon.SetActive(false);
                }

                // Activate the action if mode is DEFAULT
                SteamVR_Actions.slimecontrols.Activate();
            }
            else
            {
                if (Patch_HudUI.FPWeapon)
                {
                    Patch_HudUI.FPWeapon.SetActive(true);
                }
                // Deactivate the action if mode is not DEFAULT
                SteamVR_Actions.slimecontrols.Deactivate();
            }
            
        }
        
    }
}