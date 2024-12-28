using System;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;


namespace SRVR
{
    public class VRManager
    {
        public static bool recenter  = false;

        public static bool InitializeVR()
        {
            // if (!VRAssetManager.Initialize())
            // {
            //     LogError("Failed to load required assets!.");
            //     return false;
            //
            // }
            SteamVR_Actions.PreInitialize();
            if (!InitXRSDK())
            {
                EntryPoint.ConsoleInstance.LogError("Failed to initialize VR!.");
                return false;
            }
            if (!InitializeSteamVR())
            {
                EntryPoint.ConsoleInstance.LogError("Problem initializing SteamVR");
                return false;
            }
            return true;
            
        }
        public static bool StartVR()
        {
            EntryPoint.ConsoleInstance.Log("Starting VR...");
            return StartXRSDK();
        }

        public static bool InitializeSteamVR()
        {
            EntryPoint.ConsoleInstance.Log("Initializing SteamVR...");
            SteamVR.Initialize();
            if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
            {
                EntryPoint.ConsoleInstance.LogError("Problem Initializing SteamVR");
                return false;
            }
            if (!SteamVR_Input.initialized)
            {
                EntryPoint.ConsoleInstance.LogError("Problem Initializing SteamVR_Input");
                return false;
            }

            /*ApplicationManifestHelper.UpdateManifest(Path.Combine(Application.streamingAssetsPath, "valheim.vrmanifest"),
                                                    "steam.app.892970",
                                                    "https://steamcdn-a.akamaihd.net/steam/apps/892970/header.jpg",
                                                    "Valheim VR",
                                                    "VR mod for Valheim",
                                                    steamBuild: true,
                                                    steamAppId: 892970);
                                                    */
            return true;
        }

        private static bool StartXRSDK()
        {
            EntryPoint.ConsoleInstance.Log("Starting XRSDK!");
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null
                && XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            } else
            {
                EntryPoint.ConsoleInstance.LogError("Error Starting XRSDK!");
                if (XRGeneralSettings.Instance == null)
                {
                    EntryPoint.ConsoleInstance.LogError("XRGeneralSettings Instance is null!");
                    return false;
                } else if (XRGeneralSettings.Instance.Manager == null)
                {
                    EntryPoint.ConsoleInstance.LogError("XRManager instance is null!");
                    return false;
                } else if (XRGeneralSettings.Instance.Manager.activeLoader == null)
                {
                    EntryPoint.ConsoleInstance.LogError("Active loader is null!");
                    return false;
                }
            }
            return true;
        }

        private static bool InitXRSDK()
        {
            XRGeneralSettings xrGeneralSettings = LoadXRSettingsFromAssetBundle();
            if (xrGeneralSettings == null)
            {
                EntryPoint.ConsoleInstance.LogError("XRGeneralSettings Instance is null!");
                return false;
            }
            EntryPoint.ConsoleInstance.Log("Loaded XRGeneralSettings!");
            return InitializeXRSDKLoaders(xrGeneralSettings.Manager);
        }

        private static XRGeneralSettings LoadXRSettingsFromAssetBundle()
        {
           
            EntryPoint.ConsoleInstance.LogError("Creating XRGeneralSettings");
            var general = ScriptableObject.CreateInstance<XRGeneralSettings>();
            EntryPoint.ConsoleInstance.LogError("Creating XRManagerSettings");
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            EntryPoint.ConsoleInstance.LogError("Creating OpenVRLoader");

            var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();
            general.Manager = managerSettings;
            managerSettings.loaders.Clear();
            managerSettings.loaders.Add(xrLoader);
            XRGeneralSettings instance = XRGeneralSettings.Instance;
            if (instance == null)
            {
                EntryPoint.ConsoleInstance.LogError("XRGeneralSettings Instance is null!");
                return null;
            }
            EntryPoint.ConsoleInstance.Log("XRGeneralSettings Instance is non-null.");
            return instance;
        }

        private static bool InitializeXRSDKLoaders(XRManagerSettings managerSettings)
        {
            EntryPoint.ConsoleInstance.Log("Initializing XRSDK Loaders...");
            if (managerSettings == null)
            {
                EntryPoint.ConsoleInstance.LogError("XRManagerSettings instance is null, cannot initialize loader.");
                return false;
            }
            managerSettings.InitializeLoaderSync();
            if (managerSettings.activeLoader == null)
            {
                EntryPoint.ConsoleInstance.LogError("XRManager.activeLoader is null! Cannot initialize VR!");
                return false;
            }
            OpenVRSettings openVrSettings = OpenVRSettings.GetSettings(false);
            if (openVrSettings != null)
            {
                OpenVRSettings.MirrorViewModes mirrorMode = OpenVRSettings.MirrorViewModes.None;
                EntryPoint.ConsoleInstance.Log("Mirror View Mode: " + mirrorMode);
                openVrSettings.SetMirrorViewMode(mirrorMode);
            }
            EntryPoint.ConsoleInstance.Log("Got non-null Active Loader.");
            return true;
        }

        public static void tryRecenter()
        {
            List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(inputSubsystems);
            foreach (var subsystem in inputSubsystems)
            {
                
                EntryPoint.ConsoleInstance.Log("Recentering Input Subsystem: " + subsystem);
                subsystem.TryRecenter();
            }

            
            

            // Trigger recentering head position on player body
        }

        private static void PrintSteamVRSettings()
        {
            SteamVR_Settings settings = SteamVR_Settings.instance;
            if (settings == null)
            {
                EntryPoint.ConsoleInstance.LogWarning("SteamVR Settings are null.");
                return;
            }
            EntryPoint.ConsoleInstance.Log("SteamVR Settings:");
            EntryPoint.ConsoleInstance.Log("  actionsFilePath: " + settings.actionsFilePath);
            EntryPoint.ConsoleInstance.Log("  editorAppKey: " + settings.editorAppKey);
            EntryPoint.ConsoleInstance.Log("  activateFirstActionSetOnStart: " + settings.activateFirstActionSetOnStart);
            EntryPoint.ConsoleInstance.Log("  autoEnableVR: " + settings.autoEnableVR);
            EntryPoint.ConsoleInstance.Log("  inputUpdateMode: " + settings.inputUpdateMode);
            EntryPoint.ConsoleInstance.Log("  legacyMixedRealityCamera: " + settings.legacyMixedRealityCamera);
            EntryPoint.ConsoleInstance.Log("  mixedRealityCameraPose: " + settings.mixedRealityCameraPose);
            EntryPoint.ConsoleInstance.Log("  lockPhysicsUpdateRateToRenderFrequency: " + settings.lockPhysicsUpdateRateToRenderFrequency);
            EntryPoint.ConsoleInstance.Log("  mixedRealityActionSetAutoEnable: " + settings.mixedRealityActionSetAutoEnable);
            EntryPoint.ConsoleInstance.Log("  mixedRealityCameraInputSource: " + settings.mixedRealityCameraInputSource);
            EntryPoint.ConsoleInstance.Log("  mixedRealityCameraPose: " + settings.mixedRealityCameraPose);
            EntryPoint.ConsoleInstance.Log("  pauseGameWhenDashboardVisible: " + settings.pauseGameWhenDashboardVisible);
            EntryPoint.ConsoleInstance.Log("  poseUpdateMode: " + settings.poseUpdateMode);
            EntryPoint.ConsoleInstance.Log("  previewHandLeft: " + settings.previewHandLeft);
            EntryPoint.ConsoleInstance.Log("  previewHandRight: " + settings.previewHandRight);
            EntryPoint.ConsoleInstance.Log("  steamVRInputPath: " + settings.steamVRInputPath);
        }

        private static void PrintOpenVRSettings()
        {
            OpenVRSettings settings = OpenVRSettings.GetSettings(false);
            if (settings == null)
            {
                EntryPoint.ConsoleInstance.LogWarning("OpenVRSettings are null.");
                return;
            }
            EntryPoint.ConsoleInstance.Log("OpenVR Settings:");
            EntryPoint.ConsoleInstance.Log("  StereoRenderingMode: " + settings.StereoRenderingMode);
            EntryPoint.ConsoleInstance.Log("  InitializationType: " + settings.InitializationType);
            EntryPoint.ConsoleInstance.Log("  ActionManifestFileRelativeFilePath: " + settings.ActionManifestFileRelativeFilePath);
            EntryPoint.ConsoleInstance.Log("  MirrorView: " + settings.MirrorView);

        }
        

    }
}