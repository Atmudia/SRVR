using UnityEngine;

namespace SRVR.Components
{
    public class HUDTouchBounds : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Hand")
            {
                other.transform.localScale = Vector3.one * 0.45f;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "Hand")
            {
                other.transform.localScale = Vector3.one;
            }
        }
    }
}