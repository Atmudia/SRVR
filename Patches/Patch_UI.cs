using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SRVR.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

namespace SRVR.Patches
{
    [HarmonyPatch]
    public class Patch_UI
    {
        [HarmonyPatch(typeof(GamepadPanel), nameof(GamepadPanel.Update)), HarmonyPrefix]
        public static bool Update() => !EntryPoint.EnabledVR;
        
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
        [HarmonyPatch(typeof(DLCManageUI), nameof(DLCManageUI.Awake)), HarmonyPrefix]
        public static void Awake(DLCManageUI __instance) => __instance.gameObject.RemoveComponent<Canvas>();

        [HarmonyPatch(typeof(OptionsUI), nameof(OptionsUI.SetupVideoSettings)), HarmonyPostfix]
        public static void SetupVideoSettings(OptionsUI __instance)
        {
            // if (EntryPoint.EnabledVR)
            // {
                __instance.ambientOcclusionToggle.transform.parent.gameObject.SetActive(false);
                __instance.fovSlider.transform.parent.gameObject.SetActive(false);
            // }
        }
        [HarmonyPatch(typeof(OptionsUI), nameof(OptionsUI.SetupOtherOptions)), HarmonyPostfix]
        public static void SetupOtherOptions(OptionsUI __instance)
        {
            var uninstallObj = Object.Instantiate(__instance.resetProfileButton.gameObject, __instance.resetProfileButton.transform.parent);
            uninstallObj.GetComponentInChildren<XlateText>().SetKey("b.uninstall_srvr");
            (uninstallObj.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent()).AddListener(delegate
            {
                SRSingleton<GameContext>.Instance.UITemplates.CreateConfirmDialog("Are you sure you want to uninstall SRVR?", (ConfirmUI.OnConfirm) (() =>
                {
                    var uninstall = VRInstaller.Uninstall();
                    if (uninstall == null)
                    {
                        Application.Quit();
                    }
                    else
                    {
                        SRSingleton<GameContext>.Instance.UITemplates.CreateErrorDialog($"Uninstallation Error: {uninstall.ToString()}");
                    }
                    string installDirectory = new DirectoryInfo(Application.dataPath).Parent!.FullName;
                    Process.Start(VRInstaller.VRInstallerPath, $"\"{installDirectory}\" uninstall");
                }));
            });
            uninstallObj.name = "UninstallVRButton";
            var leftHand = Object.Instantiate(__instance.sprintHoldToggle.gameObject, __instance.sprintHoldToggle.transform.parent).transform;
            leftHand.SetSiblingIndex(7);
            leftHand.GetComponentInChildren<XlateText>().SetKey("b.switch_hands");
            var srToggleHand = leftHand.GetComponentInChildren<SRToggle>();
            (srToggleHand.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.SWITCH_HANDS = arg0;
                if (HandManager.Instance)
                {
                    HandManager.Instance.dominantHand = VRConfig.SWITCH_HANDS ? XRNode.LeftHand : XRNode.RightHand;
                    HandManager.Instance.UpdateHandStates();
                }
                VRConfig.SaveConfig();
            });
            srToggleHand.isOn = VRConfig.SWITCH_HANDS;
            
            var snapturn = Object.Instantiate(__instance.sprintHoldToggle.gameObject, __instance.sprintHoldToggle.transform.parent).transform;
            snapturn.SetSiblingIndex(8);
            snapturn.GetComponentInChildren<XlateText>().SetKey("b.snapturn");
            var srToggleSnap = snapturn.GetComponentInChildren<SRToggle>();
            (srToggleSnap.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.SNAP_TURN = arg0;
                VRConfig.SaveConfig();
            });
            srToggleSnap.isOn = VRConfig.SNAP_TURN;
            
        }

        [HarmonyPatch(typeof(OptionsUI), nameof(OptionsUI.SetupVertNav)), HarmonyPrefix]
        public static void SetupVertNav(OptionsUI __instance, ref Selectable[] selectables) =>  selectables = __instance.sprintHoldToggle.transform.parent.GetComponentsInChildren<Selectable>(); 
        
        
        

        [HarmonyPatch(typeof(DisableEffectsOnLowQuality), nameof(DisableEffectsOnLowQuality.CheckQuality)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CheckQuality(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToArray();
            foreach (var codeInstruction in codeInstructions)
            {
                if (codeInstruction.opcode == OpCodes.Call && codeInstruction.operand is MethodInfo { Name: "GetDepthTextureMode" })
                {
                    EntryPoint.ConsoleInstance.Log("Changed GetDepthTextureMode to DepthTextureModeAlternate");
                    codeInstruction.operand = typeof(Patch_UI).GetMethod(nameof(GetDepthTextureModeAlternate));
                }

                if (codeInstruction.opcode == OpCodes.Call &&
                    codeInstruction.operand is MethodInfo { Name: "get_AmbientOcclusion" })
                {
                    EntryPoint.ConsoleInstance.Log("Changed get_AmbientOcclusion to GetAmbientOcclusionAlternate");
                    codeInstruction.operand = typeof(Patch_UI).GetMethod(nameof(GetAmbientOcclusionAlternate));

                }
            }
            return codeInstructions;
        }

        public static DepthTextureMode GetDepthTextureModeAlternate()
        {
            if (EntryPoint.EnabledVR)
                return DepthTextureMode.Depth;
            else
                return SRQualitySettings.GetDepthTextureMode();
        }
        public static bool GetAmbientOcclusionAlternate()
        {
            if (EntryPoint.EnabledVR)
                return false;
            else
                return SRQualitySettings.AmbientOcclusion;
        }
    }
}