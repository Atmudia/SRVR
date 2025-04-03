using UnityEngine;

namespace SRVR.Components
{
    public class PediaInteract : MonoBehaviour
    {
        internal static GameObject pediaModel;
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Hand")
            {
                PediaDirector.Id pediaId = PediaDirector.Id.BASICS;
                PediaPopupUI objectOfType = FindObjectOfType<PediaPopupUI>();
                
                if (objectOfType)
                    pediaId = objectOfType.GetId();
                
                SceneContext.Instance.PediaDirector.ShowPedia(pediaId);
            }
        }
    }
}