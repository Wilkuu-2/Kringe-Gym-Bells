using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

namespace Inventory
{
    public enum InventoryOpOutcome
    {
        SUCCESS, 
        FULL,
        ALREADY_PRESENT,
    }
    [RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
    public class PlayerInventory : MonoBehaviour
    {

        [SerializeField][ReadOnly] List<InventoryItem> items;
        [SerializeField][ReadOnly] Movement.MovementEffectValues inventoryEffects; 
        public float throwImpulse = 1000f; 
        private bool effectsInvalidated = true;
        private Rigidbody rb; 

        public int capacity = 5;
        
        public void Start(){
            rb = GetComponent<Rigidbody>(); 
        }

        public InventoryOpOutcome AddItem(InventoryItem item, bool unique = false)
        {
            if(items.Find(i => i == item) && unique){
                return InventoryOpOutcome.ALREADY_PRESENT; 
            } 

            if(items.Count >= capacity){
                return InventoryOpOutcome.FULL;
            }
            
            items.Add(item); 
            effectsInvalidated = true; 
            return InventoryOpOutcome.SUCCESS; 
        }

        #nullable enable
        public InventoryItem? PopItem()
        {
            try 
            {
                InventoryItem item = items.Last();
                items.Remove(item);
                effectsInvalidated = true; 
                return item; 
            }
            catch (System.InvalidOperationException) { return null; }
        }
        #nullable restore 

        public bool HasItem(InventoryItem item){
            return items.Contains(item); 
        }

        public int RemoveSpecificItem(InventoryItem item, int amount = 1){
            int removedAmt = 0; 
            int foundAmt = items.RemoveAll(i => {
                // Remove the item only if we still haven't removed the item and the item matches
                if(removedAmt < amount && i == item ){                  
                   removedAmt++;
                   return true; 
                }
                return false; 
            });
            Debug.Assert(removedAmt == foundAmt); 
            // Invalidate if removed anything
            effectsInvalidated |= removedAmt > 0; 
            return removedAmt; 

        }

        public bool getEffects(out Movement.MovementEffectValues movementEffects){
            bool _invalidated = effectsInvalidated; 

            if (effectsInvalidated){
                inventoryEffects = Movement.MovementEffectValues.newEmpty("Inventory");
                inventoryEffects.duration = 99999999999999999f;
                foreach(InventoryItem item in items){
                    inventoryEffects.SumInPlace(item.movementEffects);
                }
                effectsInvalidated = false; 
            }

            movementEffects = inventoryEffects; 
            return _invalidated;

        }  

        void OnItemDrop(){
            InventoryItem? item = PopItem();
            if(item == null) 
                return;
            

            GroundItem? dropped = item.SpawnGroundItem(transform);
            
            if (dropped is not null){
                if(dropped.TryGetComponent<Rigidbody>(out Rigidbody itemrb)){
                    itemrb.velocity = rb.velocity;
                    itemrb.AddForce(transform.forward * throwImpulse, ForceMode.Impulse);
                }
            }

            
        } 

    }
}
