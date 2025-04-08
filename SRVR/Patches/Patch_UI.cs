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

        public static GameObject VRPanel;

        [HarmonyPatch(typeof(OptionsUI), nameof(OptionsUI.SetupOptionsUI)), HarmonyPostfix]
        public static void SetupOptionsUI(OptionsUI __instance)
        {
            var vrTab = __instance.otherTab.Instantiate(__instance.otherTab.transform.parent);
            vrTab.name = "VRTab";
            vrTab.GetComponentInChildren<XlateText>().SetKey("b.vr");
            vrTab.transform.SetSiblingIndex(5);
            var vrPanel = __instance.otherPanel.InstantiateInactive(__instance.otherPanel.transform.parent).transform.Find("MiscPanel");
            vrPanel.parent.name = "VRPanel";
            VRPanel = vrPanel.transform.parent.gameObject;
            (vrTab.GetComponentInChildren<SRToggle>().onValueChanged = new Toggle.ToggleEvent()).AddListener((isOn) =>
            {
                __instance.DeselectAll();
                VRPanel.SetActive(true);
            });
            var uninstallObj = Object.Instantiate(vrPanel.Find("ResetProfileButton").gameObject, vrPanel.Find("ResetProfileButton").parent);
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
            uninstallObj.name = "UninstallVRButton VR";
            
            var leftHand = Object.Instantiate(vrPanel.Find("SprintHoldToggle").gameObject, vrPanel.Find("SprintHoldToggle").parent).transform;
            leftHand.name = "SwitchHandsButton VR";
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
            
            var snap_turn = Object.Instantiate(vrPanel.Find("SprintHoldToggle").gameObject, vrPanel.Find("SprintHoldToggle").parent).transform;
            snap_turn.name = "SnapTurnButton VR";
            snap_turn.SetSiblingIndex(8);
            snap_turn.GetComponentInChildren<XlateText>().SetKey("b.snap_turn");
            var srToggleSnap = snap_turn.GetComponentInChildren<SRToggle>();
            (srToggleSnap.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.SNAP_TURN = arg0;
                VRConfig.SaveConfig();
            });
            srToggleSnap.isOn = VRConfig.SNAP_TURN;
            
            
            var distanceGrab = Object.Instantiate(vrPanel.Find("SprintHoldToggle").gameObject, vrPanel.Find("SprintHoldToggle").parent).transform;
            distanceGrab.name = "DistanceGrabButton VR";

            distanceGrab.SetSiblingIndex(10);
            distanceGrab.GetComponentInChildren<XlateText>().SetKey("b.distance_grab");
            var srToggleGrab = distanceGrab.GetComponentInChildren<SRToggle>();
            (srToggleGrab.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.DISTANCE_GRAB = arg0;
                VRConfig.SaveConfig();
            });
            srToggleGrab.isOn = VRConfig.DISTANCE_GRAB;
            
            var pediaToggle = Object.Instantiate(vrPanel.Find("SprintHoldToggle").gameObject, vrPanel.Find("SprintHoldToggle").parent).transform;
            pediaToggle.name = "PediaToggleButton VR";

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

            var snapTurnAngle = Object.Instantiate(vrPanel.Find("OverscanRow").gameObject, vrPanel.Find("OverscanRow").parent);
            snapTurnAngle.name = "SnapTurnAngle VR";
            snapTurnAngle.transform.SetSiblingIndex(13);
            var slider = snapTurnAngle.GetComponentInChildren<Slider>();
            snapTurnAngle.GetComponentInChildren<XlateText>().SetKey("b.snap_turn_angle");
            var sliderEvent = slider.onValueChanged = new Slider.SliderEvent();
            slider.minValue = 1;
            slider.maxValue = 5;
            var snapTurnAngleValueLabel = snapTurnAngle.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            sliderEvent.AddListener(value =>
            {
                VRConfig.SNAP_TURN_ANGLE = ((int)value + 1) * 15;
                snapTurnAngleValueLabel.text = VRConfig.SNAP_TURN_ANGLE.ToString(); 
                VRConfig.SaveConfig();
               
            });
            slider.value = (VRConfig.SNAP_TURN_ANGLE / 15) - 1;

            foreach (Transform vrElements in vrPanel)
            {
                if (!vrElements.name.Contains(" VR"))
                {
                    Object.Destroy(vrElements.gameObject);
                }
            }

            __instance.SetupVertNav(srToggleHand, srToggleSnap, srToggleGrab, srTogglePedia, slider, uninstallObj.GetComponentInChildren<Button>(true));
        }

        [HarmonyPatch(typeof(OptionsUI), nameof(OptionsUI.DeselectAll)), HarmonyPostfix]
        public static void DeselectAll(OptionsUI __instance) => VRPanel.Safe()?.SetActive(false);
        
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

        [HarmonyPatch(typeof(AmmoSlotUI), nameof(AmmoSlotUI.Start)), HarmonyPostfix]
        public static void AmmoStart(AmmoSlotUI __instance) => __instance.selected.transform.localRotation = Quaternion.identity;

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