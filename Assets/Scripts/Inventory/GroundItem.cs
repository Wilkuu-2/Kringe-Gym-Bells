using UnityEngine;

namespace Inventory {
    public class GroundItem : MonoBehaviour
    {
        public InventoryItem item; 
        public bool disappearOnPickup = true; 
        public bool onlyOne = true; 
        public TimeLimitedAction pickupDelay;
        
        void Start(){
            pickupDelay.Set(); 
        }

        void Collect(GameObject playerObject){
            if(pickupDelay.Peek())
                return;

            if(playerObject.TryGetComponent<PlayerInventory>(
                        out PlayerInventory inventory)){

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
