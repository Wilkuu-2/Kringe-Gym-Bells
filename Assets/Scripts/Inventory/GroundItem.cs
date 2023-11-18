using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inventory {
    public class GroundItem : MonoBehaviour
    {
        public InventoryItem item; 
        public bool disappearOnPickup = true; 
        public bool onlyOne = true; 
        public TimeLimitedAction pickupDelay;
        [HideInInspector] public bool pickedUp = false;

        private Rigidbody rb;
        private Vector3 startPos;

        void Start(){
            pickupDelay.Set();
            rb = GetComponent<Rigidbody>();
            startPos = transform.position;
        }

        void Collect(GameObject playerObject){


            if(playerObject.TryGetComponent<PlayerInventory>(
                        out PlayerInventory inventory)){

                if (pickupDelay.Peek())
                    return;
                var outcome = inventory.AddItem(item,onlyOne);
                
                if(outcome == InventoryOpOutcome.SUCCESS && disappearOnPickup){
                    Debug.Log("Picked up dropped object");
                    Destroy(gameObject); 
                }
                
                Debug.Log(outcome);

            }
        }

        void OnTriggerEnter(Collider other){
            Collect(other.gameObject);
        }

        void OnCollisionEnter(Collision collision){
            Collect(collision.gameObject);
        }
    }
}
