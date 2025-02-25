using System.Linq;
using UnityEngine;

namespace SRVR.Components
{
    public class UIPositioner : MonoBehaviour
    {
        public void LateUpdate()
        {
            if (VRConfig.STATIC_UI_POSITION && !(DisableStaticPosition.Contains(gameObject.name) || IsInCategory(gameObject.name)))
                return;
            if (Camera.main == null) return;
            transform.position = Camera.main.transform.position + Camera.main.transform.forward;
            transform.rotation = Camera.main.transform.rotation;
            
            // if (VRConfig.STATIC_UI_POSITION && !(DisableStaticPosition.Contains(gameObject.name) || IsInCategory(gameObject.name)))
        }

        public void OnEnable()
        {
            if (Camera.main == null) return;
            transform.position = Camera.main.transform.position + Camera.main.transform.forward;
            transform.rotation = Camera.main.transform.rotation;
        }

        public static bool IsInCategory(string name)
        {
            foreach (var category in DisableStaticPositionForCategories)
            {
                if (name.Contains(category)) return true;
            }
            return false;
        }
        
        public static readonly string[] DisableStaticPosition = new[]
        {
            "GlitchTerminalActivatorUI_Ammo(Clone)",
            "ExchangeOfflineUI(Clone)",
            "DeathObscurer",
            "KnockedOutUI(Clone)"
        };

        public static readonly string[] DisableStaticPositionForCategories = new[]
        {
            "Activate",
            "Popup",
        };
    }
}