using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InControl;
using SRML;
using SRML.Config.Attributes;
using SRML.Console;
using SRML.SR;
using SRVR.Components;
using SRVR.Patches;
using UnityEngine;
using Valve.VR;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace SRVR
{
    [ConfigFile("VR")]
    
    public class VRConfig
    {
        public static bool SWITCH_HANDS = false;
        public static bool STATIC_UI_POSITION = true;
        public static bool SNAP_TURN = false;
        public static int SNAP_TURN_ANGLE = 45;
        public static float TURN_SENSITIVITY = 1f;
        public static float HEIGHT_ADJUSTMENT = 0f;
        public static bool DISTANCE_GRAB = true;
        public static bool PEDIA_ON_VAC = true;
        
        public static void SaveConfig()
        {
            SRMod mod = SRModLoader.GetModForAssembly(typeof(EntryPoint).Assembly);
            SRMod.ForceModContext(mod);
            mod.Configs.Find((x) => x.FileName.ToLower() == "vr").SaveToFile();
            SRMod.ClearModContext();
        }
        
    }
    
    public class EntryPoint : ModEntryPoint
    {
        public new static Console.ConsoleInstance ConsoleInstance = new Console.ConsoleInstance("SRVR");
        public static AssetBundle VRHands = AssetBundle.LoadFromStream(typeof(EntryPoint).Assembly.GetManifestResourceStream("SRVR.newvrhands"));
        public static bool EnabledVR = true;
        public static GameObject pediaModel;

        public EntryPoint()
        {
            VRInstaller.Install();
        }
        public override void PreLoad()
        {
            EnabledVR = !Environment.GetCommandLineArgs().Contains("-novr");
            if (!VRInstaller.IsAfterInstall) {
                SRCallbacks.OnMainMenuLoaded += _ =>
                {
                    var uiTemplates = Object.FindObjectOfType<UITemplates>();
                    VRInstaller.UninstallDLC(uiTemplates);
                };
                return;
            }

            if (typeof(ConsoleWindow).GetMethod("OnGUI", AccessTools.all) != null)
            {
                EntryPoint.ConsoleInstance.LogWarning("Optimization Fixes did not work properly, please try to reinstall the game and mod.");
            }

            TranslationPatcher.AddUITranslation("b.vr", "VR");
            TranslationPatcher.AddUITranslation("b.uninstall_srvr", "Uninstall SRVR");
            TranslationPatcher.AddUITranslation("b.snap_turn", "Toggle Snap Turn");
            TranslationPatcher.AddUITranslation("b.dominant_hand", "Dominant Hand:");
            TranslationPatcher.AddUITranslation("b.distance_grab", "Distance Grab");
            TranslationPatcher.AddUITranslation("b.snap_turn_angle", "Snap Turn Angle");
            TranslationPatcher.AddUITranslation("b.pedia_toggle", "Toggle Pedia on Vacpack");
            TranslationPatcher.AddUITranslation("b.turn_sensitivity", "Turn Sensitivity");
            TranslationPatcher.AddUITranslation("b.height_adjust", "Height Adjustment");
            TranslationPatcher.AddUITranslation("b.left_handed", "Left Handed");
            TranslationPatcher.AddUITranslation("b.right_handed", "Right Handed");
            TranslationPatcher.AddUITranslation("b.static_ui", "Static UI Position");

            if (EnabledVR)
            {
                if (VRManager.InitializeVR())
                {
                    if (VRManager.StartVR())
                    {
                        VRInput.RegisterCallbacks();
                    }
                    else
                    {
                        EnabledVR = false;
                        
                    }
                }
                else
                {
                    EnabledVR = false;
                }
            }

            if (EnabledVR)
            {
                HarmonyInstance.PatchAll();
            }
            else
            {
                HarmonyInstance.Patch(AccessTools.Method(typeof(OptionsUI), nameof(OptionsUI.SetupOptionsUI)), prefix: new HarmonyMethod(typeof(Patch_UI), nameof(Patch_UI.SetupOptionsUI)));
                HarmonyInstance.Patch(AccessTools.Method(typeof(OptionsUI), nameof(OptionsUI.DeselectAll)), prefix: new HarmonyMethod(typeof(Patch_UI), nameof(Patch_UI.DeselectAll)));
                return;
            }

            pediaModel = GameObject.Find("Art").transform.Find("BeatrixMainMenu/slimepedia").gameObject.Instantiate();
            pediaModel.DontDestroyOnLoad();
            pediaModel.name = "PediaInteract";
            pediaModel.AddComponent<MeshRenderer>().material = pediaModel.GetComponent<SkinnedMeshRenderer>().material;
            pediaModel.AddComponent<MeshFilter>().mesh = pediaModel.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            Object.Destroy(pediaModel.GetComponent<SkinnedMeshRenderer>());
            
            var pediaColObj = new GameObject("Collision") { transform = { parent = pediaModel.transform } };
            
            pediaColObj.AddComponent<BoxCollider>().isTrigger = true;
            pediaColObj.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            
            pediaColObj.transform.localScale = new Vector3(0.35f, 0.1f, 0.35f);
            pediaColObj.transform.localPosition = new Vector3(0f, 0.2f, -0.05f);
            pediaColObj.transform.localEulerAngles = Vector3.zero;
            
            pediaModel.SetActive(false);

            Material lineMat = new Material(Shader.Find("Sprites/Default"));
            lineMat.name = "line";
            lineMat.hideFlags = HideFlags.HideAndDontSave;
            HandManager.lineMat = lineMat;
            
            SRCallbacks.OnMainMenuLoaded += menu =>
            {
                var deviceButtonIconDict = GameContext.Instance.UITemplates.deviceButtonIconDict;
                deviceButtonIconDict[InputDeviceStyle.XboxOne]["Trigger"] = SRLookup.MergeSpritesWithPivots(deviceButtonIconDict[InputDeviceStyle.XboxOne]["LeftTrigger"], deviceButtonIconDict[InputDeviceStyle.XboxOne]["RightTrigger"]).CreateSprite();
                deviceButtonIconDict[InputDeviceStyle.XboxOne]["Grip"] = SRLookup.MergeSpritesWithPivots(deviceButtonIconDict[InputDeviceStyle.XboxOne]["LeftBumper"], deviceButtonIconDict[InputDeviceStyle.XboxOne]["RightBumper"]).CreateSprite();

                var fpsCamera = GameObject.Find("FPSCamera");
                
                var camera = new GameObject("Camera")
                {
                    transform = // rotation Y should be 260 as far as ive seen.
                    {
                        position = new Vector3(14.26f, 1.1f, 3.98f),
                        eulerAngles = new Vector3(0, 260f, 0),
                    }
                };
                
                Patch_vp_FPInput.AdjustmentDegrees = 260f;
                fpsCamera.transform.parent = camera.transform;
                fpsCamera.transform.localPosition = Vector3.zero;
                fpsCamera.transform.localEulerAngles = Vector3.zero;
                var controllers = VRManager.InstantiateVRRig();
                controllers.transform.SetParent(camera.transform, false);
                vp_Layer.Set(controllers.gameObject, vp_Layer.Actor, true);
                fpsCamera.AddComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.Hmd; 
                menu.transform.Find("MessageOfTheDay").gameObject.SetActive(false);
            };

            Patch_LoadingUI.BackgroundSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name == "UISprite");
        }
    }
}

    