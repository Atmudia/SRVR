using System.Collections.Generic;
using SRVR.Components;
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
           
            EntryPoint.ConsoleInstance.Log("Creating XRGeneralSettings");
            var general = ScriptableObject.CreateInstance<XRGeneralSettings>();
            EntryPoint.ConsoleInstance.Log("Creating XRManagerSettings");
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            EntryPoint.ConsoleInstance.Log("Creating OpenVRLoader");

            var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();
            general.Manager = managerSettings;
            managerSettings.loaders.Clear();
            managerSettings.loaders.Add(xrLoader);
            XRGeneralSettings instance = XRGeneralSettings.Instance;
            if (!instance)
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
            if (!managerSettings)
            {
                EntryPoint.ConsoleInstance.LogError("XRManagerSettings instance is null, cannot initialize loader.");
                return false;
            }
            managerSettings.InitializeLoaderSync();
            if (!managerSettings.activeLoader)
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
        }

        public static Transform InstantiateVRRig()
        {
            var controllers = new GameObject("Controllers")
            {
                transform =
                {
                    localPosition = Vector3.zero
                }
            };
            
            var leftHandMesh = EntryPoint.VRAssets.LoadAsset<Mesh>("leftHand");
            var rightHandMesh = EntryPoint.VRAssets.LoadAsset<Mesh>("rightHand");
            
            var handsMaterial = EntryPoint.VRAssets.LoadAsset<Material>("Hands Material 1");
            var leftController = new GameObject("Left Controller")
            {
                transform =
                {
                    parent = controllers.transform,
                }
            };
            var leftHand = new GameObject("Left Hand")
            {
                transform =
                {
                    parent = leftController.transform,
                    position = new Vector3(0, 0, -0.1f),
                    rotation = Quaternion.Euler(0, 90, 0),
                }
            };
            leftHand.AddComponent<MeshRenderer>().sharedMaterial = handsMaterial;
            var leftHandCol = leftHand.AddComponent<BoxCollider>();
            leftHandCol.size = new Vector3(0.08f, 0.04f, 0.16f);
            // leftHandCol.size = new Vector3(0f, 0f, -0.1f);
            
            
            leftHand.AddComponent<MeshFilter>().sharedMesh = VRConfig.SWITCH_HANDS ? rightHandMesh : leftHandMesh;
            leftController.AddComponent<PosHand>().hand = VRConfig.SWITCH_HANDS ? XRNode.RightHand : XRNode.LeftHand;
            leftController.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            leftController.layer = LayerMask.NameToLayer("Weapon");
            leftHand.layer = LayerMask.NameToLayer("Weapon");
            var rightController = new GameObject("Right Controller")
            {
                transform = { parent = controllers.transform }
            };
            var rightHand = new GameObject("Right Hand")
            {
                transform =
                {
                    parent = rightController.transform,
                    position = new Vector3(0, 0, -0.1f),
                    rotation = Quaternion.Euler(0, 90, 0),
                }
            };
            rightHand.AddComponent<MeshRenderer>().sharedMaterial = handsMaterial;
            rightHand.AddComponent<MeshFilter>().sharedMesh = VRConfig.SWITCH_HANDS ? leftHandMesh : rightHandMesh;
            
            rightController.AddComponent<PosHand>().hand = VRConfig.SWITCH_HANDS ? XRNode.LeftHand : XRNode.RightHand;
            return controllers.transform;
        }

        

    }
}