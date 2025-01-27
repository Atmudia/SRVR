using System;
using System.IO;
using SRML;
using UnityEngine;

namespace SRVR
{
    public static class VRInstaller
    {
        public static bool IsAfterInstall;
        public static void Install()
        {
            var unitySubsystemsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "UnitySubsystems"));
            var pluginsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "Plugins", "x86_64"));
            var libsPath = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "SRML", "Libs"));
            var steamVRExist = File.Exists(Path.Combine(libsPath.FullName, "SteamVR.dll"));
            var execAssembly = typeof(EntryPoint).Assembly;
            if (unitySubsystemsDirectory.Exists && pluginsDirectory.Exists && steamVRExist)
            {
                IsAfterInstall = true;
            }
            else
            {
                IsAfterInstall = false;
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
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close();
                        var filePath = Path.Combine(libsPath.FullName, nameOfFile);
                        File.WriteAllBytes(filePath, ba);
                    }
                }
            }
        }
    }
}