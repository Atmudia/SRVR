using System;
using HarmonyLib;
using SRML.Console.Commands;
using UnityEngine;
using UnityEngine.Events;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(RanchHouseUI))]
    public class Patch_RanchHouseUI
    {
        public static RanchHouseUI Instance;
        [HarmonyPostfix, HarmonyPatch(nameof(RanchHouseUI.Awake))]
        public static void Awake(RanchHouseUI __instance)
        {
            Instance = __instance;
            __instance.mainUI.transform.Find("BackgroundSizer").transform.localPosition = new Vector3(0, 0, -0.1f);
            __instance.mainUI.transform.Find("UIContainer").transform.localPosition = new Vector3(0, 0, -0.2f);
        }
        
        

        [HarmonyPostfix, HarmonyPatch(nameof(RanchHouseUI.OnButtonDLC))]
        public static void OnButtonDLC(RanchHouseUI __instance)
        {
            __instance.currDLCManageUI.transform.localPosition += new Vector3(0, 0, 0.1f);
            __instance.currDLCManageUI.onDestroy += () =>
            {
                __instance.beatrixImg.enabled = true;
            };
            __instance.beatrixImg.enabled = false;
        }
        [HarmonyPostfix, HarmonyPatch(nameof(RanchHouseUI.Mail))]
        public static void Mail(RanchHouseUI __instance)
        {
            __instance.currMailUI.transform.localPosition += new Vector3(0, 0, 0.1f);
            __instance.currMailUI.onDestroy += () =>
            {
                __instance.beatrixImg.enabled = true;
            };
            __instance.beatrixImg.enabled = false;
        }
        [HarmonyPostfix, HarmonyPatch(nameof(RanchHouseUI.CorporatePartner))]
        public static void CorporatePartner(RanchHouseUI __instance)
        {
            __instance.currPartnerUI.transform.localPosition += new Vector3(0, 0, 0.1f);
            __instance.currPartnerUI.onDestroy += () =>
            {
                __instance.beatrixImg.enabled = true;
            };
            __instance.beatrixImg.enabled = false;
        }
        [HarmonyPostfix, HarmonyPatch(nameof(RanchHouseUI.OnButtonAppearances))]
        public static void OnButtonAppearances(RanchHouseUI __instance)
        {
            __instance.currAppearanceUI.transform.localPosition += new Vector3(0, 0, 0.1f);
            __instance.currAppearanceUI.onDestroy += () =>
            {
                __instance.beatrixImg.enabled = true;
            };
            __instance.beatrixImg.enabled = false;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(DeathObscurer), nameof(DeathObscurer.OnLocked))]
        public static bool OnLocked()
        {
            
            if (Instance && Instance.sleeping)
            {
                return false;
            }
            return true;
        }
    }
    
    
}