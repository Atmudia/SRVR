using System.Collections.Generic;
using System.Linq.Expressions;
using SRVR.Components;
using SRVR.Files;
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
        public static Material HandsMaterial;
        public static GameObject CurrentVRRig;

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
            LoadAssetsFromAssetBundle();
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

        public static void LoadAssetsFromAssetBundle()
        {
            HandsMaterial = EntryPoint.VRHands.LoadAsset<Material>("Hands Material 1");
            HandsMaterial.shader = Shader.Find("SR/Actor, Vac (Hands)");
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
            GameObject controllers = new GameObject("Controllers")
            {
                transform = { localPosition = Vector3.zero }
            };

            SteamVR_Skeleton_Pose relaxedPose = Object.Instantiate(EntryPoint.VRHands.LoadAsset<SteamVR_Skeleton_Pose>("relaxed"));
            relaxedPose.hideFlags = HideFlags.HideAndDontSave;
            relaxedPose.leftHand.bonePositions = relaxed_Pose.leftHandPositions;
            relaxedPose.leftHand.boneRotations = relaxed_Pose.leftHandRotations;
            relaxedPose.rightHand.bonePositions = relaxed_Pose.rightHandPositions;
            relaxedPose.rightHand.boneRotations = relaxed_Pose.rightHandRotations;
            SteamVR_Skeleton_Pose fistPose = Object.Instantiate(EntryPoint.VRHands.LoadAsset<SteamVR_Skeleton_Pose>("fist"));
            fistPose.hideFlags = HideFlags.HideAndDontSave;
            fistPose.leftHand.bonePositions = fist_Pose.leftHandPositions;
            fistPose.leftHand.boneRotations = fist_Pose.leftHandRotations;
            fistPose.rightHand.bonePositions = fist_Pose.rightHandPositions;
            fistPose.rightHand.boneRotations = fist_Pose.rightHandRotations;

            // left controller

            GameObject leftController = new GameObject("Left Controller")
            {
                transform = { parent = controllers.transform, }
            };

            leftController.SetActive(false);
            GameObject leftHand = Object.Instantiate(EntryPoint.VRHands.LoadAsset<GameObject>("left hand"), new Vector3(0, 0, -0.1f), Quaternion.identity, leftController.transform);
            leftHand.name = "Hand";
            leftHand.GetComponentInChildren<SkinnedMeshRenderer>(true).sharedMaterial = HandsMaterial;
            
            SteamVR_Behaviour_Skeleton leftSkeleton = leftHand.GetComponentInChildren<SteamVR_Behaviour_Skeleton>(true);
            leftSkeleton.skeletonAction = SteamVR_Actions.slimecontrols.pose_left;
            leftSkeleton.fallbackCurlAction = SteamVR_Actions.slimecontrols.fallback_left;
            SteamVR_Skeleton_Poser leftPoser = leftHand.GetComponentInChildren<SteamVR_Skeleton_Poser>();
            leftPoser.skeletonMainPose = relaxedPose;
            leftPoser.skeletonAdditionalPoses = new List<SteamVR_Skeleton_Pose>() { fistPose };

            PosHand leftPositioner = leftController.AddComponent<PosHand>();

            leftController.layer = LayerMask.NameToLayer("Weapon");
            leftHand.layer = LayerMask.NameToLayer("Weapon");
            leftHand.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer = LayerMask.NameToLayer("Weapon");

            GameObject leftPickupOrigin = new GameObject("origin")
            {
                transform =
                {
                    parent = leftHand.transform,
                    localPosition = Vector3.zero,
                    rotation = Quaternion.identity
                }
            };

            PickupVacuumable leftPickuper = leftHand.AddComponent<PickupVacuumable>();
            leftPickuper.origin = leftPickupOrigin.transform;
            leftPickuper.skeletonAction = SteamVR_Actions.slimecontrols.pose_left;

            // right controller

            GameObject rightController = new GameObject("Right Controller")
            {
                transform = { parent = controllers.transform }
            };
            rightController.SetActive(false);
            GameObject rightHand = Object.Instantiate(EntryPoint.VRHands.LoadAsset<GameObject>("right hand"), new Vector3(0, 0, -0.1f), Quaternion.identity, rightController.transform);
            rightHand.name = "Hand";
            rightHand.GetComponentInChildren<SkinnedMeshRenderer>(true).sharedMaterial = HandsMaterial;

            SteamVR_Behaviour_Skeleton rightSkeleton = rightHand.GetComponentInChildren<SteamVR_Behaviour_Skeleton>(true);
            rightSkeleton.skeletonAction = SteamVR_Actions.slimecontrols.pose_right;
            rightSkeleton.fallbackCurlAction = SteamVR_Actions.slimecontrols.fallback_right;
            SteamVR_Skeleton_Poser rightPoser = rightHand.GetComponentInChildren<SteamVR_Skeleton_Poser>();
            rightPoser.skeletonMainPose = relaxedPose;
            rightPoser.skeletonAdditionalPoses = new List<SteamVR_Skeleton_Pose>() { fistPose };

            PosHand rightPositioner = rightController.AddComponent<PosHand>();

            rightController.layer = LayerMask.NameToLayer("Weapon");
            rightHand.layer = LayerMask.NameToLayer("Weapon");
            rightHand.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer = LayerMask.NameToLayer("Weapon");

            GameObject rightPickupOrigin = new GameObject("origin")
            {
                transform =
                {
                    parent = rightHand.transform,
                    localPosition = Vector3.zero,
                    rotation = Quaternion.identity
                }
            };

            PickupVacuumable rightPickuper = rightHand.AddComponent<PickupVacuumable>();
            rightPickuper.origin = rightPickupOrigin.transform;
            rightPickuper.skeletonAction = SteamVR_Actions.slimecontrols.pose_right;

            HandManager toggler = controllers.AddComponent<HandManager>();
            toggler.Awake();
            toggler.leftController = leftController;
            toggler.rightController = rightController;
            toggler.leftHand = leftHand;
            toggler.rightHand = rightHand;
            toggler.leftHandModel = leftHand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject;
            toggler.rightHandModel = rightHand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject;
            toggler.leftHandPositioner = leftPositioner;
            toggler.rightHandPositioner = rightPositioner;
            toggler.UpdateHandStates();

            CurrentVRRig = controllers;

            return controllers.transform;
        }
    }
}