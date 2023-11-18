using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Falloff : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if(other.gameObject.TryGetComponent<RespawnManager>(out RespawnManager manager))
            {
                Debug.Log(other.gameObject.name);
                manager.doRespawn();
            }
        }
    }
}
