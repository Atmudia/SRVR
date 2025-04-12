using System.Collections.Generic;
using System.Linq;
using SRVR.Components;
using SRVR.Files;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR.Management;
using Valve.VR;


namespace SRVR
{
    public class VRManager
    {
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
            SteamVR.Initialize(true);
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
            if (XRGeneralSettings.Instance && XRGeneralSettings.Instance.Manager
                                           && XRGeneralSettings.Instance.Manager.activeLoader)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            } else
            {
                EntryPoint.ConsoleInstance.LogError("Error Starting XRSDK!");
                if (!XRGeneralSettings.Instance)
                {
                    EntryPoint.ConsoleInstance.LogError("XRGeneralSettings Instance is null!");
                    return false;
                }

                if (!XRGeneralSettings.Instance.Manager)
                {
                    EntryPoint.ConsoleInstance.LogError("XRManager instance is null!");
                    return false;
                }
                if (!XRGeneralSettings.Instance.Manager.activeLoader)
                {
                    EntryPoint.ConsoleInstance.LogError("Active loader is null!");
                    return false;
                }
            }
            return true;
        }

        private static bool InitXRSDK()
        {
            XRGeneralSettings xrGeneralSettings = CreateXRSettings();
            if (!xrGeneralSettings)
            {
                EntryPoint.ConsoleInstance.LogError("XRGeneralSettings Instance is null!");
                return false;
            }
            EntryPoint.ConsoleInstance.Log("Loaded XRGeneralSettings!");
            return InitializeXRSDKLoaders(xrGeneralSettings.Manager);
        }

        private static XRGeneralSettings CreateXRSettings()
        {
            EntryPoint.ConsoleInstance.Log("Creating XRGeneralSettings");
            var general = ScriptableObject.CreateInstance<XRGeneralSettings>();
            EntryPoint.ConsoleInstance.Log("Creating XRManagerSettings");
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            EntryPoint.ConsoleInstance.Log("Creating OpenVRLoader");
            var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();
            general.Manager = managerSettings;
            managerSettings.GetField<List<XRLoader>>("m_Loaders").Clear();
            managerSettings.GetField<List<XRLoader>>("m_Loaders").Add(xrLoader);
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
            if (openVrSettings)
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

            Sprite[] sp = EntryPoint.VRHands.LoadAssetWithSubAssets<Sprite>("steambuttons");
            VRInput.leftControllerIcons = new Dictionary<string, Sprite>()
            {
                { "/input/a", sp.First(x => x.name == "steam_a_l") },
                { "/input/b", sp.First(x => x.name == "steam_b_l") },
                { "/input/x", sp.First(x => x.name == "steam_x_l") },
                { "/input/y", sp.First(x => x.name == "steam_y_l") },
                { "/input/system", sp.First(x => x.name == "steam_menu_l") },
                { "/input/thumbstick/move", sp.First(x => x.name == "steam_left_joystick_move") },
                { "/input/thumbstick", sp.First(x => x.name == "steam_left_joystick_press") },
                { "/input/trackpad", sp.First(x => x.name == "steam_left_pad") },
                { "/input/trackpad/up", sp.First(x => x.name == "steam_left_pad_up") },
                { "/input/trackpad/down", sp.First(x => x.name == "steam_left_pad_down") },
                { "/input/trackpad/left", sp.First(x => x.name == "steam_left_pad_left") },
                { "/input/trackpad/right", sp.First(x => x.name == "steam_left_pad_right") },
                { "/input/trigger", sp.First(x => x.name == "steam_left_trigger") },
                { "/input/grip", sp.First(x => x.name == "steam_left_grip") }
            };
            VRInput.rightControllerIcons = new Dictionary<string, Sprite>()
            {
                { "/input/a", sp.First(x => x.name == "steam_a_r") },
                { "/input/b", sp.First(x => x.name == "steam_b_r") },
                { "/input/x", sp.First(x => x.name == "steam_x_r") },
                { "/input/y", sp.First(x => x.name == "steam_y_r") },
                { "/input/system", sp.First(x => x.name == "steam_menu_r") },
                { "/input/thumbstick/move", sp.First(x => x.name == "steam_right_joystick_move") },
                { "/input/thumbstick", sp.First(x => x.name == "steam_right_joystick_press") },
                { "/input/trackpad", sp.First(x => x.name == "steam_right_pad") },
                { "/input/trackpad/up", sp.First(x => x.name == "steam_right_pad_up") },
                { "/input/trackpad/down", sp.First(x => x.name == "steam_right_pad_down") },
                { "/input/trackpad/left", sp.First(x => x.name == "steam_right_pad_left") },
                { "/input/trackpad/right", sp.First(x => x.name == "steam_right_pad_right") },
                { "/input/trigger", sp.First(x => x.name == "steam_right_trigger") },
                { "/input/grip", sp.First(x => x.name == "steam_right_grip") }
            };
        }
        public static Transform InstantiateVRRig()
        {
            Patches.Patch_vp_FPInput.HMDPosition = Vector3.zero;

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
            leftSkeleton.skeletonAction = SteamVR_Actions.global.pose_left;
            leftSkeleton.fallbackCurlAction = SteamVR_Actions.global.grab_left;
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

            LineRenderer leftLine = leftHand.AddComponent<LineRenderer>();
            leftLine.enabled = false;
            leftLine.startWidth = 0.005f;
            leftLine.endWidth = 0.005f;

            leftLine.material = HandManager.lineMat;
            leftLine.startColor = Color.cyan * new Color(1, 1, 1, 0);
            leftLine.endColor = Color.cyan;

            PickupVacuumable leftPickuper = leftHand.AddComponent<PickupVacuumable>();
            leftPickuper.origin = leftPickupOrigin.transform;
            leftPickuper.raycastOrigin = leftHand.transform.Find("laser origin");
            leftPickuper.line = leftLine;
            leftPickuper.skeletonAction = SteamVR_Actions.global.pose_left;
            leftPickuper.grabAction = SteamVR_Actions.global.grab_left;

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
            rightSkeleton.skeletonAction = SteamVR_Actions.global.pose_right;
            rightSkeleton.fallbackCurlAction = SteamVR_Actions.global.grab_right;
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

            LineRenderer rightLine = rightHand.AddComponent<LineRenderer>();
            rightLine.enabled = false;
            rightLine.startWidth = 0.005f;
            rightLine.endWidth = 0.005f;

            rightLine.material = HandManager.lineMat;
            rightLine.startColor = Color.cyan * new Color(1, 1, 1, 0);
            rightLine.endColor = Color.cyan;

            PickupVacuumable rightPickuper = rightHand.AddComponent<PickupVacuumable>();
            rightPickuper.origin = rightPickupOrigin.transform;
            rightPickuper.raycastOrigin = rightHand.transform.Find("laser origin");
            rightPickuper.line = rightLine;
            rightPickuper.skeletonAction = SteamVR_Actions.global.pose_right;
            rightPickuper.grabAction = SteamVR_Actions.global.grab_right;
            rightPickuper.originScaler = -1f;

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
            toggler.leftPickuper = leftPickuper;
            toggler.rightPickuper = rightPickuper;
            toggler.UpdateHandStates();

            CurrentVRRig = controllers;

            return controllers.transform;
        }
    }
}