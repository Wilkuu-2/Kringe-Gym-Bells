using UnityEngine;


namespace SRLegacy {
    [RequireComponent(typeof(Collider))]
    public class ExternalCollider : MonoBehaviour
    {
        [SerializeField]
        private ExternalColliderReciever reciever;  
        public Collider col {get; private set;} 
        
        // Get the collider
        private void Start() {
            col = GetComponent<Collider>();
        }

        // Pass the events 
        void OnTriggerEnter(Collider other) {
           reciever.OnETriggerEnter(this,other); 
        }
        
        void OnTriggerEnter(Collision collision){
            Debug.Log("Collision: " + collision);
        }

        void OnTriggerExit(Collider other) {
           reciever.OnETriggerExit(this,other); 
        }
        
        void OnTriggerStay(Collider other) {
            reciever.OnETriggerStay(this, other);   
        }
    }
}
