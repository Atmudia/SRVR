using System.Runtime.CompilerServices;
using HarmonyLib;
using InControl;
using MonomiPark.SlimeRancher.DataModel;
using SRML.Console;
using SRML.SR;
using UnityEngine;
using Valve.VR;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(vp_FPWeapon))]
    public class Patch_HudUI
    {
        public static GameObject FPWeapon;
        [HarmonyPatch(nameof(vp_FPWeapon.Start))]
        public static void Postfix(vp_FPWeapon __instance)
        {
            // SolarShieldUpgrader
          
            __instance.enabled = false;
            RectTransform hudUITransform = (RectTransform)SRSingleton<HudUI>.Instance.transform;
            var hudUIContainer = SRSingleton<HudUI>.Instance.uiContainer.transform;

            hudUIContainer.Find("HealthIcon").GetComponent<RectTransform>().SetAnchoredPosition2D(480.0001f, 264.3f);
            hudUIContainer.Find("Health Meter").GetComponent<RectTransform>().SetAnchoredPosition2D(551f, 269f);
            
            hudUIContainer.Find("EnergyIcon").GetComponent<RectTransform>().SetAnchoredPosition2D(1139, 265);
            hudUIContainer.Find("Energy Meter").GetComponent<RectTransform>().SetAnchoredPosition2D(1210, 269);
            
            hudUIContainer.Find("RadIcon").GetComponent<RectTransform>().SetAnchoredPosition2D(-164.0002f, 260f);
            hudUIContainer.Find("Rad Meter").GetComponent<RectTransform>().SetAnchoredPosition2D(-89f, 269f);
            
            hudUIContainer.Find("KeysIcon").GetComponent<RectTransform>().SetAnchoredPosition2D(1323f, 383f);            
            hudUIContainer.Find("Keys").GetComponent<RectTransform>().SetAnchoredPosition2D(1373f, 387f);
            
            hudUIContainer.Find("CurrencyIcon").GetComponent<RectTransform>().SetAnchoredPosition2D(472f, 361f);
            hudUIContainer.Find("Currency").GetComponent<RectTransform>().SetAnchoredPosition2D(548f, 356f);
            
            hudUIContainer.Find("MailIcon").GetComponent<RectTransform>().SetAnchoredPosition2D(1331f, -591f);
            
            hudUIContainer.Find("Targeting").GetComponent<RectTransform>().SetAnchoredPosition2D(-769f, -485.9f);
            
            hudUIContainer.Find("crossHair").gameObject.SetActive(false);
            
            var model_vac_prefab = __instance.transform.GetChild(0);
            var scaler = model_vac_prefab.transform.Find("Scaler");
            var bone_vac = scaler.transform.Find("bone_vac");
            
            scaler.localScale = new Vector3(25, 25, 25);
            scaler.localRotation = Quaternion.Euler(15, 10, 0);

            
            
            // hudUITransform.SetParent(model_vac_prefab, false);
            var scaler2 = new GameObject("Scaler")
            {
                transform = { localScale = new Vector3(0.04f, 0.04f, 0.04f) }
            };
            scaler2.transform.SetParent(bone_vac, false);

            hudUITransform.SetParent(scaler2.transform, true);
            // hudUITransform.localPosition = new Vector3(-0.1545f, 0.4708f, 0.0003f);
            hudUITransform.localPosition = new Vector3(0.1455f, 0.3708f, 0.0003f);
            // hudUITransform.localRotation = Quaternion.Euler(new Vector3(2.3379f, 72.3335f, 0.2003f));
            hudUITransform.localRotation = Quaternion.Euler(new Vector3(36.823f, 90.00001f, 0));
            hudUITransform.gameObject.layer = LayerMask.NameToLayer("Weapon");
            var canvas = hudUITransform.GetComponentInParent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            hudUITransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            

            var rightHand = new GameObject("Controller (Right)")
            {
                transform = { parent = GameObject.Find("SimplePlayer").transform}
            };
            FPWeapon = rightHand;
            if (EntryPoint.EnabledVR)
            {
                scaler.SetParent(rightHand.transform, false);// Parent it to Scaler
                scaler.transform.localPosition = Vector3.zero;
                rightHand.SetActive(false);
                rightHand.AddComponent<PosHand>();
                rightHand.SetActive(true);
            }
            
            var vacShapeCache = GameObject.Find("SimplePlayer/FPSCamera/vac shape").transform;
            var _vacconeCache = vacShapeCache.transform.Find("Vaccone Prefab");

            vacShapeCache.GetComponent<DynamicBone>().enabled = false;
            vacShapeCache.parent = scaler.transform;
            vacShapeCache.localRotation = Quaternion.Euler(new Vector3(-7118.613f, -8.3496f, -1.2340469f)); //These 
            vacShapeCache.localPosition = new Vector3(0.01f, -0.0071f, 0.0682f);
            _vacconeCache.localPosition = Vector3.zero;
            
            scaler.Find("arms").gameObject.SetActive(false);
            scaler.Find("mesh_l_armextra").gameObject.SetActive(false);
        }
        
        
    }
}