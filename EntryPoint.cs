using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SRML;
using SRML.Config.Attributes;
using SRML.Console;
using SRML.SR;
using SRML.SR.Utils.Debug;
using SRVR.Components;
using SRVR.Patches;
using Steamworks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using Console = SRML.Console.Console;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace SRVR
{
    [ConfigFile("VR")]
    
    public class VRConfig
    {
        public static bool SWITCH_HANDS = false;
        public static bool STATIC_UI_POSITION = true;
        public static bool SNAP_TURN = false;
        public static bool DISTANCE_GRAB = true;
        
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
        
        public EntryPoint()
        {
            VRInstaller.Install();
        }
        public override void PreLoad()
        {
            if (!VRInstaller.IsAfterInstall)
            {
                SRCallbacks.OnMainMenuLoaded += menu =>
                {
                    var findObjectOfType = Object.FindObjectOfType<UITemplates>();
                    var steamManager = AccessTools.TypeByName("SteamManager");
                    if (steamManager != null)
                    {
                        if ((bool)steamManager.GetProperty("Initialized").GetValue(null))
                        {
                            var steamApps = AccessTools.TypeByName("SteamApps");
                            var invoke = Activator.CreateInstance(AccessTools.TypeByName("AppId_t"), 939480u);
                            if ((bool)AccessTools.Method(steamApps, "BIsDlcInstalled").Invoke(null, new[]
                                {
                                    invoke
                                }))
                            {
                                var confirmDialog = findObjectOfType.CreateConfirmDialog(
                                    "Slime Rancher: VR Playground is installed!\n Do you want to uninstall it to have the ability to run Slime Rancher from SteamVR?",
                                    () =>
                                    {
                                        AccessTools.Method(steamApps, "UninstallDLC").Invoke(null, new[]
                                        {
                                            invoke
                                        });
                                        Shutdown();
                                    });
                                var buttonClickedEvent = new Button.ButtonClickedEvent();
                                confirmDialog.transform.Find("MainPanel/CancelButton").GetComponent<Button>().onClick =
                                    buttonClickedEvent;
                                buttonClickedEvent.AddListener(Shutdown);
                                return;
                            }
                        }
                    }

                    Shutdown();
                    return;


                    void Shutdown()
                    {
                        var errorDialog = findObjectOfType.CreateErrorDialog(
                            "Please restart your game to fully initialize VR. This is only on first install. The button below will quit the game.");
                        var button = errorDialog.transform.Find("MainPanel/OKButton").GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        button.onClick.AddListener(() =>
                        {
                            Debug.LogError("   ");
                            Debug.LogError("   ");
                            Debug.LogError(
                                "[SRVR] The shutdown of the game was caused by the SRVR mod because it was installed for the first time. This only happens on the first installation.");
                            Debug.LogError("   ");
                            Debug.LogError("   ");
                            if (typeof(ConsoleWindow).GetMethod("OnGUI",AccessTools.all) != null)
                            {
                                string installDirectory = new DirectoryInfo(Application.dataPath).Parent.FullName;
                                Process.Start(VRInstaller.VRInstallerPath, $"\"{installDirectory}\" install");
                            }

                            Application.Quit();

 
                        });
                    }
                };
                return;
            }

            if (typeof(ConsoleWindow).GetMethod("OnGUI", AccessTools.all) != null)
            {
                EntryPoint.ConsoleInstance.LogWarning("Optimization Fixes did not work properly, please try to reinstall the game and mod.");
            }
            
            HarmonyInstance.PatchAll();
            foreach (var patchedMethod in HarmonyInstance.GetPatchedMethods())
            {
                EntryPoint.ConsoleInstance.Log(patchedMethod.ToString());
            }
            TranslationPatcher.AddUITranslation("b.uninstall_srvr", "Uninstall SRVR");
            TranslationPatcher.AddUITranslation("b.snapturn", "Turn Snap Turn");
            TranslationPatcher.AddUITranslation("b.switch_hands", "Switch Hands");

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
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            var pediaModel = GameObject.Find("Art").transform.Find("BeatrixMainMenu/slimepedia").gameObject.Instantiate();
            pediaModel.DontDestroyOnLoad();
            pediaModel.name = "PediaInteract";
            pediaModel.AddComponent<MeshRenderer>().material = pediaModel.GetComponent<SkinnedMeshRenderer>().material;
            pediaModel.AddComponent<MeshFilter>().mesh = pediaModel.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            Object.Destroy(pediaModel.GetComponent<SkinnedMeshRenderer>());
            
            var pediaColObj = new GameObject("Collision") { transform = { parent = pediaModel.transform } };
            
            pediaColObj.AddComponent<BoxCollider>().isTrigger = true;
            pediaColObj.AddComponent<PediaInteract>();
            pediaColObj.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            
            pediaColObj.transform.localScale = new Vector3(0.35f, 0.1f, 0.35f);
            pediaColObj.transform.localPosition = new Vector3(0f, 0.2f, -0.05f);
            pediaColObj.transform.localEulerAngles = Vector3.zero;
            
            pediaModel.SetActive(false);
            
            PediaInteract.pediaModel = pediaModel;
            
            SRCallbacks.OnMainMenuLoaded += menu =>
            {
                var fpsCamera = GameObject.Find("FPSCamera");
                
                var camera = new GameObject("Camera")
                {
                    transform = // rotation Y should be 260 as far as ive seen.
                    {
                        position = new Vector3(14.26f, 1.1f, 3.98f),
                        eulerAngles = new Vector3(0, 260f, 0),
                    }
                };
                
                Patch_vp_FPInput.adjustmentDegrees = 260f;
                fpsCamera.transform.parent = camera.transform;
                fpsCamera.transform.localPosition = Vector3.zero;
                fpsCamera.transform.localEulerAngles = Vector3.zero;
                var controllers = VRManager.InstantiateVRRig();
                controllers.transform.SetParent(camera.transform, false);
                vp_Layer.Set(controllers.gameObject, vp_Layer.Actor, true);
                fpsCamera.AddComponent<SteamVR_TrackedObject>().index = SteamVR_TrackedObject.EIndex.Hmd; 
                menu.transform.Find("MessageOfTheDay").gameObject.SetActive(false);
            };

            Patch_LoadingUI.backgroundSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((x) => x.name == "UISprite");
        }
    }
}

    