using HarmonyLib;

namespace SRVR.Patches
{
    // [HarmonyPatch(typeof(GamepadPanel))]
    // public class Patch_GamepadPanel
    // {
    //     [HarmonyPatch(nameof(GamepadPanel.Update))]
    //     public static bool Update(GamepadPanel __instance)
    //     {
    //         bool flag = __instance.inputDir.UsingSteamController();
    //         __instance.standardPanel.SetActive(!flag);
    //         __instance.steamPanel.SetActive(flag);
    //         __instance.defaultGamepadVisualPanel.gameObject.SetActive(true);
    //         __instance.ps4GamepadVisualPanel.gameObject.SetActive(false);
    //         return false;
    //     }
    //     [HarmonyPatch(nameof(GamepadPanel.Awake))]
    //     public static void Awake(GamepadPanel __instance)
    //     {
    //         __instance
    //     }
    // }
}