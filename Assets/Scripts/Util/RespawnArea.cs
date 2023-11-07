using UnityEngine;

public class RespawnArea : MonoBehaviour
{
    public Transform respawnAt;
    public LayerMask layers;

    void Start()
    {
        // Make sure the collider is a trigger
        var collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var fallen_object = other.gameObject;
        if (layers == (layers | (1 << fallen_object.layer)))
        {
            // Teleport
            fallen_object.transform.position = respawnAt.position;
            Debug.Log("Respawn");

            if (other.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                // Place the object with no velocity
                rb.velocity = Vector3.zero;
            }
        }
    }
}
