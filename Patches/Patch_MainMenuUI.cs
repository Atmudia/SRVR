// using HarmonyLib;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace SRVR.Patches
// {
//     [HarmonyPatch(typeof(MainMenuUI), nameof(MainMenuUI.Awake))]
//     public static class Patch_MainMenuUI
//     {
//         public static Camera TestCamera;
//         public static void Prefix(MainMenuUI __instance)
//         {
//            
//             
//             foreach (var componentsInChild in __instance.GetComponentsInChildren<Canvas>(true))
//             {
//                 componentsInChild.renderMode = RenderMode.WorldSpace;
//                 componentsInChild.worldCamera = TestCamera;
//                 
//             }
//             Object.Destroy(__instance.GetComponent<CanvasScaler>());
//
//             __instance.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
//         }
//     }
// }