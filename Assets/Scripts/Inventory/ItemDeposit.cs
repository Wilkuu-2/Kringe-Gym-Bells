using UnityEngine;
using System.Collections.Generic;

namespace Inventory
{
    [System.Serializable]
    public struct ItemAndCount
    {
        public InventoryItem item;
        public int count;
        public int limit;

        // This is legal in foreach loops, += isn't
        public void addToCount(int addition)
        {
            count += addition;
        }
    }

    [RequireComponent(typeof(Collider))]
    public class ItemDeposit : MonoBehaviour
    {

        public ItemAndCount[] acceptedItems;
        public int uptakeAmount = 1;
        public int capacity = 4;
        public FMODUnity.EventReference depositSound; 

        private void TryDeposit(GameObject target)
        {
            int currentTotal = TotalAmount(); 

            if (target.TryGetComponent<PlayerInventory>(out var inventory))
            {
                Debug.Log("Has Inventory");
                int uptakeLeft = System.Math.Min(uptakeAmount, capacity - currentTotal);
                int uptakeLeftStart = uptakeLeft;

                // Loop over all items and remove them if they are in the inventory
                for (int i = 0; i < acceptedItems.Length; i++)
                {
                    var to_take = uptakeLeft;
                    if (acceptedItems[i].limit > 0)
                    {
                        to_take = System.Math.Min(acceptedItems[i].limit - acceptedItems[i].count, to_take);
                    }

                    var items_taken = inventory.RemoveSpecificItem(acceptedItems[i].item, to_take);
                    uptakeLeft -= items_taken;
                    acceptedItems[i].count += items_taken;

                    if (uptakeLeft == 0)
                        break;
                }
                if (uptakeLeftStart > uptakeLeft){
                    FMODUnity.RuntimeManager.PlayOneShot(depositSound,transform.position);
                }
            }
            else if (target.TryGetComponent<GroundItem>(out var gItem))
            {
                Debug.Log("Has GroundItem");

                if(TotalAmount() >= capacity)
                    return; // Full 

                if(gItem.pickedUp)
                    return; //Already picked up

                for(int i = 0; i < acceptedItems.Length; i++) 
                {
                    if(acceptedItems[i].item == gItem.item){
                        // Check if over limit 
                        if (acceptedItems[i].limit > 0 
                                && acceptedItems[i].count >= acceptedItems[i].limit)
                            return;

                        // Increment count 
                        gItem.pickedUp = true; 
                        Destroy(gItem.gameObject);
                        acceptedItems[i].count++;
                        FMODUnity.RuntimeManager.PlayOneShot(depositSound,transform.position);
                        return;
                    }
                }

            }
        }
        //public void OnTriggerEnter(Collider other)
        //{
        //    TryDeposit(other.gameObject);
        //}

        public void OnCollisionEnter(Collision collision)
        {
            TryDeposit(collision.gameObject);
        }

        public int TotalAmount(){
            int total = 0;
            foreach (var item in acceptedItems){
                total += item.count; 
            }
            return total;
        }
    
    }

}
