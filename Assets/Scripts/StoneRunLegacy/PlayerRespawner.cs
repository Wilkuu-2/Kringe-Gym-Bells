using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawner : MonoBehaviour
{
    public Transform respawnFlag;
    public PlayerControllerRigibody player;

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == player.gameObject){
            player.transform.position = respawnFlag.position;
            other.attachedRigidbody.velocity = Vector3.zero; 
        }
    }
}
