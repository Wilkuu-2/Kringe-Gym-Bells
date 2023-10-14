using UnityEngine;
using UnityEngine.InputSystem;

namespace Movement {
    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(PlayerInput))]

    public class PlayerController: MonoBehaviour
    {
        // Settings
        public LayerMask groundMask; 
        public LayerMask ceilingMask;

        public SRLegacy.ObstacleDetector detection;

        [Header("Camera")]
        public Transform viewCamera;

        [Header("Floating")] 
        [Tooltip("How high the collider hover above the ground ")] 
        public float floatHeight   = 0.4f; 
        public float floatSpringLength = 2; 
        [Tooltip("Spring force coefficient for hovering")]
        public float floatSpring   = 50000f; 
        [Tooltip("Damping force coefficient for hovering ")]
        public float floatDamping  = 1000f;  
        [Tooltip("Max speed fed into the damping force")]
        public float floatDampingClamp = 3f;
        public float maxVerticalVelocityOnGround = 3f;

        private float springNeutralPoint {get => col.bounds.extents.y + floatHeight;}
        

        [Range(60,3600)]
        [Tooltip("The speed at which the character rotates to the movement")]
        public float rotationSpeed = 720; 


        [Header("Settings")]
        // public SRLegacy.MovementClass movement; 
        public WallClass o_wall;
        public GroundClass     o_walk;
        public AerialClass      o_air;
        

        public MovementState state;

        // Objects
        private Rigidbody rb; 
        private Collider col; 
        private Animator hb_anim; 
        private Vector2 in_walk;

        public void Start() 
        {
            // Get all the components
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            hb_anim = GetComponent<Animator>(); 

            Cursor.lockState = CursorLockMode.Locked; 
        } 
        public void FixedUpdate() {
            rb.angularVelocity = new Vector3(rb.angularVelocity.x, 0 ,rb.angularVelocity.z); 

            if(detection[SRLegacy.DIR.DOWN]){
                state.isGrounded.Set(detection[SRLegacy.DIR.DOWN].getPoint().y > (transform.position.y - springNeutralPoint));  
            } else {
                state.isGrounded.Set(false);
                
            }
 
            state.nearCeiling.Set((bool) detection[SRLegacy.DIR.UP]); 
         
            //ceilCheck();
            //wallCheck();
            determineMode();
            
            determineAccelerationAndDesiredSpeed(out float acc, out float desiredSpeed);
            applyFloatation();
            applyLateralMovementForce(acc, desiredSpeed);

            applyCharacterRotation();

            if(state.isGrounded && o_air.jumpBuffering.Peek()) {
                o_air.jumpBuffering.Consume(); 
                Jump(false);
                
            }
        }


        public bool CanSlide() {
            // TODO implement speed check
            return rb.velocity.magnitude > o_walk.slideMinVel;
        }

        void determineMode() {
#if false 
            Debug.Log(string.Format("\ngrounded: {0} | mode: {1} | velocity {2}",
                       state.isGrounded.current,
                       state.mode.current,
                       rb.velocity.y));
#endif
           if (state.isGrounded) {
                if (state.mode == MovementMode.SLIDE)
                    if (CanSlide())
                        state.mode.Set(MovementMode.SLIDE);
                    else {
                        state.mode.Set(MovementMode.WALK);
                        hb_anim.SetBool("isSliding",false);
                    }
                else if (state.mode == MovementMode.JUMP && rb.velocity.y > -0.1)
                    state.mode.Set(MovementMode.JUMP);
                else if (state.mode != MovementMode.WALK)
                {
                    state.mode.Set(MovementMode.WALK);
                    state.curAirActions = 0;
                }
           } else {
                if (!((state.mode == MovementMode.JUMP && rb.velocity.y > -0.1) ||
                     state.mode == MovementMode.SLIDE ||
                     state.mode == MovementMode.WALL))
                    state.mode.Set(MovementMode.AIR);

                if (state.mode.previous == MovementMode.WALK && 
                    state.mode.current == MovementMode.AIR) 
                    o_air.coyoteTime.Set();
           }
        }

        void applyFloatation(){
            if(detection[SRLegacy.DIR.DOWN])
            {
                var targetY = detection[SRLegacy.DIR.DOWN].getPoint().y + springNeutralPoint; 
                
                var diff    = targetY - transform.position.y;
                
                if (diff < -floatSpringLength || 
                    diff > floatSpringLength)
                    diff = 0; 
            

                if (state.mode == MovementMode.WALK){ 
                    
                    if (diff > -0.2f && diff < 0.5f && Mathf.Abs(rb.velocity.y) < 1.0f) {
                        transform.position.Set(transform.position.x, targetY, transform.position.z);  
                        return;
                    }

                    var dampingForce = rb.velocity.y * -floatDamping;
                    var springForce = floatSpring * diff; 

                    rb.AddForce(Vector3.up * (springForce + dampingForce) * Time.deltaTime);

                }
            
            }
#if false
                Debug.Log(string.Format("diff: {0}, speed: {3}, spring: {1}, damping: {2}",
                            diff,
                            springForce, 
                            dampingForce,
                            rb.velocity.y));
#endif
             
        }

        void determineAccelerationAndDesiredSpeed(out float acc, out float desiredSpeed){
                switch (state.mode.current) {
                case MovementMode.WALK:
                    acc = state.isSprinting ? o_walk.runAcc : o_walk.walkAcc;
                    desiredSpeed = state.isSprinting ? o_walk.runSpeedMax : o_walk.walkSpeedMax; 
                    break;
                case MovementMode.SLIDE: 
                        if(state.isGrounded) {
                            acc = 2 * o_walk.runAcc;
                             // Make sure the player is not stuck in ceiling
                            if(state.nearCeiling)
                            {
                                desiredSpeed = o_walk.walkSpeedMax; // Keep moving when under a obstacle
                            }
                            else 
                            { 
                                
                                // desiredSpeed = new Vector2(rb.velocity.x, rb.velocity.z).magnitude + Mathf.Clamp(-rb.velocity.y,0, float.MaxValue);
                                desiredSpeed = new Vector3(rb.velocity.x, 
                                                            Mathf.Clamp(rb.velocity.y, float.MinValue, 0),
                                                            rb.velocity.z).magnitude;

                                if (desiredSpeed > o_walk.walkSpeedMax){
                                    desiredSpeed -= (o_walk.slideDrag * rb.velocity.magnitude * Time.deltaTime); // Incur drag
                                } else if (desiredSpeed < o_walk.slideMinVel+2) {
                                    desiredSpeed = 0; 
                                }

                                    
#if true
                                Debug.Log(string.Format("\ncurrent: {0} | desired: {1} | can slide {2} | vspeed {3}",
                                           rb.velocity.magnitude,
                                           desiredSpeed,
                                           CanSlide(),
                                           rb.velocity.y));
#endif
                            }
                        }
                        else 
                        {
                            goto case MovementMode.AIR;
                        }
                break;

                case MovementMode.JUMP:
                case MovementMode.AIR: 
                    // Jumping movement

                    acc = o_air.inAirAcceleration;
                    desiredSpeed = Mathf.Max(o_air.inAirMaxAddedSpeed, 
                                             new Vector3(rb.velocity.x,0,rb.velocity.z).magnitude);                                                                                                                                                             
                break;
                default:  
                        acc = 0; 
                        desiredSpeed = 0; 
                break;
            }


        }

        void applyLateralMovementForce(float acc, float desiredSpeed){
                var x_movement = state.mode == MovementMode.SLIDE ? 0 : in_walk.x; 
                var z_movement = 0.0f;
                var _desiredSpeed = desiredSpeed; 

                if (state.isGrounded || o_air.allowForward) {
                    z_movement = state.mode == MovementMode.SLIDE ? 1 : in_walk.y;
                } 
                // Calculate velocity vector with acquired speed 
                var desiredVelocity = new Vector3(x_movement,0,z_movement)
                      * desiredSpeed;

                if (detection[SRLegacy.DIR.DOWN]){
                    // Project on any plane we could be 
                    var normal = detection[SRLegacy.DIR.DOWN].getNormal();
                    desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, normal);
                }
                
                desiredVelocity += Vector3.up * rb.velocity.y;

                
                // Adjust to camera rotation
                desiredVelocity = Quaternion.AngleAxis(viewCamera.eulerAngles.y,Vector3.up) * desiredVelocity;
                
                // Get the actual acceleration vector
                var acceleration = (desiredVelocity - rb.velocity).normalized * acc;

                // Apply force
                rb.AddForce(acceleration * Time.deltaTime ,ForceMode.Acceleration);

                
        }

        void applyCharacterRotation(){
            if (state.mode == MovementMode.SLIDE || in_walk.magnitude > 0){
                // Rotate character
                float angle = transform.eulerAngles.y; 
                
                /*
                float desiredAngle = viewCamera.eulerAngles.y + 
                    (state.mode == MovementMode.SLIDE ? 0 : (Mathf.Atan2(in_walk.x,in_walk.y) * Mathf.Rad2Deg));
                */

                float desiredAngle = viewCamera.eulerAngles.y;
                float turnSpeed = rotationSpeed * Time.deltaTime;
                
                float delta = Mathf.DeltaAngle(angle, desiredAngle);
                
                float turning = delta / 180 * turnSpeed;
                transform.Rotate(0,turning,0);
            }

        }

        void OnMove(InputValue val){
            in_walk = val.Get<Vector2>();
        }

        void OnSprintStart(){
            state.isSprinting.Set(true);
        }
        
        void OnSprintEnd(){
            state.isSprinting.Set(false);
        } 
        
        void OnSlideStart(){
            if(CanSlide()){
                state.mode.Set(MovementMode.SLIDE);
                hb_anim.SetBool("isSliding" ,true);
            }
        }
        
        void OnSlideEnd(){
            Debug.Log("Input: Stop Sliding");
            Debug.Log(state.nearCeiling.current);
            if(!state.nearCeiling.current){
                state.mode.Set(MovementMode.WALK);
                determineMode();
                state.mode.Set(MovementMode.WALK);
                Debug.Log("Action: Stop Sliding");
                 hb_anim.SetBool("isSliding" ,false);
            }
        }

        void Jump(bool isInAir){
                // Cancel out any vertical speed if it is negative 
                if(rb.velocity.y < 0)
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                // Add jumpSpeed as velocity when in Air
                rb.AddForce(transform.up * o_air.velocity * rb.mass,ForceMode.Impulse);
                // Increment action counter
                if (isInAir)
                    state.curAirActions += 1;

                state.mode.Set(MovementMode.JUMP);
                // hb_anim.SetTrigger("a_tJump");
        } 
        void OnJumpStart(){
            state.pressingJump.Set(true); 
            // TODO Wall logic
            if(state.isGrounded || o_air.coyoteTime.Consume()){
                Jump(false);
            }
            else if(state.curAirActions < o_air.maxInAirActions){
                Jump(true);
            }
            else{
                o_air.jumpBuffering.Set();
            }
        }

        void OnJumpEnd(){
            state.pressingJump.Set(false);
            o_air.jumpBuffering.Consume(); 
        }

        
    }

    [System.Serializable]
    public class MovementState {
        public ShiftValue<MovementMode> mode = new ShiftValue<MovementMode>(MovementMode.WALK); 
        public ShiftValue<bool> isSprinting = new ShiftValue<bool>(false); 
        public ShiftValue<bool> isGrounded  = new ShiftValue<bool>(false);
        public ShiftValue<bool> nearCeiling = new ShiftValue<bool>(false);
        public ShiftValue<bool> pressingJump = new ShiftValue<bool>(false); 
        public int  curAirActions = 0;

    }
    
    
    public enum MovementMode {
        WALK,
        AIR,
        JUMP,
        SLIDE,
        WALL, 
    }


    [System.Serializable]
    public class GroundClass 
    {
        [Header("GroundMovement")]
        [Tooltip("Acceleration for walking ")]
        public float walkAcc = 900;
        [Tooltip("Acceleration for running")]
        public float runAcc  = 1200; 
        [Tooltip("Max walking speed ")]
        public float walkSpeedMax = 6; 
        [Tooltip("Max sprinting speed ")]
        public float runSpeedMax = 12; 

        [Header("Slide")]
        public float slideDrag = 0.1f;
        public float slideMinVel = 0.5f; 

    }

    [System.Serializable]
    public class AerialClass {
        [Header("General")]
        [Tooltip("How much double jumps or dashes can the charater do before having to land again ")]
        public int   maxInAirActions = 1;  

        [Tooltip("Upwards velocity which gets added to the current velocity when jumping ")]
        public float velocity  = 9;  
        [Tooltip("Forwards velocity which gets added to the current velocity when dashing ")]
        public float dashVelocity  = 12; 

        [Header("Strafing")]
        [Tooltip("Acceleration that is applied in-air")]
        public float inAirAcceleration = 50;
        [Tooltip("Maximal speed that is added to in-air movements")]
        [Range(5.0f,float.MaxValue)]
        public float inAirMaxAddedSpeed = 5;
        [Tooltip("Allow adding forward (or backward) movement in-air.\nDisable for Source-like strafing")]
        public bool  allowForward = false; 

        #region  coyoteTime 
        public TimeLimitedAction coyoteTime;
        #endregion

        #region buffering
        public TimeLimitedAction jumpBuffering;
        #endregion 

    }

    [System.Serializable]
    public class WallClass
    {
        [Tooltip("Force which is applied while wallrunning")]
        public float runForce = 10000; 
        [Tooltip("Maximal speed while wallrunning ")]
        public float maxSpeed = 10;    
        [Tooltip("The vertical friction applied while wallrunning ")]
        public float friction = 100f;  
        [Tooltip("The jump velocity while bouncing off the wall")]
        public float jumpVelocity = 12;  
    }
}
