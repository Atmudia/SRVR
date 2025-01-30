using System.Runtime.CompilerServices;
using HarmonyLib;
using InControl;
using MonomiPark.SlimeRancher.DataModel;
using SRML.Console;
using SRML.SR;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(vp_FPWeapon))]
    public class Patch_vp_FPWeapon
    {
        public static GameObject FPWeapon;
        public static GameObject FPInteract;
        [HarmonyPatch(nameof(vp_FPWeapon.Start))]
        public static void Postfix(vp_FPWeapon __instance)
        {
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
            
            hudUIContainer.Find("PartnerArea").GetComponent<RectTransform>().SetAnchoredPosition2D(580, -552.0001f);
            
            hudUIContainer.Find("TimeIcon").GetComponent<RectTransform>().SetAnchoredPosition2D(580, -502.0001f);
            
            hudUIContainer.Find("CurrentDay").GetComponent<RectTransform>().SetAnchoredPosition2D(580, -446.0001f);
            
            hudUIContainer.Find("CurrentTime").GetComponent<RectTransform>().SetAnchoredPosition2D(632f, -498);

            hudUIContainer.Find("Ammo Slots").GetComponent<HorizontalLayoutGroup>().spacing = 80f;
            
            hudUIContainer.Find("crossHair").gameObject.SetActive(false);
            
            var model_vac_prefab = __instance.transform.GetChild(0);
            var scaler = model_vac_prefab.transform.Find("Scaler");
            var bone_vac = scaler.transform.Find("bone_vac");
            
            scaler.localScale = new Vector3(12.5f, 12.5f, 12.5f);
            scaler.localRotation = Quaternion.Euler(15, 10, 0);
            
            
            // hudUITransform.SetParent(model_vac_prefab, false);
            var scaler2 = new GameObject("Scaler")
            {
                transform = { localScale = new Vector3(0.03f, 0.03f, 0.03f) }
            };
            scaler2.transform.SetParent(bone_vac, false);

            hudUITransform.SetParent(scaler2.transform, true);
            hudUITransform.localPosition = new Vector3(0.1455f, 0.3708f, 0.0003f);
            hudUITransform.localRotation = Quaternion.Euler(new Vector3(36.823f, 90.00001f, 0));
            hudUITransform.gameObject.layer = LayerMask.NameToLayer("Weapon");
            var canvas = hudUITransform.GetComponentInParent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
           
            hudUITransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);


            var simplePlayer = GameObject.Find("SimplePlayer");
            var rightHand = new GameObject("Controller (Right)")
            {
                transform = { parent = simplePlayer.transform}
            };
            FPWeapon = rightHand;
            scaler.SetParent(rightHand.transform, false);// Parent it to right hand
            rightHand.SetActive(false);
            if (EntryPoint.EnabledVR)
                rightHand.AddComponent<PosHand>().hand = XRNode.RightHand;
            else
            {
                rightHand.transform.position = simplePlayer.transform.position + new Vector3(0, 1f, 0);
            }
            rightHand.SetActive(true);

            var fpsCamera = simplePlayer.transform.Find("FPSCamera");
            var vacShapeCache = fpsCamera.transform.Find("vac shape").transform;
            var vacconePrefab = vacShapeCache.transform.Find("Vaccone Prefab");

            vacShapeCache.GetComponent<DynamicBone>().enabled = false; // I dont see why this is disabled. when enabled in the middle of gameplay it works, but im sure it should work.
            vacShapeCache.parent = scaler.transform;
            vacShapeCache.localRotation = Quaternion.Euler(new Vector3(-7118.613f, -8.3496f, -1.2340469f)); // Thanks Tranfox for this values
            vacShapeCache.localPosition = new Vector3(0.01f, -0.0071f, 0.0682f);
            vacShapeCache.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            vacconePrefab.localPosition = Vector3.zero;

            scaler.Find("arms").gameObject.SetActive(false);
            scaler.Find("mesh_l_armextra").gameObject.SetActive(false);
            
            scaler.localPosition = new Vector3(-0.2f, 0.35f, 0.1f);
            
            fpsCamera.gameObject.AddComponent<PlayerVRPos>();
            
            // Interaction
            
            // Code from EntryPoint
            var arms = EntryPoint.VRAssets.LoadAsset<Mesh>("arms");
            var handsMaterial = EntryPoint.VRAssets.LoadAsset<Material>("Hands Material 1");
            var leftHand = new GameObject("Controller (Left)")
            {
                transform =
                {
                    parent = simplePlayer.transform
                }
            };
            var leftHandModel = new GameObject("Model")
            {
                transform =
                {
                    parent = leftHand.transform,
                    position = new Vector3(0, 0, -0.1f),
                    rotation = Quaternion.Euler(0, 90, 0),
                }
            };
            leftHandModel.AddComponent<MeshRenderer>().sharedMaterial = handsMaterial;
            leftHandModel.AddComponent<MeshFilter>().sharedMesh = arms;
            leftHand.AddComponent<PosHand>().hand = XRNode.LeftHand;
            
            leftHand.layer = LayerMask.NameToLayer("Weapon");
            leftHandModel.layer = LayerMask.NameToLayer("Weapon");

            var leftHandCol = leftHand.AddComponent<BoxCollider>();
            leftHandCol.size = new Vector3(0.08f, 0.04f, 0.16f);
            leftHandCol.size = new Vector3(0f, 0f, -0.1f);
            
            leftHand.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            
            var weaponCamera = fpsCamera.transform.Find("WeaponCamera");
            weaponCamera.GetComponent<Camera>().nearClipPlane = 0.05f;
            
            FPInteract = leftHand;
            
            // Configs
            // Left handed mode
            if (VRConfig.SWITCH_HANDS)
            {
                rightHand.GetComponent<PosHand>().hand = XRNode.LeftHand;
                leftHand.GetComponent<PosHand>().hand = XRNode.RightHand;
                // TODO: Add different interact hand models for both controllers
            }
            
            var pedia = PediaInteract.pediaModel.Instantiate();
            
            pedia.transform.SetParent(scaler2.transform, false);
            pedia.transform.localScale = Vector3.one;
            pedia.transform.localPosition = new Vector3(0.46f, -0.06f, -0.08f);
            pedia.transform.localEulerAngles = new Vector3(450.6109f, 269.9038f, 57.2968f);
            pedia.SetActive(true);
            pedia.layer = LayerMask.NameToLayer("Weapon");
        }
        
        
    }
}