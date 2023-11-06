using UnityEngine;
using UnityEngine.InputSystem;

namespace Movement {
    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(PlayerInput))]
    public class PlayerController: MonoBehaviour
    {
        public SRLegacy.ObstacleDetector detection;
        [Tooltip("Don't touch")]
        public float springMultiplier = 1.0f;

        [Header("Camera")]
        public Transform viewCamera;

        private float springNeutralPoint {get => col.bounds.extents.y + (o_spring.floatHeight * springMultiplier);}

        [Range(60,10800)]
        [Tooltip("The speed at which the character rotates to the movement")]
        public float rotationSpeed = 6400f; 


        [Header("Settings")]
        // public SRLegacy.MovementClass movement; 
        public MovementValues movementVals; 
        private SpringOptions o_spring {get => movementVals.appliedSpring;}
        private GroundClass     o_walk {get => movementVals.appliedGround;}
        private AerialClass      o_air {get => movementVals.appliedAir;}
        

        public MovementState state;

        // Objects
        private Rigidbody rb; 
        private Collider col; 
        private Animator hb_anim; 
        private Vector2 in_walk;
        private Inventory.PlayerInventory inventory; 
        private PlayerAnimationManager anim; 

        public void MessageTest(Powerup.PickupType type){
            Debug.Log("Player says:");
            Debug.Log(type);
        }

        public void MessageTest2(){
            Debug.Log("Player says:");
            Debug.Log("DeezNuts");
        }

        public void Start() 
        {
            // Get all the components
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            anim = GetComponentInChildren<PlayerAnimationManager>();
            
            hb_anim = GetComponent<Animator>(); 
            inventory = GetComponent<Inventory.PlayerInventory>();  
            movementVals.Init();

            // TODO: Move somewhere else
            Cursor.lockState = CursorLockMode.Locked; 
        } 

        public void Update(){
            hb_anim.SetBool("isSliding", state.mode == MovementMode.SLIDE);
        }

        public void FixedUpdate() {
            movementVals.Refresh(inventory);
            
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
            
            applyFloatation();
            lateralMovement();

            applyCharacterRotation();
            // Bhopping 
            if (state.mode.previous == MovementMode.AIR  
                && state.mode.current == MovementMode.WALK 
                && state.pressingJump
                && o_air.auto_bhop){
                movementVals.jumpBuffering.Set();
            }

            if(state.isGrounded.current && state.isGrounded.previous && movementVals.jumpBuffering.Peek()) {
                movementVals.jumpBuffering.Consume(); 
                Jump(false);
                
            }
        
        }


        public bool CanSlide() {
            // Only slide if in-air or if the speed is adequate
            return (rb.velocity.magnitude > o_walk.slideMinVel || 
                    !state.isGrounded) && 
                movementVals.CanSlide(); // Block sliding if we have the mop
        }
        
        // This function is the entire state machine of the different kinds of movement, excluding input. 
        void determineMode() {
           // Transitions when touching ground 
           // EXCLUDES: 
           //   Jump key: ANY -> JUMP 
           //   Slide key: ANY -> SLIDE 
           Debug.Log(state.mode.current);
           if (state.isGrounded) {
               // Transition out of jumping
                if (state.mode == MovementMode.SLIDE && !CanSlide()){
                        Debug.Log("Switching to walking bc CanSlide failed");
                        state.mode.Set(MovementMode.WALK);
                }
                // Transition 
                //else if (state.mode == MovementMode.JUMP && rb.velocity.y > -0.1)
                //    Untitledstate.mode.Set(MovementMode.JUMP);
                
                // Transition from NOT SLIDE OR WALK to WALK 
                else if (state.mode != MovementMode.WALK && 
                         state.mode != MovementMode.SLIDE)
                {
                    state.mode.Set(MovementMode.WALK);
                    state.curAirActions = 0; // Reset jumping 
                }
           } else {
                // When falling and not sliding go to AIR 
                if (!((state.mode == MovementMode.JUMP && rb.velocity.y > -0.1) ||
                     state.mode == MovementMode.SLIDE ||
                     state.mode == MovementMode.WALL || 
                     state.mode == MovementMode.AIR))
                    state.mode.Set(MovementMode.AIR);
            
                // Set coyote time if fallen off a ledge while walking
                // TODO: This should be moved somewhere where it fits more
                // TODO: This introduced a bug where you don't have coyote time 
                //        when sliding off a ledge
                if (state.mode.previous == MovementMode.WALK && 
                    state.mode.current == MovementMode.AIR) 
                    movementVals.coyoteTime.Set();
           }
        }
        
        // This method is the magic that keeps the hitbox above the grounds so one can use stairs
        void applyFloatation(){
            // Check if we have ground below us 
            // TODOD: isGrounded can be used for consistency
            if(detection[SRLegacy.DIR.DOWN])
            {
                // Get the height at which we want to float 
                var targetY = detection[SRLegacy.DIR.DOWN].getPoint().y + springNeutralPoint; 
                
                // Subtract where the missle is from the missle isn't
                var diff    = targetY - transform.position.y;
                
                // If the spring is stretched too long, lose contact
                if (diff < -o_spring.floatSpringLength)
                    diff = 0; 
            
                // Only use the spring when walking
                if (state.mode == MovementMode.WALK){ 
                    
                    // If we are close to being neutral, snap to the target height
                    if (false && diff > -0.2f && diff < 0.5f && Mathf.Abs(rb.velocity.y) < 1.0f) {
                        rb.MovePosition(Vector3.up * diff);  
                        return; // No spring action needed 
                    }
                
                    // Calculate spring-damper system 
                    var dampingForce = rb.velocity.y * -o_spring.floatDamping;
                    var springForce = o_spring.floatSpring * diff; 

                    // Add the sum of the two forces 
                    // TODO: Is the time.deltaTime really necessary? 
                    //     * Both the damping and spring values are pretty large 
                    rb.AddForce(Vector3.up * (springForce + dampingForce) * Time.deltaTime);

                }
            
            }
#if false
            // Debug logging of the floatation action 
            // TIP: Use te preprocessor directive above to toggle 
                Debug.Log(string.Format("diff: {0}, speed: {3}, spring: {1}, damping: {2}",
                            diff,
                            springForce, 
                            dampingForce,
                            rb.velocity.y));
#endif
             
        }

        void lateralMovement(){
            // Find the desired speed of the player and the acceleration
            switch (state.mode.current) {
            case MovementMode.WALK:
                // Basic movement, faster when sprinting
                var acc = state.isSprinting ? o_walk.runAcc : o_walk.walkAcc;
                var desiredSpeed = state.isSprinting ? o_walk.runSpeedMax : o_walk.walkSpeedMax; 
                WalkFriction(o_walk.walkDrag);
                WalkMovementSource(desiredSpeed,acc);
                break;
            case MovementMode.SLIDE: 
                    // TODO: Refactor this mess 
                    if(state.isGrounded) {
                         // Make sure the player is not stuck in ceiling
                        if(state.nearCeiling)
                        {
                            WalkFriction(o_walk.walkDrag);
                            WalkMovementSource(o_walk.runSpeedMax,o_walk.runAcc);
                        }
                        else 
                        { 
                            Vector3 nn = detection[SRLegacy.DIR.DOWN].getNormal();
                            WalkFriction(o_walk.slideDrag);
                            
                            // Slope case
                            if (nn != Vector3.up){
                                float grav = Physics.gravity.y;
                                //Debug.Log(nn);
                                /*Vector3 fullNormalForce = new Vector3(
                                         nn.x / nn.y * -grav,
                                         -grav,
                                         nn.z / nn.y * -grav); */

                                Vector3 lateralNormalForce = new Vector3(
                                         nn.x / nn.y * -grav,
                                         0,
                                         nn.z / nn.y * -grav);

                                //Debug.DrawRay(transform.position, nn * 10, Color.red);
                                //Debug.DrawRay(transform.position, fullNormalForce, Color.blue);
                                //Debug.DrawRay(transform.position, lateralNormalForce, Color.green);

                                // Absolutely cancel out the normal force 
                                // rb.AddForce(-lateralNormalForce, ForceMode.Acceleration);

                                // Difference between the horizontal component of the normal force 
                                Vector3 viewDifference = (ViewAdjustedInput() - lateralNormalForce.normalized) / 2;
                                // Debug.Log(viewDifference.magnitude);

                                // Don't correct when the normal force and view angle go in similar directions
                                if (viewDifference.magnitude < 0.34){
                                    viewDifference = Vector3.zero;
                                }
                
                                // Calculte the force 
                                Vector3 slideCohesionForce =  Vector3.ClampMagnitude(viewDifference * o_walk.slideTurnAccel, lateralNormalForce.magnitude);
                                
                                //Debug.DrawRay(transform.position, ViewAdjustedInput() * 10, Color.red);
                                //Debug.DrawRay(transform.position, slideCohesionForce, Color.cyan);

                                rb.AddForce(slideCohesionForce, ForceMode.Acceleration);
                            } else {
                                
                                Vector3 lateralSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                                Vector3 viewDifference = (ViewAdjustedInput() - lateralSpeed) /2.0f;
                                Vector3 slideTurnForce =  Vector3.ClampMagnitude(viewDifference * o_walk.slideTurnAccel, lateralSpeed.magnitude );
                                rb.AddForce(slideTurnForce, ForceMode.Acceleration);
                                

                            }


                        }
                    }
                    else 
                    {
                        // No unique behaviour when in air
                        goto case MovementMode.AIR;
                    }
            break;

            case MovementMode.JUMP:
            case MovementMode.AIR: 
                WalkMovementSource(o_air.inAirMaxSpeed,o_air.inAirAcceleration); 
            break;
            default:
                // In case of edge cases, dont move 
            break;
            }


                
        }

        Vector3 ViewAdjustedInput(){
            if (in_walk.magnitude == 0 && state.mode != MovementMode.SLIDE) {
                return Vector3.zero;
            } 

            // Don't allow strafing while sliding 
            float x_movement = in_walk.x; 
            // Only allow forward and backward movement when on the ground 
            //   unless allow forward is enabled
            float z_movement = 0.0f;
            if (state.isGrounded || o_air.allowForward) {
                // Slide locks you into going forward
                z_movement = state.mode == MovementMode.SLIDE ? 1 : in_walk.y;
            } 
            // if(state.mode == MovementMode.SLIDE){
            //     z_movement = 1; 
            //    x_movement = 0; 
            // }
            return Quaternion.AngleAxis(viewCamera.eulerAngles.y,Vector3.up) * new Vector3(x_movement,0,z_movement).normalized;

        } 

        Vector3 ProjectOnGround(Vector3 target){
            // Project the velocity on the surface we are walking on 
            if (detection[SRLegacy.DIR.DOWN]){
                var normal = detection[SRLegacy.DIR.DOWN].getNormal();
                return Vector3.ProjectOnPlane(target, normal);
            }
            return target;  
        } 

        void WalkFriction(float drag_constant){
            var speed = rb.velocity.magnitude; 
            if (speed > 0) {
                var drag = speed * -drag_constant * Time.deltaTime;
                rb.AddForce(rb.velocity.normalized * Mathf.Min(drag,0), ForceMode.Acceleration);
            } 
        }

        void WalkMovementSource(float desiredSpeed, float acceleration){
            var accelerationDirection = ViewAdjustedInput();

           // Project the input with the velocity we have already 
           var projectedSpeed = Vector3.Dot(rb.velocity,ViewAdjustedInput());
           var acc = acceleration * Time.deltaTime; 
           
           if (projectedSpeed + acc > desiredSpeed) 
                acc = desiredSpeed - projectedSpeed; 

           rb.AddForce(acc * accelerationDirection,ForceMode.Acceleration);
        }
        void WalkMovement(float desiredSpeed, float acceleration){
                // Calculate velocity vector with acquired
                var desiredVelocity = ProjectOnGround(
                        (ViewAdjustedInput() * desiredSpeed))
                            + (Vector3.up * rb.velocity.y);
                
                // See what adjustments we need to achieve the desiredVelocity; 
                var differenceVector = desiredVelocity - rb.velocity; 

                // Clamp the magnitude of the change to 1 
                if (differenceVector.magnitude > 1.0f) {
                    differenceVector.Normalize();  
                }

                // Get the actual acceleration vector
                var accelerationVector = (desiredVelocity - rb.velocity).normalized * acceleration;

                // Apply force
                rb.AddForce(accelerationVector * Time.deltaTime, ForceMode.Acceleration);

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
// 
// Messages from PlayerInput 
// 
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
                Debug.Log("Start slide");
                state.mode.Set(MovementMode.SLIDE);
            }
        }
        
        void OnSlideEnd(){
            Debug.Log("Input: Stop Sliding");
            Debug.Log(state.nearCeiling.current);
            if(!state.nearCeiling.current){
                state.mode.Set(MovementMode.WALK);
                Debug.Log("Action: Stop Sliding");
            }
        }

        void Jump(bool isInAir){
                if (!movementVals.canJump()) return;
                Debug.Log("Jumping from: " + (isInAir ? "Air" : "Ground"));
                Debug.Log("State:" + state.mode.current);
                // Cancel out any vertical speed if it is negative 
                if(rb.velocity.y < 0)
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                // Add jumpSpeed as velocity when in Air
                rb.AddForce(transform.up * o_air.velocity * rb.mass,ForceMode.Impulse);
                // Increment action counter
                if (isInAir)
                    state.curAirActions += 1;

                state.mode.Set(MovementMode.JUMP);
        } 
        void OnJumpStart(){
            state.pressingJump.Set(true); 
            // TODO Wall logic
            if((state.isGrounded || movementVals.coyoteTime.Consume())
                && state.mode != MovementMode.JUMP
                && state.mode != MovementMode.AIR){
                Jump(false);
            }
            else if(state.curAirActions < o_air.maxInAirActions
                    && state.mode != MovementMode.JUMP){
                Jump(true);
            }
            else{
                movementVals.jumpBuffering.Set();
            }
        }

        void OnJumpEnd(){
            state.pressingJump.Set(false);
            movementVals.jumpBuffering.Consume(); 
        }

        
    }

//
// Option objects 
//
// TODO: Move to separate file

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
}
