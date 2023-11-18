using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inventory {
    public class GroundItem : MonoBehaviour
    {
        public InventoryItem item; 
        public bool disappearOnPickup = true; 
        public bool onlyOne = true; 
        public TimeLimitedAction pickupDelay;
        [HideInInspector] public bool pickedUp = false;

        private Rigidbody rb;
        private Vector3 startPos;
        private float angle;

        private float amplitude = 0.5f;
        private float frequency = 0.01f;
        private float rotationalSpeed = 0.8f;
        private Vector3 rotationAngle = new Vector3(0, 0, 1);
        private bool isFlying = true;
        private bool waiting = false;


        void Start(){
            pickupDelay.Set();
            rb = GetComponent<Rigidbody>();
            startPos = transform.position;
            angle = 0f;
        }

        void Collect(GameObject playerObject){


            if(playerObject.TryGetComponent<PlayerInventory>(
                        out PlayerInventory inventory)){

                if (pickupDelay.Peek())
                    return;
                var outcome = inventory.AddItem(item,onlyOne);
                
                if(outcome == InventoryOpOutcome.SUCCESS && disappearOnPickup){
                    Debug.Log("Picked up dropped object");
                    Destroy(gameObject); 
                }
                
                Debug.Log(outcome);

            }
            else
            {
                if (playerObject.CompareTag("Respawn"))
                {
                    if (!isFlying)
                    {
                        if (!waiting)
                        {

                            StartCoroutine(waitToReset());
                        }
                    }
                }
            }
        }

        IEnumerator waitToReset()
        {
            waiting = true;
            yield return new WaitForSeconds(2);
            resetSelf();
            waiting = false;
        }

        public void resetSelf()
        {
            isFlying = true;
            startPos = transform.position + new Vector3(0,3,0);
        }

        public void setFlying(bool flying)
        {
            isFlying = flying;
        }

        private void LateUpdate()
        {
            if (isFlying)
            {
                angle += frequency;
                transform.position = new Vector3(startPos.x, startPos.y + amplitude * Mathf.Sin(angle), startPos.z);
                rb.angularVelocity = rotationAngle * rotationalSpeed;
                if (angle > 360)
                {
                    angle = 0f;
                }

            }
        }

        void OnTriggerEnter(Collider other){
            Collect(other.gameObject);
        }

        void OnCollisionEnter(Collision collision){
            Collect(collision.gameObject);
        }
    }
}
