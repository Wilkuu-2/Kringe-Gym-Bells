using UnityEngine;

namespace SRLegacy { 
    abstract public class ExternalColliderReciever : MonoBehaviour
    {
        abstract public void OnETriggerEnter(ExternalCollider collider, Collider other);
        abstract public void OnETriggerExit(ExternalCollider collider, Collider other); 
        abstract public void OnETriggerStay(ExternalCollider collider, Collider other); 
    }
}
