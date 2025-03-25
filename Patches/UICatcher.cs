using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SRVR.Components;
using UnityEngine;
using UnityEngine.UI;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    public class UICatcher
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
          
            canvas.transform.localScale = Vector3.one * 0.0005f;
            vp_Layer.Set(canvas.gameObject, LayerMask.NameToLayer("Weapon"), true );
    
            canvas.gameObject.AddComponent<UIPositioner>();        
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
            "HudUI"
        };
    }
}