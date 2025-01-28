using System;
using System.Linq;
using HarmonyLib;
using SRML;
using SRML.Console;
using SRML.SR;
using SRML.Utils;
using SRVR.Patches;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace SRVR
{
    public class EntryPoint : ModEntryPoint
    {
        public static Console.ConsoleInstance ConsoleInstance = new Console.ConsoleInstance("SRVR");
        public static AssetBundle VRAssets = AssetBundle.LoadFromStream(typeof(EntryPoint).Assembly.GetManifestResourceStream("SRVR.vrassets"));//Temporary
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
                            Application.Quit();
                        });
                    }
                };
                return;
            }

            Console.RegisterCommand(new ContinueGameCommand());
            Console.RegisterCommand(new UnparentVacGun());
            Console.RegisterCommand(new ParentVacGun());
            HarmonyInstance.PatchAll();
            TranslationPatcher.AddUITranslation("b.uninstall_vr", "Uninstall VR");
            if (!EnabledVR) return;
            if (!VRManager.InitializeVR()) return;
            if (!VRManager.StartVR()) return;
            VRInput.RegisterCallbacks();

            //return;
            Controllers = new GameObject(nameof(Controllers));
            Controllers.transform.localPosition = Vector3.zero;
            var arms = VRAssets.LoadAsset<Mesh>("arms");
            var handsMaterial = VRAssets.LoadAsset<Material>("Hands Material 1");
            var leftController = new GameObject("Left Controller")
            {
                transform =
                {
                    parent = Controllers.transform
                }
            };
            var lefthand_alone = new GameObject("Left Hand")
            {
                transform =
                {
                    parent = leftController.transform,
                    position = new Vector3(0, 0, -0.1f),
                    rotation = Quaternion.Euler(0, 90, 0),
                }
            };
            lefthand_alone.AddComponent<MeshRenderer>().sharedMaterial = handsMaterial;
            lefthand_alone.AddComponent<MeshFilter>().sharedMesh = arms;
            leftController.AddComponent<PosHand>().hand = XRNode.LeftHand;
            var rightController = new GameObject("Right Controller")
            {
                transform = { parent = Controllers.transform }
            };
            var righthand_alone = new GameObject("Right Hand")
            {
                transform =
                {
                    parent = rightController.transform,
                    position = new Vector3(0, 0, -0.1f),
                    rotation = Quaternion.Euler(0, 90, 0),
                }
            };
            righthand_alone.AddComponent<MeshRenderer>().sharedMaterial = handsMaterial;
            righthand_alone.AddComponent<MeshFilter>().sharedMesh = arms;
            rightController.AddComponent<PosHand>().hand = XRNode.RightHand;

            SRCallbacks.OnMainMenuLoaded += menu =>
            {
                var fpsCamera = GameObject.Find("FPSCamera");
                var camera = new GameObject("Camera")
                {
                    transform =
                    {
                        position = fpsCamera.transform.position
                    }
                };
                fpsCamera.transform.parent = camera.transform;
                fpsCamera.transform.localEulerAngles = Vector3.zero;
                Controllers.transform.SetParent(camera.transform, false);
                vp_Layer.Set(Controllers, vp_Layer.Actor, true);
                fpsCamera.AddComponent<RotHMD>(); 
                
                var mainMenuUI = GameObject.Find("MainMenuUI").GetComponent<Canvas>();
                mainMenuUI.renderMode = RenderMode.WorldSpace;
                mainMenuUI.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
                mainMenuUI.transform.localPosition = new Vector3(12.3258f, 1.8956f, 3.7663f);
                mainMenuUI.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            };


        }

        public class RotHMD : MonoBehaviour
        {
            public void Update()
            {
                InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                if (head.TryGetFeatureValue(CommonUsages.deviceRotation, out var rot))
                {
                    this.transform.localRotation = rot;
                }
                if (head.TryGetFeatureValue(CommonUsages.devicePosition, out var pos))
                {
                    this.transform.localPosition = pos;
                }
            }
        }

        public static GameObject Controllers;
        



    }

    public class ContinueGameCommand : ConsoleCommand
    {
        public override bool Execute(string[] args)
        {
            GameData.Summary saveToContinue = SRSingleton<GameContext>.Instance.AutoSaveDirector.GetSaveToContinue();
            SRSingleton<GameContext>.Instance.AutoSaveDirector.BeginLoad(saveToContinue.name, saveToContinue.saveName, (Action) (() =>
            {
            }));
            return true;
        }

        public override string ID => "continue_game";
        public override string Usage => ID;
        public override string Description => ID;
    }
    public class UnparentVacGun : ConsoleCommand
    {
        public override bool Execute(string[] args)
        {
            if (!ParentVacGun.Parent)
                ParentVacGun.Parent = Patch_vp_FPWeapon.FPWeapon.transform.parent;
            Patch_vp_FPWeapon.FPWeapon.transform.SetParent(null);
            return true;
        }

        public override string ID => "unparent_vacgun";
        public override string Usage => ID;
        public override string Description => ID;
    }
    public class ParentVacGun : ConsoleCommand
    {
        public static Transform Parent;
        public override bool Execute(string[] args)
        {
            Patch_vp_FPWeapon.FPWeapon.transform.SetParent(Parent);
            return true;
        }

        public override string ID => "parent_vacgun";
        public override string Usage => ID;
        public override string Description => ID;
    }
    
}

    