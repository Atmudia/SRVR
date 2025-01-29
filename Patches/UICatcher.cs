using System.Linq;
using HarmonyLib;
using SRML.Console;
using UnityEngine;
using UnityEngine.UI;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    public class UICatcher
    {
        private static RenderTexture uGuiTexture;
        public static void Prefix(CanvasScaler __instance)
        {
            if (!EntryPoint.EnabledVR)
                return;
            var canvas = __instance.GetComponent<Canvas>();
            if (!Camera.main || IsCanvasToIgnore(__instance.name)) return;
            if (!canvas) return;
            if (canvas.renderMode == RenderMode.WorldSpace) return;
            canvas.renderMode = RenderMode.WorldSpace;
          
            canvas.transform.localScale = Vector3.one * 0.0005f;
            vp_Layer.Set(canvas.gameObject, LayerMask.NameToLayer("Weapon"), true );
    
            canvas.gameObject.AddComponent<Components>();        
        }
        private static bool IsCanvasToIgnore(string canvasName)
        {
            return canvasesToIgnore.Contains(canvasName);
        }
        private static readonly string[] canvasesToIgnore =
        {
            "com.sinai.unityexplorer_Root", // UnityExplorer.
            "com.sinai.unityexplorer.MouseInspector_Root", // UnityExplorer.
            "ExplorerCanvas",
            "HudUI",
            "MainMenuUI"
        };
    }
}