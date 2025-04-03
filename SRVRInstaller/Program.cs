using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using AsmResolver.DotNet;

namespace SRVRInstaller
{
    public static class Program
    {
        private const string TargetProcess = "slimerancher";
        private const string NonVrSuffix = "_nonvr";
        
        public static void Main(string[] args)
        {
            if (!ValidateArguments(args)) return;
            
            WaitForProcessExit(TargetProcess);
            
            var (method, directoryPath) = (args[0], args[1]);
            ProcessDllFiles(directoryPath, method);
            
            Console.WriteLine("Processing complete.");
        }

        private static bool ValidateArguments(string[] args)
        {
            if (args.Length == 2) return true;
            
            Console.WriteLine("Usage: SRVRInstaller <install|uninstall> <target-directory>");
            return false;
        }

        private static void WaitForProcessExit(string processName)
        {
            Console.WriteLine("Waiting for game to close...");
            while (Process.GetProcessesByName(processName).Any())
            {
                Thread.Sleep(1000);
            }
        }

        private static void ProcessDllFiles(string directoryPath, string method)
        {
            foreach (var dllPath in Directory.EnumerateFiles(directoryPath, "*.dll", SearchOption.AllDirectories))
            {
                if (dllPath.EndsWith(NonVrSuffix)) continue;
                try
                {
                    if (method == "install") InstallPatch(dllPath);
                    else if (method == "uninstall") UninstallPatch(dllPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {dllPath}: {ex.Message}");
                }
            }
        }

        private static void InstallPatch(string dllPath)
        {
            var backupPath = GetBackupPath(dllPath);
            if (File.Exists(backupPath)) return;

            try
            {
                var module = ModuleDefinition.FromFile(dllPath);
                var modifiedTypes = module.GetAllTypes()
                    .Where(t => t.Methods.Any(m => (string)m.Name == "OnGUI"))
                    .ToList();

                if (!modifiedTypes.Any()) return;
                
                File.Copy(dllPath, backupPath);

                foreach (var type in modifiedTypes)
                {
                    var methodsToRemove = type.Methods.Where(m => (string)m.Name == "OnGUI").ToList();
                    foreach (var method in methodsToRemove)
                    {
                        type.Methods.Remove(method);
                    }
                }

                module.Write(dllPath);
                Console.WriteLine($"Patched original DLL and saved backup at: {backupPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing DLL {dllPath} during installation: {ex.Message}");
            }
        }

        private static void UninstallPatch(string dllPath)
        {
            var backupPath = GetBackupPath(dllPath);
            if (!File.Exists(backupPath)) return; // Skip if no backup exists

            try
            {
                File.Copy(backupPath, dllPath, overwrite: true);

                File.Delete(backupPath);
                Console.WriteLine($"Restored original DLL from backup: {dllPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring DLL {dllPath} during uninstallation: {ex.Message}");
            }
        }

        private static string GetBackupPath(string originalPath)
        {
            var directory = Path.GetDirectoryName(originalPath);
            var fileName = Path.GetFileNameWithoutExtension(originalPath);
            string newName;
            if (fileName.Contains(NonVrSuffix))
                newName = fileName;
            else
                newName = fileName + NonVrSuffix;

            return Path.Combine(directory, $"{newName}.dll");
        }
    }
}
