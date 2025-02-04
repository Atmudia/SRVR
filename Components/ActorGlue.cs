using UnityEngine;

namespace SRVR.Components
{
    public class ActorGlue : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == vp_Layer.Actor&& other.GetComponent<Identifiable>())
            {
                other.transform.parent = transform;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == vp_Layer.Actor&& other.GetComponent<Identifiable>())
            {
                other.transform.parent = null;
            }
        }
    }
}