using UnityEngine;

namespace SRLegacy { 
    public class RayCastPlus : MonoBehaviour
    {
        // Private state 
        private int nHit; 
        private bool rayDidHit;
        private Collider col;
        private RaycastHit hit;
        private bool willSphereCast;

        // Public input 
        public Vector3 direction;
        public float maxDist;  
        public LayerMask mask; 
        public Transform origin;
        [Tooltip("If 0, will raycast instead")]
        public float sphereCastRadius = 0;
        public bool useCollider = true;

        void Start()
        {
            willSphereCast = sphereCastRadius != 0;
            if(useCollider){
                col = GetComponent<Collider>();
                Debug.Assert(col);
            }
            

        }
        
        void FixedUpdate(){
            if(nHit > 0 || !useCollider){
                RaycastHit h;
                Ray r = new Ray(origin.position, origin.TransformDirection(direction));
                if(willSphereCast)
                    rayDidHit = Physics.SphereCast(r,sphereCastRadius,out h ,maxDist,mask,QueryTriggerInteraction.Ignore);
                else
                    rayDidHit = Physics.Raycast(r,out h ,maxDist,mask,QueryTriggerInteraction.Ignore);
                
                hit = h;
            }
        }

        void OnTriggerEnter(Collider other){
            if(mask == (mask | (1 << other.gameObject.layer))){    
                nHit++;
            } 
        }
        
        void OnTriggerExit(Collider other){
            if(mask == (mask | (1 << other.gameObject.layer))){    
                nHit--;
            }
        }
         
        public RaycastHit getRayHit(){
            // Debug.Assert(isHit(),"The getRayHit method is used without isHit being true");
            return hit; 
        }
        public bool isHit(){
            return (nHit > 0 || !useCollider) && rayDidHit; 
        } 
        public Vector3 getNormal(){
            // Debug.Assert(isHit(),"The getNormal method is used without isHit being true");
            return hit.normal;
        }
        public Collider getCollider(){
            // Debug.Assert(isHit(),"The getCollider method is used without isHit being true");
            return hit.collider;
        }
        public GameObject getObject(){
            // Debug.Assert(isHit(),"The getObject method is used without isHit being true");
            return hit.collider.gameObject;
        }
        public float getDistance(){
            // Debug.Assert(isHit(),"The getDistance method is used without isHit being true");
            return hit.distance;
        }
        public Vector3 getPoint(){
            // Debug.Assert(isHit(),"The getPoint method is used without isHit being true");
            return hit.point;
            
        }

        // Converting to float distance or boolean is hit
        public static implicit operator bool(RayCastPlus r) => r.isHit();
        public static implicit operator float(RayCastPlus r) => r.getDistance();


    }
}
