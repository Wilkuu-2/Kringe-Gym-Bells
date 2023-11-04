using UnityEngine;
using Movement; 

namespace Powerup
{
    public enum PickupType { Protein, Creatine, Preworkout, Grip, Belt }
    public class Powerup : MonoBehaviour
    {
        public PickupEffectSettings effects; 
        public PickupType pickupType;
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
            var player = collision.gameObject; 
            if (player.CompareTag("Player"))
            {
                PlayerController controller = player.GetComponent<PlayerController>();
                var effect = effects.GetEffect(pickupType); 
                effects.TrySendMessage(pickupType, player);
                
                controller.movementVals.stack.AddEffect(effect);
                
                Destroy(gameObject);
            }
        }
    }
}
