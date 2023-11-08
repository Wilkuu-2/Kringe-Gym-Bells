using UnityEngine;
using Inventory; 

public class LevelManager : MonoBehaviour
{
    public GameObject endArea;
    public GameObject startArea;
    public PlayerInventory inventory; 

    // Update is called once per frame
    void Update()
    {
        // Unlock the end area if no items left
        if (Object.FindObjectsOfType(typeof(GroundItem)).Length == 0 &&
                inventory.ItemAmount() <= 0 ){
            endArea.SetActive(true);
            startArea.SetActive(false);
        }
        
    }
}
