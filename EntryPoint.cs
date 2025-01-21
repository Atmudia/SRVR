using System;
using HarmonyLib;
using SRML;
using SRML.Console;
using SRML.SR;
using SRVR.Patches;
using UnityEngine;
using UnityEngine.UI;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace SRVR
{
    public class EntryPoint : ModEntryPoint
    {
        public static Console.ConsoleInstance ConsoleInstance = new Console.ConsoleInstance("SRVR");
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
                                var confirmDialog = findObjectOfType.CreateConfirmDialog("Slime Rancher: VR Playground is installed!\n Do you want to uninstall it to have the ability to run Slime Rancher from SteamVR?",
                                    () =>
                                    {
                                        AccessTools.Method(steamApps, "UninstallDLC").Invoke(null, new []
                                        {
                                            invoke
                                        });
                                        Shutdown();
                                    });
                                var buttonClickedEvent = new Button.ButtonClickedEvent();
                                confirmDialog.transform.Find("MainPanel/CancelButton").GetComponent<Button>().onClick = buttonClickedEvent;
                                buttonClickedEvent.AddListener(Shutdown);
                                return;
                            }
                        }
                    }
                    Shutdown();
                    return;


                    void Shutdown()
                    {
                        var errorDialog = findObjectOfType.CreateErrorDialog("Please restart your game to fully initialize VR. This is only on first install. The button below will quit the game.");
                        var button = errorDialog.transform.Find("MainPanel/OKButton").GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        button.onClick.AddListener(() =>
                        {
                            Debug.LogError("   ");
                            Debug.LogError("   ");
                            Debug.LogError("[SRVR] The shutdown of the game was caused by the SRVR mod because it was installed for the first time. This only happens on the first installation.");
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
            if (EnabledVR)
            {
                if (VRManager.InitializeVR())
                {
                    if (VRManager.StartVR())
                    {
                        VRInput.RegisterCallbacks();
                    }
                }
            }
            
            
              
            
            // MeshTextStyler
        }

        /*public override void Update()
        {
            if (Camera.main != EntryPoint.mainCamera && Levels.IsLevel(Levels.WORLD))
            {
                EntryPoint.mainCamera = Camera.main;
                EntryPoint.mainCamera.gameObject.AddComponent<SteamVR_CameraHelper>();
                // EntryPoint.mainCamera.gameObject.AddComponent<PosHMD>();
            }
        }*/

        
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
                ParentVacGun.Parent = Patch_HudUI.FPWeapon.transform.parent;
            Patch_HudUI.FPWeapon.transform.SetParent(null);
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
            Patch_HudUI.FPWeapon.transform.SetParent(Parent);
            return true;
        }

        public override string ID => "parent_vacgun";
        public override string Usage => ID;
        public override string Description => ID;
    }
    
}

    