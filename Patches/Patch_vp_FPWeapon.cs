using HarmonyLib;
using SRVR.Components;
using UnityEngine;
using UnityEngine.UI;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(vp_FPWeapon))]
    public class Patch_vp_FPWeapon
    {
        public static Transform FPWeapon;
        public static Transform FPInteract;

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

            var death = hudUIContainer.parent.Find(nameof(DeathObscurer)) as RectTransform;
            death.gameObject.AddComponent<CanvasScaler>();
            death.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            death.localScale = Vector3.one * 99f;
            death.SetParent(null);

            var deathText = SceneContext.Instance.player.GetComponent<PlayerDeathHandler>().deathUIPrefab;
            deathText.transform.GetChild(0).localPosition += Vector3.back * 0.05f;
            deathText.transform.GetChild(1).localPosition += Vector3.back * 0.05f;
            
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


            var controllers = VRManager.InstantiateVRRig();
            controllers.transform.SetParent(simplePlayer.transform, false);
            controllers.transform.localPosition = Vector3.zero;
            var rightController = controllers.Find("Right Controller");
            var leftController = controllers.Find("Left Controller");
            var leftHand = leftController.Find("Left Hand");
            scaler.SetParent(rightController, false); // Parent it to right hand
            FPWeapon = scaler;
            if (!EntryPoint.EnabledVR)
                rightController.transform.position = simplePlayer.transform.position + new Vector3(0, 1f, 0);
            
            
            var fpsCamera = simplePlayer.transform.Find("FPSCamera");
            var vacShapeCache = fpsCamera.transform.Find("vac shape").transform;
            var vacconePrefab = vacShapeCache.transform.Find("Vaccone Prefab");

            vacShapeCache.GetComponent<DynamicBone>().enabled =
                false; // I dont see why this is disabled. when enabled in the middle of gameplay it works, but im sure it should work.
            vacShapeCache.parent = scaler.transform;
            vacShapeCache.localRotation = Quaternion.Euler(new Vector3(-7118.613f, -8.3496f, -1.2340469f)); // Thanks Tranfox for this values
            vacShapeCache.localPosition = new Vector3(0.01f, -0.0071f, 0.0682f);
            vacShapeCache.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            vacconePrefab.localPosition = Vector3.zero;

            scaler.Find("arms").gameObject.SetActive(false);
            scaler.Find("mesh_l_armextra").gameObject.SetActive(false);

            scaler.localPosition = new Vector3(-0.2f, 0.35f, 0.1f);

            fpsCamera.gameObject.AddComponent<PlayerVRPos>();

            var weaponCamera = fpsCamera.transform.Find("WeaponCamera");
            weaponCamera.GetComponent<Camera>().nearClipPlane = 0.05f;
            FPInteract = leftController;
            
            
            
            // FPInteract = EntryPoint.Controllers.transform.Find("Left Controller").gameObject;

            var pedia = PediaInteract.pediaModel.Instantiate();

            pedia.transform.SetParent(scaler2.transform, false);
            pedia.transform.localScale = Vector3.one;
            pedia.transform.localPosition = new Vector3(0.46f, -0.06f, -0.08f);
            pedia.transform.localEulerAngles = new Vector3(450.6109f, 269.9038f, 57.2968f);
            pedia.SetActive(true);
            pedia.layer = LayerMask.NameToLayer("Weapon");
            fpsCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("Held");
            
            var uiCollider = new GameObject("UICollider")
            {
                transform =
                {
                    parent = scaler2.transform,
                    localPosition = hudUITransform.localPosition,
                    localRotation = hudUITransform.localRotation,
                    localScale = new Vector3(1.3f, 1.2f, 0.4f)
                }
            };
            uiCollider.AddComponent<BoxCollider>().isTrigger = true;
            uiCollider.AddComponent<HUDTouchBounds>();

            
            var slots = hudUIContainer.Find("Ammo Slots");
            for (int i = 0; i < slots.childCount; i++)
            {
                var slot = slots.GetChild(i);
                var col = new GameObject("SlotCollider")
                {
                    transform =
                    {
                        localScale = Vector3.one * 0.03f,
                    }
                };
                // col.transform.SetParent(slot.transform, false);
                var boxCollider = col.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;;
                
                col.SetActive(false);
                var ammoSlotTouchUI = col.AddComponent<AmmoSlotTouchUI>();
                ammoSlotTouchUI.slotIDX = i;
                ammoSlotTouchUI.slotObject = slot;
                col.SetActive(true);
               
            }
            
             
            VRDeathHandler.EventAdd();

            // var vacCollider = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            // vacCollider.transform.SetParent(scaler2.transform, false);
            // vacCollider.transform.localScale = Vector3.one;
            // vacCollider.transform.localPosition = new Vector3(0.35f, -0.1f, 0f);
            // vacCollider.transform.localRotation = Quaternion.Euler(0,0,90);
            // // vacCollider.RemoveComponent<MeshRenderer>();
            // vacCollider.layer = vp_Layer.PenWalls;
            //var glueTrigger = vacCollider.AddComponent<SphereCollider>();
            //glueTrigger.radius = 0.8f;
            //glueTrigger.center = new Vector3(2f, -0.1f, 0f);
            //glueTrigger.isTrigger = true;
            //vacCollider.AddComponent<ActorGlue>(); // i think this will be funny; balancing largo mini-game
        }
    }
}