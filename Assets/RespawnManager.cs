using Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{

    private Transform respawnPoint;
    private Vector3 offset = new Vector3(0, 3, 0);

    void Start()
    {
    }

    public Transform getRespawnPoint()
    {
        return respawnPoint;
    }

    public void setRespawnPoint(Transform point)
    {
        respawnPoint = point;
    }

    void Update()
    {
        
    }

    public void doRespawn()
    {
        if(gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
        }
        if(respawnPoint != null)
        {
            transform.position = respawnPoint.position + offset;
        }
        else
        {
            transform.position = new Vector3 (142, 0, 49) + offset;
        }
        if(gameObject.GetComponent<GroundItem>())
        {
            gameObject.GetComponent<GroundItem>().resetSelf();
        }
    }
}
