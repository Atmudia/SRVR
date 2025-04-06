using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SRML.Console;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace SRVR
{
    public static class VRInstaller
    {
        public static bool IsAfterInstall = true;
        public static string VRInstallerPath;
        private static readonly List<Assembly> Assemblies = new List<Assembly>();
        private static bool _applyPatches = false;
        public static bool TypeByNamePatch(string className, ref Type __result)
        {
            __result = AccessTools.TypeByName(className);
            return false;
        }

        public static void InstallPatch()
        {
            
            Harmony harmony = new Harmony("SRVR.Installer");
            harmony.Patch(typeof(OpenVRHelpers).GetMethod("GetType", new []
            {
                typeof(string), typeof(bool)
            }), prefix: new HarmonyMethod(typeof(VRInstaller).GetMethod(nameof(TypeByNamePatch))));
        }
        public static void Install()
        {
            var unitySubsystemsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "UnitySubsystems"));
            var pluginsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "Plugins", "x86_64"));
            var streamingAssetsDirectory = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, "SteamVR"));

            var execAssembly = typeof(EntryPoint).Assembly;
            VRInstallerPath = Path.Combine(Application.dataPath, "SRVRInstaller.exe");
            if (unitySubsystemsDirectory.Exists && File.Exists(Path.Combine(pluginsDirectory.FullName, "openvr_api.dll")) && streamingAssetsDirectory.Exists && File.Exists(VRInstallerPath))
            {
                IsAfterInstall = true;
            }
            else
            {
                IsAfterInstall = false;
                unitySubsystemsDirectory.Create();
            
                // Create the "SteamVR" directory under StreamingAssets
                if (!streamingAssetsDirectory.Exists)
                    streamingAssetsDirectory.Create();
            
                // Loop through all resource names
                foreach (var manifestResourceName in execAssembly.GetManifestResourceNames())
                {
                    if (manifestResourceName.Contains("SRVRInstaller.exe"))
                    {
                        string UnitySubSystems = "SRVRInstaller.exe";
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close(); // Ensure the stream is closed after reading
                        // Combine path for UnitySubsystems
                        var filePath = Path.Combine(Application.dataPath, UnitySubSystems);
                        File.WriteAllBytes(filePath, ba);
                    }
                    
                    
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

                }
            }
            foreach (var manifestResourceName in execAssembly.GetManifestResourceNames())
            {
                if (manifestResourceName.Contains("Managed"))
                {
                    var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                    byte[] ba = new byte[manifestResourceStream.Length];
                    _ = manifestResourceStream.Read(ba, 0, ba.Length);
                    manifestResourceStream.Close(); // Ensure the stream is closed after reading
                    Assemblies.Add(Assembly.Load(ba));
                    
                }
            }
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return Assemblies.FirstOrDefault(assembly => AssemblyName.GetAssemblyName(args.Name).FullName == assembly.FullName);
            };
            InstallPatch();
          
        }

        public static Exception Uninstall()
        {
            try
            {
                var unitySubsystemsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "UnitySubsystems"));
                var pluginsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "Plugins", "x86_64"));
                var streamingAssetsDirectory = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, "SteamVR"));
                if (unitySubsystemsDirectory.Exists)
                    unitySubsystemsDirectory.Delete(true);
                if (streamingAssetsDirectory.Exists)
                    streamingAssetsDirectory.Delete(true);
                File.Delete(Path.Combine(pluginsDirectory.FullName, "openvr_api.dll"));
                File.Delete(Path.Combine(pluginsDirectory.FullName, "XRSDKOpenVR.dll"));
                File.Delete(typeof(EntryPoint).Assembly.Location);
                return null;
            }
            catch (Exception e)
            {
                EntryPoint.ConsoleInstance.Log(e);
                return e;
            }
          
        }

        public static void UninstallDLC(UITemplates uiTemplates)
        {
            if (!uiTemplates) return;

            var steamManagerType = AccessTools.TypeByName("SteamManager");
            if (steamManagerType == null) return;

            var initializedProperty = steamManagerType.GetProperty("Initialized");
            if (initializedProperty == null || !(bool)initializedProperty.GetValue(null)) return;

            var steamAppsType = AccessTools.TypeByName("SteamApps");
            if (steamAppsType == null) return;

            var appIdType = AccessTools.TypeByName("AppId_t");
            if (appIdType == null) return;

            var appIdInstance = Activator.CreateInstance(appIdType, 939480u);
            var isDlcInstalledMethod = AccessTools.Method(steamAppsType, "BIsDlcInstalled");
            if (isDlcInstalledMethod == null || !(bool)isDlcInstalledMethod.Invoke(null, new[] { appIdInstance }))
            {
                InstallOptimizationPatches(uiTemplates);
                return;
            }

            var confirmDialog = uiTemplates.CreateConfirmDialog(
                "Slime Rancher: VR Playground is installed!\nDo you want to uninstall it to have the ability to run Slime Rancher from SteamVR?",
                () =>
                {
                    var uninstallMethod = AccessTools.Method(steamAppsType, "UninstallDLC");
                    uninstallMethod?.Invoke(null, new[] { appIdInstance });
                    InstallOptimizationPatches(uiTemplates);
                });
            var cancelButton = confirmDialog.transform.Find("MainPanel/CancelButton")?.GetComponent<Button>();
            if (cancelButton != null)
            {
                var buttonClickedEvent = new Button.ButtonClickedEvent();
                cancelButton.onClick = buttonClickedEvent;
                cancelButton.onClick.AddListener(() => InstallOptimizationPatches(uiTemplates));
            }
        }

        public static void InstallOptimizationPatches(UITemplates uiTemplates)
        {
            if (typeof(ConsoleWindow).GetMethod("OnGUI", AccessTools.all) != null ||
                typeof(GameController).GetMethod("OnGUI", AccessTools.all) != null)
            {
                var confirmDialog = uiTemplates.CreateConfirmDialog(
                    "Do you want to install patches to base game files to make the game more performance? This will disable SRML Console. This may cause Anti-Virus False Flag",
                    () =>
                    {
                        _applyPatches = true;
                        Shutdown(uiTemplates);
                    });
                var cancelButton = confirmDialog.transform.Find("MainPanel/CancelButton")?.GetComponent<Button>();
                if (cancelButton != null)
                {
                    var buttonClickedEvent = new Button.ButtonClickedEvent();
                    cancelButton.onClick = buttonClickedEvent;
                    cancelButton.onClick.AddListener(() => Shutdown(uiTemplates));
                }
            }
            else Shutdown(uiTemplates);
            
           
        }
        public static void Shutdown(UITemplates UITemplates)
        {
            var errorDialog = UITemplates.CreateErrorDialog(
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
                if (_applyPatches)
                {
                    string installDirectory = new DirectoryInfo(Application.dataPath).Parent.FullName;
                    Process.Start(VRInstaller.VRInstallerPath, $"install \"{installDirectory}\"");
                }
              
                Application.Quit();
            });
        }
    }
}