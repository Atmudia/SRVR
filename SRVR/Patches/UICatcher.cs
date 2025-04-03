using System.Linq;
using HarmonyLib;
using SRVR.Components;
using UnityEngine;
using UnityEngine.UI;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    internal class UICatcher
    {
        public static void Prefix(CanvasScaler __instance)
        {
            if (!EntryPoint.EnabledVR)
                return;
            var canvas = __instance.GetComponent<Canvas>();
            if (!Camera.main || IsCanvasToIgnore(__instance.name)) return;
            if (!canvas) return;
            if (canvas.renderMode == RenderMode.WorldSpace)
                return;
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            if (Levels.isMainMenu())
            {
                canvas.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                canvas.transform.localPosition = new Vector3(12.3258f, 1.8956f, 3.7663f);
                canvas.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                return;
            }
            if (Patch_RanchHouseUI.Instance)
            {
                var component = Patch_RanchHouseUI.Instance.GetComponent<Canvas>();
                canvas.transform.localScale = Vector3.one * 0.0005f;
                canvas.transform.localPosition = component.transform.localPosition - new Vector3(0, 0, -0.1f);
                canvas.transform.localRotation = component.transform.localRotation;
                return;
            }
          
            canvas.transform.localScale = Vector3.one * 0.0005f;
            canvas.gameObject.AddComponent<UIPositioner>();        
        }
        private static bool IsCanvasToIgnore(string canvasName)
        {
            return CanvasesToIgnore.Contains(canvasName);
        }
        private static readonly string[] CanvasesToIgnore =
        {
            "com.sinai.unityexplorer_Root", // UnityExplorer.
            "com.sinai.unityexplorer.MouseInspector_Root", // UnityExplorer.
            "ExplorerCanvas",
            "HudUI"
        };
    }
}