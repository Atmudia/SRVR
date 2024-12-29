using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using InControl;
using SRML;
using SRML.Console;
using SRML.SR;
using SRVR.Patches;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using Console = SRML.Console.Console;
using InputDevice = UnityEngine.XR.InputDevice;
using Object = UnityEngine.Object;

namespace SRVR
{
    public class EntryPoint : ModEntryPoint
    {
        public static Console.ConsoleInstance ConsoleInstance = new Console.ConsoleInstance("SRVR");
        public static bool EnabledVR = true;
        public static Camera mainCamera;
        


        public static bool PreInitializedLoaded = false;

        public EntryPoint()
        {
            
           DirectoryInfo unitySubsystemsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "UnitySubsystems"));
            DirectoryInfo pluginsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "Plugins", "x86_64"));
            DirectoryInfo libsPath = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "SRML", "Libs"));
            bool steamVRExist = File.Exists(Path.Combine(libsPath.FullName, "SteamVR.dll"));

            var execAssembly = typeof(EntryPoint).Assembly;

            if (!unitySubsystemsDirectory.Exists || !pluginsDirectory.Exists || !steamVRExist)
            {
                PreInitializedLoaded = false;
                unitySubsystemsDirectory.Create();
                libsPath.Create();

                // Create the "SteamVR" directory under StreamingAssets
                DirectoryInfo streamingAssetsDirectory =
                    new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, "SteamVR"));
                if (!streamingAssetsDirectory.Exists)
                    streamingAssetsDirectory.Create();

                // Loop through all resource names
                foreach (var manifestResourceName in execAssembly.GetManifestResourceNames())
                {
                    if (manifestResourceName.Contains("UnitySubsystems"))
                    {
                        string UnitySubSystems = "UnitySubsystemsManifest.json";
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close(); // Ensure the stream is closed after reading

                        // Combine path for UnitySubsystems
                        var xrSdkOpenVrDirectory = unitySubsystemsDirectory.CreateSubdirectory("XRSDKOpenVR");
                        var filePath = Path.Combine(xrSdkOpenVrDirectory.FullName, UnitySubSystems);
                        Debug.Log(filePath);
                        File.WriteAllBytes(filePath, ba);
                    }

                    if (manifestResourceName.Contains("Plugins"))
                    {
                        string nameOfFile = manifestResourceName.Replace("SRVR.Files.Plugins.", string.Empty);
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close();

                        // Ensure the plugin directory exists
                        if (!pluginsDirectory.Exists)
                            pluginsDirectory.Create();

                        var filePath = Path.Combine(pluginsDirectory.FullName, nameOfFile);
                        File.WriteAllBytes(filePath, ba);
                    }

                    if (manifestResourceName.Contains("SteamVRFiles"))
                    {
                        string nameOfFile = manifestResourceName.Replace("SRVR.Files.SteamVRFiles.", string.Empty);
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close();

                        // Ensure the streamingAssetsDirectory exists
                        if (!streamingAssetsDirectory.Exists)
                            streamingAssetsDirectory.Create();

                        var filePath = Path.Combine(streamingAssetsDirectory.FullName, nameOfFile);
                        File.WriteAllBytes(filePath, ba);
                    }
                    if (manifestResourceName.Contains("Managed"))
                    {
                        string nameOfFile = manifestResourceName.Replace("SRVR.Files.Managed.", string.Empty);
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        manifestResourceStream.Read(ba, 0, ba.Length);
                    
                        var filePath = Path.Combine(libsPath.FullName, nameOfFile);
                        File.WriteAllBytes(filePath, ba);
                    }
                }
            }
            else
            {
                PreInitializedLoaded = true;
            }
            



        }

        public override void PreLoad()
        {
            if (!PreInitializedLoaded)
            {
                SRCallbacks.OnMainMenuLoaded += menu =>
                { 
                    var errorDialog = Object.FindObjectOfType<UITemplates>().CreateErrorDialog("Please restart your game to fully initialize VR. This is only on first install. The button below will quit the game.");
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
                };
                return;
            }
            
            HarmonyInstance.PatchAll();
            Console.RegisterCommand(new ContinueGameCommand());
            Console.RegisterCommand(new UnparentVacGun());
            Console.RegisterCommand(new ParentVacGun());

            if (EnabledVR)
                if (VRManager.InitializeVR())
                {
                    if (VRManager.StartVR())
                    {
                        VRInput.RegisterCallbacks();
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

    