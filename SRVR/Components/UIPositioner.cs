using System.Collections;
using System.Linq;
using UnityEngine;

namespace SRVR.Components
{
    public class UIPositioner : MonoBehaviour
    {
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        public void LateUpdate()
        {
            if (VRConfig.STATIC_UI_POSITION && !(DisableStaticPosition.Contains(gameObject.name) || IsInCategory(gameObject.name)))
                return;
            if (!_camera)
            {
                _camera = Camera.main;
                if (!_camera)
                    return;
            }
            transform.position = _camera.transform.position + _camera.transform.forward;
            transform.rotation = _camera.transform.rotation;
        }
        private IEnumerator DelayedPositionUpdate()
        {
            yield return new WaitForEndOfFrame();
            if (!_camera)
            {
                _camera = Camera.main;
                if (!_camera)
                    yield break;
            }
            transform.position = _camera.transform.position + _camera.transform.forward;
            transform.rotation = _camera.transform.rotation;
        }

        public void OnEnable()
        {
            StartCoroutine(DelayedPositionUpdate());
        }

        private static bool IsInCategory(string name)
        {
            foreach (var category in DisableStaticPositionForCategories)
            {
                if (name.Contains(category)) return true;
            }
            return false;
        }

        private static readonly string[] DisableStaticPosition = new[]
        {
            "GlitchTerminalActivatorUI_Ammo(Clone)",
            "ExchangeOfflineUI(Clone)",
            "DeathObscurer",
            "KnockedOutUI(Clone)"
        };

        private static readonly string[] DisableStaticPositionForCategories = new[]
        {
            "Activate",
            "Popup",
        };
    }
}