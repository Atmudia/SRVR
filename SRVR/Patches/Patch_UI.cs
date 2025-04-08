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
using Valve.VR.InteractionSystem;

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

            /*var leftHand = Object.Instantiate(vrPanel.Find("SprintHoldToggle").gameObject, vrPanel.Find("SprintHoldToggle").parent).transform;
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
            srToggleHand.isOn = VRConfig.SWITCH_HANDS;*/

            vrPanel.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
            vrPanel.GetComponent<VerticalLayoutGroup>().childControlWidth = false;

            var distanceGrab = Object.Instantiate(vrPanel.Find("SprintHoldToggle").gameObject, vrPanel.Find("SprintHoldToggle").parent).transform;
            distanceGrab.name = "DistanceGrabButton VR";

            distanceGrab.SetSiblingIndex(8);
            distanceGrab.GetComponentInChildren<XlateText>().SetKey("b.distance_grab");
            var srToggleGrab = distanceGrab.GetComponentInChildren<SRToggle>();
            (srToggleGrab.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.DISTANCE_GRAB = arg0;
                VRConfig.SaveConfig();
            });
            srToggleGrab.isOn = VRConfig.DISTANCE_GRAB;

            GameObject dominantHandGroup = new GameObject("DominantHandGroup VR")
            {
                transform =
                {
                    parent = vrPanel.Find("SprintHoldToggle").parent
                }
            };
            dominantHandGroup.SetActive(false);

            RectTransform dominantRT = dominantHandGroup.AddComponent<RectTransform>();
            dominantRT.SetSiblingIndex(7);
            LayoutElement dominantLayout = dominantHandGroup.AddComponent<LayoutElement>();
            dominantLayout.preferredHeight = 50;
            dominantLayout.preferredWidth = 650;
            HorizontalLayoutGroup dominantHandLayout = dominantHandGroup.AddComponent<HorizontalLayoutGroup>();
            dominantHandLayout.spacing = 16;
            dominantHandLayout.childAlignment = TextAnchor.MiddleCenter;
            dominantHandLayout.childControlHeight = true;
            dominantHandLayout.childControlWidth = true;
            dominantHandLayout.childForceExpandHeight = false;
            dominantHandLayout.childForceExpandHeight = false;
            SRToggleGroup dominantToggleGroup = dominantHandGroup.AddComponent<SRToggleGroup>();
            dominantToggleGroup.allowSwitchOff = false;

            dominantRT.sizeDelta = new Vector2(650, 50);

            GameObject dominantHandLabel = Object.Instantiate(vrPanel.Find("SprintHoldToggle").gameObject.GetComponentInChildren<XlateText>().gameObject, dominantRT);
            dominantHandLabel.GetComponent<XlateText>().SetKey("b.dominant_hand");
            dominantHandLabel.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            LayoutElement dominantHandLabelLayout = dominantHandLabel.AddComponent<LayoutElement>();
            dominantHandLabelLayout.preferredHeight = 50;
            dominantHandLabelLayout.preferredWidth = 250;

            GameObject leftHandButton = Object.Instantiate(vrTab, dominantRT);
            leftHandButton.name = "Left Hand";
            leftHandButton.GetComponentInChildren<XlateText>().SetKey("b.left_handed");
            SRToggle leftHandToggle = leftHandButton.GetComponentInChildren<SRToggle>();
            leftHandToggle.group = dominantToggleGroup;
            (leftHandToggle.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                if (!arg0)
                    return;

                VRConfig.SWITCH_HANDS = true;
                if (HandManager.Instance && EntryPoint.EnabledVR)
                {
                    HandManager.Instance.dominantHand = XRNode.LeftHand;
                    HandManager.Instance.UpdateHandStates();
                }
                VRConfig.SaveConfig();
            });
            leftHandToggle.SetIsOnWithoutNotify(VRConfig.SWITCH_HANDS);
            LayoutElement leftHandLayout = leftHandToggle.GetComponent<LayoutElement>();
            leftHandLayout.preferredHeight = 50;
            leftHandLayout.preferredWidth = 250;

            GameObject rightHandButton = Object.Instantiate(vrTab, dominantRT);
            rightHandButton.name = "Right Hand";
            rightHandButton.GetComponentInChildren<XlateText>().SetKey("b.right_handed");
            SRToggle rightHandToggle = rightHandButton.GetComponentInChildren<SRToggle>();
            rightHandToggle.group = dominantToggleGroup;
            (rightHandToggle.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                if (!arg0)
                    return;

                VRConfig.SWITCH_HANDS = false;
                if (HandManager.Instance && EntryPoint.EnabledVR)
                {
                    HandManager.Instance.dominantHand = XRNode.RightHand;
                    HandManager.Instance.UpdateHandStates();
                }
                VRConfig.SaveConfig();
            });
            rightHandToggle.SetIsOnWithoutNotify(!VRConfig.SWITCH_HANDS);
            LayoutElement rightHandLayout = rightHandToggle.GetComponent<LayoutElement>();
            rightHandLayout.preferredHeight = 50;
            rightHandLayout.preferredWidth = 250;

            dominantHandGroup.SetActive(true);

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

            var turnSensitivity = Object.Instantiate(vrPanel.Find("OverscanRow").gameObject, vrPanel.Find("OverscanRow").parent);
            turnSensitivity.name = "TurnSensitivity VR";
            turnSensitivity.transform.SetSiblingIndex(14);
            var sensSlider = turnSensitivity.GetComponentInChildren<Slider>();
            turnSensitivity.GetComponentInChildren<XlateText>().SetKey("b.turn_sensitivity");
            var sensSliderEvent = sensSlider.onValueChanged = new Slider.SliderEvent();
            sensSlider.minValue = 1;
            sensSlider.maxValue = 10;
            sensSlider.wholeNumbers = false;
            var sensValueLabel = turnSensitivity.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            sensSliderEvent.AddListener(value =>
            {
                VRConfig.TURN_SENSITIVITY = value;
                sensValueLabel.text = VRConfig.TURN_SENSITIVITY.ToString();
                VRConfig.SaveConfig();
            });
            sensSlider.value = VRConfig.TURN_SENSITIVITY;

            turnSensitivity.SetActive(!VRConfig.SNAP_TURN);
            snapTurnAngle.SetActive(VRConfig.SNAP_TURN);

            var heightAdjust = Object.Instantiate(vrPanel.Find("OverscanRow").gameObject, vrPanel.Find("OverscanRow").parent);
            heightAdjust.name = "HeightAdjust VR";
            heightAdjust.transform.SetSiblingIndex(15);
            var heightSlider = heightAdjust.GetComponentInChildren<Slider>();
            heightAdjust.GetComponentInChildren<XlateText>().SetKey("b.height_adjust");
            var heightSliderEvent = heightSlider.onValueChanged = new Slider.SliderEvent();
            heightSlider.minValue = -1.5f;
            heightSlider.maxValue = 1.5f;
            heightSlider.wholeNumbers = false;
            var heightValueLabel = heightAdjust.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            heightSliderEvent.AddListener(value =>
            {
                VRConfig.HEIGHT_ADJUSTMENT = value;
                heightValueLabel.text = VRConfig.HEIGHT_ADJUSTMENT.ToString();
                VRConfig.SaveConfig();
            });
            heightSlider.value = VRConfig.HEIGHT_ADJUSTMENT;

            var snap_turn = Object.Instantiate(vrPanel.Find("SprintHoldToggle").gameObject, vrPanel.Find("SprintHoldToggle").parent).transform;
            snap_turn.name = "SnapTurnButton VR";
            snap_turn.SetSiblingIndex(8);
            snap_turn.GetComponentInChildren<XlateText>().SetKey("b.snap_turn");
            var srToggleSnap = snap_turn.GetComponentInChildren<SRToggle>();
            (srToggleSnap.onValueChanged = new Toggle.ToggleEvent()).AddListener(arg0 =>
            {
                VRConfig.SNAP_TURN = arg0;

                turnSensitivity.SetActive(!VRConfig.SNAP_TURN);
                snapTurnAngle.SetActive(VRConfig.SNAP_TURN);
                SetupVertNav(leftHandToggle, srToggleSnap, srToggleGrab, srTogglePedia, VRConfig.SNAP_TURN ? slider : sensSlider, heightSlider, uninstallObj.GetComponentInChildren<Button>(true));

                VRConfig.SaveConfig();
            });
            srToggleSnap.isOn = VRConfig.SNAP_TURN;

            leftHandToggle.navigation = leftHandToggle.navigation with
            {
                mode = Navigation.Mode.Explicit,
                selectOnRight = rightHandToggle
            };
            rightHandToggle.navigation = rightHandToggle.navigation with
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = leftHandToggle,
                selectOnDown = srToggleSnap,
            };

            foreach (Transform vrElements in vrPanel)
            {
                if (!vrElements.name.Contains(" VR"))
                {
                    Object.Destroy(vrElements.gameObject);
                }
            }

            SetupVertNav(leftHandToggle, srToggleSnap, srToggleGrab, srTogglePedia, VRConfig.SNAP_TURN ? slider : sensSlider, heightSlider, uninstallObj.GetComponentInChildren<Button>(true));
        }

        private static void SetupVertNav(params Selectable[] selectables)
        {
            List<Selectable> selectableList = new List<Selectable>();
            foreach (Selectable selectable in selectables)
            {
                if (selectable.gameObject.activeSelf)
                    selectableList.Add(selectable);
            }
            for (int index = 0; index < selectableList.Count; ++index)
            {
                Navigation navigation = selectableList[index].navigation with
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = index != 0 ? selectableList[index - 1] : null,
                    selectOnDown = index != selectableList.Count - 1 ? selectableList[index + 1] : null
                };
                selectableList[index].navigation = navigation;
            }
            if (selectableList.Count <= 0)
                return;
            if (!selectableList[0].GetComponent<InitSelected>())
                selectableList[0].gameObject.AddComponent<InitSelected>();
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

        private static int lastSlot = -1;

        [HarmonyPatch(typeof(AmmoSlotUI), nameof(AmmoSlotUI.Awake)), HarmonyPostfix]
        public static void AmmoAwake(AmmoSlotUI __instance) => lastSlot = -1;
        [HarmonyPatch(typeof(AmmoSlotUI), nameof(AmmoSlotUI.Update)), HarmonyPostfix]
        public static void AmmoUpdate(AmmoSlotUI __instance)
        {
            int selectedAmmoIdx = __instance.player.Ammo.GetSelectedAmmoIdx();
            if (lastSlot != selectedAmmoIdx)
            {
                __instance.selected.transform.localRotation = Quaternion.identity;
                __instance.slots[selectedAmmoIdx].front.color = Color.yellow;
                if (lastSlot > -1)
                    __instance.slots[lastSlot].front.color = Color.white;
                
                lastSlot = selectedAmmoIdx;
            }
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