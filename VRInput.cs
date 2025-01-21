using InControl;
using SRML.Console;
using SRVR.Patches;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using InputDevice = InControl.InputDevice;

namespace SRVR
{
    public class VRInput : InputDevice
    {
        public static VRInput Instance;
        public static SRInput.InputMode Mode;
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
            
            
            // UpdateWithState(InputControlType.Action1, SteamVR_Actions.slimecontrols.jump.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
            // UpdateWithState(InputControlType.LeftStickButton, SteamVR_Actions.slimecontrols.sprint.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
            // UpdateWithState(InputControlType.Action3, SteamVR_Actions.slimecontrols.interact.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
            // UpdateWithState(InputControlType.Action2, SteamVR_Actions.slimecontrols.pulse.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
            // UpdateWithState(InputControlType.DPadRight, SteamVR_Actions.slimecontrols.map.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
            
            
        }
        
        
        public override void Update(ulong updateTick, float deltaTime)
        {
            // if (Mode == SRInput.InputMode.DEFAULT)
            {
                UpdateLeftStickWithValue(SteamVR_Actions.slimecontrols.move.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateRightStickWithValue(SteamVR_Actions.slimecontrols.look.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateWithValue(InputControlType.RightTrigger, SteamVR_Actions.slimecontrols.shoot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateWithValue(InputControlType.LeftTrigger, SteamVR_Actions.slimecontrols.vac.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateWithValue(InputControlType.LeftBumper, SteamVR_Actions.slimecontrols.nextslot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateWithValue(InputControlType.RightBumper, SteamVR_Actions.slimecontrols.prevslot.GetAxis(SteamVR_Input_Sources.Any), updateTick, deltaTime);
            
                UpdateWithState(InputControlType.Action1, SteamVR_Actions.slimecontrols.jump.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateWithState(InputControlType.LeftStickButton, SteamVR_Actions.slimecontrols.sprint.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateWithState(InputControlType.Action3, SteamVR_Actions.slimecontrols.interact.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateWithState(InputControlType.Action2, SteamVR_Actions.slimecontrols.pulse.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);
                UpdateWithState(InputControlType.DPadRight, SteamVR_Actions.slimecontrols.map.GetState(SteamVR_Input_Sources.Any), updateTick, deltaTime);

                if (!SceneContext.Instance?.TimeDirector?.HasPauser() ?? false)
                {
                    Patch_vp_FPInput.adjustmentDegrees += RightStickX.Value;
                    Patch_vp_FPInput.adjustmentDegrees %= 360;
                }
            }
        }

        public static void RegisterCallbacks()
        {
            Instance = new VRInput();
            InputManager.AttachDevice(Instance);
            // SteamVR_Actions.slimecontrols.Activate();

           


        }
    }
}