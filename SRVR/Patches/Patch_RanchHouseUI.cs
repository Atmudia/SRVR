using HarmonyLib;
using UnityEngine;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(RanchHouseUI))]
    internal class Patch_RanchHouseUI
    {
        public static RanchHouseUI Instance;
        [HarmonyPostfix, HarmonyPatch(nameof(RanchHouseUI.Awake))]
        public static void Awake(RanchHouseUI __instance)
        {
            if (!EntryPoint.EnabledVR) 
                return;
            Instance = __instance;
            __instance.mainUI.transform.Find("BackgroundSizer").transform.localPosition = new Vector3(0, 0, -0.1f);
            __instance.mainUI.transform.Find("UIContainer").transform.localPosition = new Vector3(0, 0, -0.2f);
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(DeathObscurer), nameof(DeathObscurer.OnLocked))]
        public static bool OnLocked()
        {
            if (!EntryPoint.EnabledVR) 
                return true;
            if (Instance && Instance.sleeping)
            {
                return false;
            }
            return true;
        }
    }
    
    
}