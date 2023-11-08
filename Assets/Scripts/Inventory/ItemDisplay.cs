using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    [RequireComponent(typeof(ItemDeposit))]
    public class ItemDisplay : MonoBehaviour
    {
        private int deposited = 0;
        private ItemDeposit deposit;
        private GameObject[] displayItems;
        public bool random = true;

        // Start is called before the first frame update
        void Start()
        {
            deposit = GetComponent<ItemDeposit>();
            displayItems = new GameObject[deposit.capacity];
            int index = 0;
            foreach (Transform child in transform)
            {
                if (child.CompareTag("DisplayItem"))
                {
                    displayItems[index] = child.gameObject;
                    index++; 
                    child.gameObject.SetActive(false);
                }
            }
            if (random){
                Shuffle(displayItems);
            }
        }

        // https://stackoverflow.com/questions/273313/randomize-a-list
        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Update is called once per frame
        void Update()
        {
            int deposit_amount = deposit.TotalAmount();
            if (deposit_amount != deposited)
            {
                deposited = deposit_amount;
                for (int i = 0; i < displayItems.Length; i++)
                {
                    displayItems[i].SetActive(i < deposited);
                }
            }
        }
    }
}
