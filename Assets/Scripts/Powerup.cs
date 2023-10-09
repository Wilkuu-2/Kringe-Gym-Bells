using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
namespace Powerup
{
    public enum PickupType { Protein, Creatine, Preworkout, Grip, Belt }
    public class Powerup : MonoBehaviour
    {
        public PickupType pickuptype;
        private Rigidbody rb;
        private Vector3 startPos;
        private float angle;

        private float amplitude = 0.5f;
        private float frequency = 0.01f;
        private float rotationalSpeed = 0.8f;
        private Vector3 rotationAngle = new Vector3(0, 0, 1);

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            startPos = transform.position;
            angle = 0f;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            angle += frequency;
            transform.position = new Vector3(startPos.x, startPos.y + amplitude * Mathf.Sin(angle), startPos.z);
            rb.angularVelocity = rotationAngle * rotationalSpeed;
            if(angle > 360)
            {
                angle = 0f;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Destroy(gameObject);
            if (collision.gameObject.CompareTag("Player"))
            {
                switch (pickuptype)
                {
                    case PickupType.Protein:
                        // Increment collection score by 1
                        break;
                    case PickupType.Creatine:
                        // Increase how much you can carry before impaired for x seconds
                        break;
                    case PickupType.Preworkout:
                        // Increase movement speed by x for y seconds
                        break;
                    default:
                        break;
                }
            }
        }
    }
}