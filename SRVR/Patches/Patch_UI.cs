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
    internal class Patch_UI
    {
        
        private static int[] PresetSnapTurnAngles = {30, 45, 60, 90};
        
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
            __instance.ambientOcclusionToggle.transform.parent.gameObject.SetActive(false);
            __instance.fovSlider.transform.parent.gameObject.SetActive(false);
        }
        [HarmonyPatch(typeof(OptionsUI), nameof(OptionsUI.SetupOtherOptions)), HarmonyPostfix]
        public static void SetupOtherOptions(OptionsUI __instance)
        {
            var uninstallObj = Object.Instantiate(__instance.resetProfileButton.gameObject, __instance.resetProfileButton.transform.parent);
            uninstallObj.GetComponentInChildren<XlateText>().SetKey("b.uninstall_srvr");
            (uninstallObj.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent()).AddListener(delegate
            {
                SRSingleton<GameContext>.Instance.UITemplates.CreateConfirmDialog("Are you sure you want to uninstall SRVR?", () =>
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

                });
            });
            uninstallObj.name = "UninstallVRButton";
            var leftHand = Object.Instantiate(__instance.sprintHoldToggle.gameObject, __instance.sprintHoldToggle.transform.parent).transform;
            leftHand.SetSiblingIndex(7);
            leftHand.GetComponentInChildren<XlateText>().SetKey("b.switch_hands");
            var srToggleHand = leftHand.GetComponentInChildren<SRToggle>();
            (srToggleHand.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.SWITCH_HANDS = arg0;
                if (HandManager.Instance && EntryPoint.EnabledVR)
                {
                    HandManager.Instance.dominantHand = VRConfig.SWITCH_HANDS ? XRNode.LeftHand : XRNode.RightHand;
                    HandManager.Instance.UpdateHandStates();
                }
                VRConfig.SaveConfig();
            });
            srToggleHand.isOn = VRConfig.SWITCH_HANDS;
            
            var snap_turn = Object.Instantiate(__instance.sprintHoldToggle.gameObject, __instance.sprintHoldToggle.transform.parent).transform;
            snap_turn.SetSiblingIndex(8);
            snap_turn.GetComponentInChildren<XlateText>().SetKey("b.snap_turn");
            var srToggleSnap = snap_turn.GetComponentInChildren<SRToggle>();
            (srToggleSnap.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.SNAP_TURN = arg0;
                VRConfig.SaveConfig();
            });
            srToggleSnap.isOn = VRConfig.SNAP_TURN;
            
            var distanceGrab = Object.Instantiate(__instance.sprintHoldToggle.gameObject, __instance.sprintHoldToggle.transform.parent).transform;
            distanceGrab.SetSiblingIndex(10);
            distanceGrab.GetComponentInChildren<XlateText>().SetKey("b.distance_grab");
            var srToggleGrab = distanceGrab.GetComponentInChildren<SRToggle>();
            (srToggleGrab.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.DISTANCE_GRAB = arg0;
                VRConfig.SaveConfig();
            });
            srToggleGrab.isOn = VRConfig.DISTANCE_GRAB;
            
            var pediaToggle = Object.Instantiate(__instance.sprintHoldToggle.gameObject, __instance.sprintHoldToggle.transform.parent).transform;
            pediaToggle.SetSiblingIndex(11);
            pediaToggle.GetComponentInChildren<XlateText>().SetKey("b.pedia_toggle");
            var srTogglePedia = pediaToggle.GetComponentInChildren<SRToggle>();
            (srTogglePedia.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.PEDIA_ON_VAC = arg0;

                if (HandManager.Instance?.pedia)
                    HandManager.Instance.pedia.SetActive(VRConfig.PEDIA_ON_VAC);

                VRConfig.SaveConfig();
            });
            srTogglePedia.isOn = VRConfig.DISTANCE_GRAB;

            var snapTurnAngle = Object.Instantiate(__instance.overscanFovRow, __instance.overscanFovRow.transform.parent);
            __instance.overscanFovRow.SetActive(false);
            snapTurnAngle.transform.SetSiblingIndex(13);
            var slider = snapTurnAngle.GetComponentInChildren<Slider>();
            snapTurnAngle.GetComponentInChildren<XlateText>().SetKey("b.snap_turn_angle");
            var sliderEvent = slider.onValueChanged = new Slider.SliderEvent();
            slider.minValue = 30;
            slider.maxValue = 90;
            var snapTurnAngleValueLabel = snapTurnAngle.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            sliderEvent.AddListener(value =>
            {
                int closest = PresetSnapTurnAngles[0];
                foreach (int angle in PresetSnapTurnAngles)
                {
                    if (Mathf.Abs(value - angle) < Mathf.Abs(value - closest))
                    {
                        closest = angle;
                    }
                }
                VRConfig.SNAP_TURN_ANGLE = closest;
                slider.SetValueWithoutNotify(closest);  // Snap the UI slider to the closest preset
                snapTurnAngleValueLabel.text = closest.ToString(); 
                VRConfig.SaveConfig();
               
            });
            slider.value = VRConfig.SNAP_TURN_ANGLE;
            
           
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
        [HarmonyPatch(typeof(MapUI), nameof(MapUI.Awake)), HarmonyPrefix]
        public static void Awake(MapUI __instance)
        {
            ((RectTransform)__instance.transform.Find("UIContainer/Panel/MapControls/Compass")).anchoredPosition3D = Vector3.zero;
            var icon = (RectTransform)__instance.treasurePodCountLine.transform.Find("Icon");
            icon.anchoredPosition3D = icon.anchoredPosition3D with { z = 0 };
        }

        [HarmonyPatch(typeof(IntroUI), nameof(IntroUI.Awake)), HarmonyPrefix]
        public static void Awake(IntroUI __instance)
        {
            __instance.background.transform.localScale = Vector3.one * 99f;
            HandManager.Instance.rightController.gameObject.SetActive(false);
            HandManager.Instance.leftController.gameObject.SetActive(false);
            __instance.onDestroy += () =>
            {
                HandManager.Instance.rightController.gameObject.SetActive(true);
                HandManager.Instance.leftController.gameObject.SetActive(true);
            };
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