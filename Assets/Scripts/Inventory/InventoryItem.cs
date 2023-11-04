using UnityEngine;

namespace Inventory {     
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory Item", order = 1)]
    public class InventoryItem : ScriptableObject
    {
        public string Name; 
        public Movement.MovementEffectValues movementEffects;
        public GroundItem itemPrefab; 
        public bool droppable = true; 

        public void Awake(){
            Validate();        
        } 
        
        // Validates that the item prefab has this item as it's assigned item
        public void Validate(){
            if(droppable){
                Debug.Assert(itemPrefab != null, "The item prefab does not have the correct component(GroundItem) on it");
                Debug.Assert(itemPrefab.item == this, "The item prefab has a mismatching item selected, select this item in the GroundItemComponent of your prefab" );
            }
        }

        public bool SpawnGroundItem(Transform drop_transform, Rigidbody rb){
            if(!droppable)
                return false;

            GroundItem item = Instantiate(itemPrefab, drop_transform.position, drop_transform.rotation);
            item.pickupDelay.Set();
            
            if(item.TryGetComponent<Rigidbody>(out Rigidbody itemrb)){
                itemrb.velocity = rb.velocity;
            }

            return true; 
        }
    }


}
