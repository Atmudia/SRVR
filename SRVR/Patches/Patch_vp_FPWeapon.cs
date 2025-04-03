using HarmonyLib;
using SRVR.Components;
using UnityEngine;
using UnityEngine.UI;

namespace SRVR.Patches
{
    [HarmonyPatch(typeof(vp_FPWeapon))]
    internal class Patch_vp_FPWeapon
    {
        [HarmonyPatch(nameof(vp_FPWeapon.Start))]
        public static void Postfix(vp_FPWeapon __instance)
        {
            __instance.enabled = false;

            GameObject simplePlayer = GameObject.Find("SimplePlayer");

            RectTransform hudUITransform = (RectTransform)SRSingleton<HudUI>.Instance.transform;
            Transform hudUIContainer = SRSingleton<HudUI>.Instance.uiContainer.transform;

            Transform model_vac_prefab = __instance.transform.GetChild(0);
            Transform scaler = model_vac_prefab.transform.Find("Scaler");
            Transform bone_vac = scaler.transform.Find("bone_vac");
            Transform fpsCamera = simplePlayer.transform.Find("FPSCamera");
            Transform vacShapeCache = fpsCamera.transform.Find("vac shape");
            Transform vacconePrefab = vacShapeCache.transform.Find("Vaccone Prefab");

            Transform controllers = VRManager.InstantiateVRRig();
            Transform rightController = controllers.Find("Right Controller");
            Transform leftController = controllers.Find("Left Controller");

            // set up VR rig

            GameObject scaler2 = new GameObject("Scaler")
            {
                transform =
                {
                    parent = bone_vac,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                    localScale = new Vector3(0.03f, 0.03f, 0.03f)
                }
            };

            controllers.SetParent(simplePlayer.transform, false);
            controllers.localPosition = Vector3.zero;

            // atmudia will replace with new system?
            // when/if that happens, make sure to account for either non-dominant hand, and one-handed setups
            LineRenderer addComponent = leftController.gameObject.AddComponent<LineRenderer>();
            addComponent.gameObject.AddComponent<LaserPointer>();

            GameObject pedia = PediaInteract.pediaModel.Instantiate();
            pedia.transform.SetParent(scaler2.transform, false);
            pedia.transform.localScale = Vector3.one;
            pedia.transform.localPosition = new Vector3(0.46f, -0.06f, -0.08f);
            pedia.transform.localEulerAngles = new Vector3(450.6109f, 269.9038f, 57.2968f);
            pedia.SetActive(true);
            pedia.layer = LayerMask.NameToLayer("Weapon");

            fpsCamera.Find("WeaponCamera").GetComponent<Camera>().nearClipPlane = 0.05f;
            fpsCamera.Find("WeaponCamera").GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("UI");
            fpsCamera.GetComponent<Camera>().cullingMask |= (1 << LayerMask.NameToLayer("Held")) | (1 << LayerMask.NameToLayer("Weapon"));
            fpsCamera.GetComponent<Camera>().cullingMask ^= 1 << LayerMask.NameToLayer("UI");
            fpsCamera.gameObject.AddComponent<PosHMD>().baseCam = fpsCamera.GetComponent<vp_FPCamera>();

            vacShapeCache.GetComponent<DynamicBone>().enabled = false; // I dont see why this is disabled. when enabled in the middle of gameplay it works, but im sure it should work.
            vacShapeCache.parent = scaler.transform;
            vacShapeCache.localRotation = Quaternion.Euler(new Vector3(-7118.613f, -8.3496f, -1.2340469f)); // Thanks Tranfox for this values
            vacShapeCache.localPosition = new Vector3(0.01f, -0.0071f, 0.0682f);
            vacShapeCache.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            vacconePrefab.localPosition = Vector3.zero;

            scaler.Find("arms").gameObject.SetActive(false);
            scaler.Find("mesh_l_armextra").gameObject.SetActive(false);

            // set up UI with VR layout

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

            //hudUITransform.gameObject.layer = LayerMask.NameToLayer("Weapon");
            Canvas canvas = hudUITransform.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            hudUITransform.localScale = new Vector3(0.000375f, 0.000375f, 0.000375f);

            GameObject uiCollider = new GameObject("UICollider")
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

            Transform slots = hudUIContainer.Find("Ammo Slots");
            for (int i = 0; i < slots.childCount; i++)
            {
                Transform slot = slots.GetChild(i);
                GameObject col = new GameObject("SlotCollider")
                {
                    transform =
                    {
                        localScale = Vector3.one * 0.03f,
                    }
                };

                col.AddComponent<BoxCollider>().isTrigger = true;
                col.SetActive(false);
                AmmoSlotTouchUI ammoSlotTouchUI = col.AddComponent<AmmoSlotTouchUI>();
                ammoSlotTouchUI.slotIDX = i;
                ammoSlotTouchUI.slotObject = slot;
                col.SetActive(true);
            }

            // death UI

            RectTransform death = hudUIContainer.parent.Find(nameof(DeathObscurer)) as RectTransform;
            death.gameObject.AddComponent<CanvasScaler>();
            death.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            death.localScale = Vector3.one * 99f;
            death.SetParent(null);
            death.gameObject.AddComponent<UIPositioner>();

            GameObject deathText = SceneContext.Instance.player.GetComponent<PlayerDeathHandler>().deathUIPrefab;
            deathText.transform.GetChild(0).localPosition += Vector3.back * 0.05f;
            deathText.transform.GetChild(1).localPosition += Vector3.back * 0.05f;

            VRDeathHandler.EventAdd();

            if (!EntryPoint.EnabledVR)
            {
                rightController.transform.position = simplePlayer.transform.position + new Vector3(0, 1f, 0);
                leftController.transform.position += new Vector3(0, 1.3f, 0);
            }

            HandManager.Instance.UI = hudUITransform.gameObject;
            HandManager.Instance.FPWeapon = scaler;
            HandManager.Instance.vacuumer = simplePlayer.GetComponentInChildren<WeaponVacuum>();
            HandManager.Instance.FPInteract = leftController; // will need to be properly modified with new interact system if such exists
            HandManager.Instance.dominantHand = VRConfig.SWITCH_HANDS ? UnityEngine.XR.XRNode.LeftHand : UnityEngine.XR.XRNode.RightHand;
            HandManager.Instance.UpdateHandStates();
        }
    }
    public class LaserPointer : MonoBehaviour
    {
        public Transform controller;
        public LineRenderer lineRenderer;

        public void Awake()
        {
            this.lineRenderer = this.gameObject.GetComponent<LineRenderer>();
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            this.controller = this.transform;
        }

        void Update()
        {
            // Set the laser start point at the controller
            lineRenderer.SetPosition(0, controller.position);
            
            Vector3 startPoint = controller.position;
            Vector3 endPoint = controller.position + controller.forward;

            if (Physics.CapsuleCast(startPoint, endPoint, 0.3f, controller.forward, out var hit, 10))
            {
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                lineRenderer.SetPosition(1, controller.position + controller.forward * 10f);
            }
        }
    }
}