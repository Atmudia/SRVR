using System;
using InControl;
using SRVR.Components;
using SRVR.Patches;
using Steamworks;
using UnityEngine;
using Valve.VR;
using InputDevice = InControl.InputDevice;

namespace SRVR
{
    public class VRInput : InputDevice
    {
        public static VRInput Instance;
        public static SRInput.InputMode Mode;
        public static float repauseDelay = 0.0f;

        public const float REPAUSE_DELAY = 0.25f;

        public VRInput()
        {
            Instance = this;
            Name = "Virtual Device";
            Meta = "Simulates inputs for SteamVR";
            
            // Add controls for the device
            AddControl(InputControlType.RightTrigger, "Right Trigger");
            AddControl(InputControlType.LeftTrigger, "Left Trigger");
            AddControl(InputControlType.LeftBumper, "Left Bumper");
            AddControl(InputControlType.RightBumper, "Right Bumper");
            
            this.AddControl(InputControlType.LeftStickLeft, "Left Stick Left", 0.2f, 0.9f);
            this.AddControl(InputControlType.LeftStickRight, "Left Stick Right", 0.2f, 0.9f);
            this.AddControl(InputControlType.LeftStickUp, "Left Stick Up", 0.2f, 0.9f);
            this.AddControl(InputControlType.LeftStickDown, "Left Stick Down", 0.2f, 0.9f);
            this.AddControl(InputControlType.RightStickLeft, "Right Stick Left", 0.2f, 0.9f);
            this.AddControl(InputControlType.RightStickRight, "Right Stick Right", 0.2f, 0.9f);
            this.AddControl(InputControlType.RightStickUp, "Right Stick Up", 0.2f, 0.9f);
            this.AddControl(InputControlType.RightStickDown, "Right Stick Down", 0.2f, 0.9f);
            
            this.AddControl(InputControlType.Action1, "Action1");
            this.AddControl(InputControlType.DPadRight, "DPad Right");
            this.AddControl(InputControlType.LeftStickButton, "Left Stick Button");
            this.AddControl(InputControlType.Action3, "Action3");
            this.AddControl(InputControlType.Action2, "Action2");

            SteamVR_Actions.global.Activate();
        }

        public override void Update(ulong updateTick, float deltaTime)
        {
            if (repauseDelay > 0.0f)
            {
                repauseDelay = Mathf.Clamp(repauseDelay - deltaTime, 0, REPAUSE_DELAY);
                return;
            }

            switch (Mode)
            {
                case SRInput.InputMode.DEFAULT:
                    UpdateLeftStickWithValue(SteamVR_Actions.slimecontrols.move.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    UpdateRightStickWithValue(SteamVR_Actions.slimecontrols.look.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    SRInput.Actions.attack.UpdateWithValue(SteamVR_Actions.slimecontrols.shoot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.vac.UpdateWithValue(SteamVR_Actions.slimecontrols.vac.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.nextSlot.UpdateWithValue(SteamVR_Actions.slimecontrols.nextslot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.prevSlot.UpdateWithValue(SteamVR_Actions.slimecontrols.prevslot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    SRInput.Actions.jump.UpdateWithState(SteamVR_Actions.slimecontrols.jump.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.run.UpdateWithState(SteamVR_Actions.slimecontrols.sprint.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.interact.UpdateWithState(SteamVR_Actions.slimecontrols.interact.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.burst.UpdateWithState(SteamVR_Actions.slimecontrols.pulse.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.openMap.UpdateWithState(SteamVR_Actions.slimecontrols.map.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    if (SteamVR_Actions.slimecontrols.toggle_vacgun.GetStateDown(SteamVR_Input_Sources.Any))
                        HandManager.Instance?.SetVacVisibility(!HandManager.Instance.vacShown);

                    SRInput.Actions.pedia.UpdateWithState(SteamVR_Actions.slimecontrols.slimepedia.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.light.UpdateWithState(SteamVR_Actions.slimecontrols.flashlight.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.radarToggle.UpdateWithState(SteamVR_Actions.slimecontrols.radar.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.toggleGadgetMode.UpdateWithState(SteamVR_Actions.slimecontrols.gadgetmode.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.Actions.menu.UpdateWithState(SteamVR_Actions.slimecontrols.pause.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    break;
                case SRInput.InputMode.PAUSE:

                    SRInput.PauseActions.submit.UpdateWithState(SteamVR_Actions.ui.submit.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.altSubmit.UpdateWithState(SteamVR_Actions.ui.alt_submit.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.cancel.UpdateWithState(SteamVR_Actions.ui.cancel.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.closeMap.UpdateWithState(SteamVR_Actions.ui.close.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.menuTabLeft.UpdateWithState(SteamVR_Actions.ui.prev_tab.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.menuTabRight.UpdateWithState(SteamVR_Actions.ui.next_tab.GetStateDown(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                    SRInput.PauseActions.unmenu.UpdateWithState(SteamVR_Actions.ui.close.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                    float value = SteamVR_Actions.ui.scroll.GetAxis(SteamVR_Input_Sources.Any).y * 5;
                    (value > 0 ? SRInput.PauseActions.menuScrollUp : SRInput.PauseActions.menuScrollDown).UpdateWithValue(Mathf.Abs(value), updateTick, deltaTime);

                    Vector2 navigation = SteamVR_Actions.ui.navigate.GetAxis(SteamVR_Input_Sources.Any);
                    (navigation.x > 0 ? SRInput.PauseActions.menuRight : SRInput.PauseActions.menuLeft).UpdateWithValue(Mathf.Abs(navigation.x), updateTick, deltaTime);
                    (navigation.y > 0 ? SRInput.PauseActions.menuUp : SRInput.PauseActions.menuDown).UpdateWithValue(Mathf.Abs(navigation.y), updateTick, deltaTime);

                    break;
            }

            if (!(SceneContext.Instance?.TimeDirector?.HasPauser() ?? false))
            {
                float rightStickX = SteamVR_Actions.slimecontrols.look.axis.x;
                float snapThreshold = 0.7f; // Stick must be pushed at least 70% to trigger snap turn
                float resetThreshold = 0.2f; // Stick must return below 20% to reset
                int snapAngle = 45; // Degrees per snap turn

                if (VRConfig.SNAP_TURN)
                {
                    if (Mathf.Abs(rightStickX) > snapThreshold && !Patch_vp_FPInput.snapTriggered)
                    {
                        int snapDirection = rightStickX > 0 ? 1 : -1;
                        Patch_vp_FPInput.adjustmentDegrees += snapDirection * snapAngle;
                        Patch_vp_FPInput.adjustmentDegrees %= 360;
                        Patch_vp_FPInput.snapTriggered = true; // Prevent multiple snaps until stick resets
                    }
                    else if (Mathf.Abs(rightStickX) < resetThreshold) // Stick has returned to center
                    {
                        Patch_vp_FPInput.snapTriggered = false; // Allow next snap
                    }
                }
                else
                {
                    Patch_vp_FPInput.adjustmentDegrees += (rightStickX * deltaTime) * SteamVR.instance.hmd_DisplayFrequency;
                    Patch_vp_FPInput.adjustmentDegrees %= 360;
                }
            }
        }
        

        public static void RegisterCallbacks()
        {
            Instance = new VRInput();
            InputManager.AttachDevice(Instance);
        }
        public enum SteamVRLocalizedOrigin
        {
            // Generic Hand Assignments
            LeftHand,
            RightHand,

            // Oculus Touch Controllers
            LeftIndexTrigger,
            RightIndexTrigger,
            LeftThumbstick,
            RightThumbstick,
            AButton,
            BButton,
            XButton,
            YButton,

            // Valve Index (Knuckles) Controllers
            LeftTrigger,
            RightTrigger,
            LeftTrackpad,
            RightTrackpad,
            LeftGrip,
            RightGrip,
            LeftBButton,
            RightBButton,
            LeftAButton,
            RightAButton,

            // HTC Vive Controllers
            LeftMenuButton,
            RightMenuButton,

            // Windows Mixed Reality Controllers
            LeftMenu,
            RightMenu
        }

        public static bool GetLocalizedOriginEnum(string localizedOrigin, out SteamVRLocalizedOrigin result)
        {
            return Enum.TryParse(localizedOrigin.Replace(" ", string.Empty), true, out result);
        }
        public static bool GetVRButton(UITemplates uiTemplates, SteamVR_Action_In_Source action , out Sprite result)
        {
            string inputSource = action.GetLocalizedOriginPart(EVRInputStringBits.VRInputString_InputSource);
            string controllerType = action.GetLocalizedOriginPart(EVRInputStringBits.VRInputString_ControllerType);
            
            if (GetLocalizedOriginEnum(inputSource, out var localizedOrigin))
            {
                EntryPoint.ConsoleInstance.Log($"GetVRButton: {controllerType}, {localizedOrigin}");
                switch (localizedOrigin)
                {
                    case SteamVRLocalizedOrigin.AButton:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["Action1"];
                        break;
                    }
                    case SteamVRLocalizedOrigin.LeftAButton:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.Unknown]["Action1"];
                        break;
                    }
                    case SteamVRLocalizedOrigin.BButton:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["Action2"];
                        break;
                    }
                    case SteamVRLocalizedOrigin.LeftBButton:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["Action2"];
                        break;
                    }
                    case SteamVRLocalizedOrigin.XButton:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["Action3"];
                        break;
                    }
                    case SteamVRLocalizedOrigin.YButton:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["Action4"];
                        break;
                    }
                  
                    
                    case SteamVRLocalizedOrigin.RightTrigger:
                    case SteamVRLocalizedOrigin.RightIndexTrigger:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["RightTrigger"];
                        break;
                    }
                    case SteamVRLocalizedOrigin.RightThumbstick:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["RightStickMove"];
                        break;
                    }
                    case SteamVRLocalizedOrigin.LeftTrigger:
                    case SteamVRLocalizedOrigin.LeftIndexTrigger:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["LeftTrigger"];
                        break;
                    }
                    case SteamVRLocalizedOrigin.LeftThumbstick:
                    {
                        result = uiTemplates.deviceButtonIconDict[InputDeviceStyle.XboxOne]["LeftStickMove"];
                        break;
                    }
                    
                    default:
                    {
                         
                        result = uiTemplates.unknownButtonIcon;
                        return false;
                    }
                }

                return true;
            }

            EntryPoint.ConsoleInstance.Log("Can't parsed localized origin : " +localizedOrigin);
            result = uiTemplates.unknownButtonIcon;
            return false;
        }

    }
}