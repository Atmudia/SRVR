using HarmonyLib;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(RanchHouseUI))]
    public class Patch_RanchHouseUI
    {
        [HarmonyPostfix, HarmonyPatch(nameof(RanchHouseUI.Awake))]
        public static void Awake(RanchHouseUI __instance)
        {
            __instance.mainUI.transform.Find("BackgroundSizer").transform.localPosition = new Vector3(0, 0, -0.1f);
            __instance.mainUI.transform.Find("UIContainer").transform.localPosition = new Vector3(0, 0, -0.2f);
        }
    }
}