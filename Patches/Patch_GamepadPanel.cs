using HarmonyLib;
using TMPro;
using Valve.VR;

namespace SRVR.Patches
{
    [HarmonyPatch]
    public class Patch_Panel
    {
        [HarmonyPatch(typeof(GamepadPanel), nameof(GamepadPanel.Update)), HarmonyPrefix]
        public static bool Update(GamepadPanel __instance)
        {
            return false;
        }
        [HarmonyPatch(typeof(GamepadPanel), nameof(GamepadPanel.Awake)), HarmonyPrefix]
        public static void GamepadAwake(GamepadPanel __instance)
        {
            __instance.standardPanel.SetActive(false);
            __instance.steamPanel.SetActive(true);
            __instance.steamPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Open SteamVR Bindings";
            __instance.defaultGamepadVisualPanel.gameObject.SetActive(true);
            __instance.ps4GamepadVisualPanel.gameObject.SetActive(false);
        }
        [HarmonyPatch(typeof(GamepadPanel), nameof(GamepadPanel.ShowSteamControllerConfig)), HarmonyPrefix]
        public static bool ShowSteamControllerConfig()
        {
            SteamVR_Input.OpenBindingUI();
            return false;
        }
    }
}