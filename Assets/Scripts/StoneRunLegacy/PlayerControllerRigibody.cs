using UnityEngine;
using UnityEngine.InputSystem;


namespace SRLegacy {
    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(PlayerInput)), RequireComponent(typeof(Animator))]
    public class PlayerControllerRigibody : MonoBehaviour
    {
#region Serialized fields
        public LayerMask  groundLayer;
        public LayerMask  ceilCheckLayer;
        public ObstacleDetector detection; 
        
        public bool animate = true;

        [Header("Camera")]
        public GameObject camera;

        public MovementClass movement; 
        public InputValuesClass input;

        public float slideDrag = 0.1f;
        public float slideMinVel = 0.5f; 


        [Header("Floating")] 
        [Tooltip("How high the collider hover above the ground ")] 
        public float floatHeight   = 0.4f; 
        [Tooltip("Spring force coefficient for hovering")]
        public float floatSpring   = 50000f; 
        [Tooltip("Damping force coefficient for hovering ")]
        public float floatDamping  = 1000f;  
        [Tooltip("Max speed fed into the damping force")]
        public float floatDampingClamp = 3f;
        public float maxVerticalVelocityOnGround = 3f;


#endregion
#region State variables
        //Input state
        private Vector2 in_look , in_walk;
        // True if the sprint key is pressed
        public bool in_isSprinting {get; private set;} = false;

        // True if clinging into the wall
        public bool s_onWall {get; private set;} = false;
        // True if touching the ground
        public bool s_isGrounded {get; private set;} = false;
        // True if touching the ground on the previous physics update 
        public bool s_wasGrounded {get; private set;} = false;
        // True between jumping and touching the ground 
        public bool s_isJumping {get; private set;} = false;
        public bool s_isCloseToCeiling{get; private set;} = false;
        // Amount of double jumps and dashes after last jump/fall/etc
        public int currentInAirActions {get; private set;} =0;
        // True if sliding
        bool s_isSliding = false; 
        
#endregion
#region Objects
        // Objects from the player game-object
        private Rigidbody rb; 
        private Collider  col;
        private Animator hb_anim; 
        private WalkSoundScript walkSounds;
        
#endregion

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody>(); 
            col = GetComponent<Collider>();
            hb_anim = GetComponent<Animator>();
            walkSounds = GetComponent<WalkSoundScript>();
            
            // Disable cursor
            Cursor.lockState = CursorLockMode.Locked;
        }
        // Update is called once per frame
        void Update(){
            /*if(s_onWall){
                walkSounds.setTempo(WALK_TEMPO_STATE.WALL);
            } else if(in_isSprinting){
                walkSounds.setTempo(WALK_TEMPO_STATE.RUN);
            } else {
                walkSounds.setTempo(WALK_TEMPO_STATE.WALK);
            }*/
            
            updateAnimator();

            
        }
        
        void updateAnimator(){
           if (animate) { 
           hb_anim.SetBool("a_isSliding", s_isSliding);
           hb_anim.SetBool("a_isWalking", in_walk != Vector2.zero);
           hb_anim.SetBool("a_isGrounded", s_isGrounded);
           hb_anim.SetBool("a_isWallRunning", s_onWall);
           if(s_isGrounded && !s_wasGrounded)
                hb_anim.SetTrigger("a_tLand");
           hb_anim.SetFloat("a_WalkSpeed", in_isSprinting ? 0.6f : 1.2f);
            }
        }

#region physics 

        void determineAccelerationAndDesiredSpeed(out float acc, out float desiredSpeed){
                // Grounded Movement  
                if(s_isGrounded)
                {
                    if(s_isSliding)
                    {
                        acc = movement.walk.runAcc;
                         // Make sure the player is not stuck in ceiling
                        if(s_isCloseToCeiling)
                        {
                            desiredSpeed = movement.walk.walkSpeedMax; // Keep moving when under a obstacle
                        }
                        else 
                        { 
                            desiredSpeed = rb.velocity.magnitude - slideDrag * rb.velocity.magnitude * Time.deltaTime; // Incur drag
                        }
                    }
                    else
                    {
                        acc = in_isSprinting ? movement.walk.runAcc : movement.walk.walkAcc;
                        desiredSpeed = in_isSprinting ? movement.walk.runSpeedMax : movement.walk.walkSpeedMax; 
                    }


                }

                // Jumping movement
                else if(s_isJumping){
                    acc = movement.jump.inAirAcceleration;
                    desiredSpeed = Mathf.Max(movement.jump.inAirMaxAddedSpeed, 
                                             new Vector2(rb.velocity.x,rb.velocity.z).magnitude); 
                } 
                else {  
                    acc = 0; 
                    desiredSpeed = 0; 
                }


        }

        void applyLateralMovementForce(float acc, float desiredSpeed){
                // Calculate velocity vector with acquired speed 
                var desiredVelocity = new Vector3(s_isSliding ? 0 : in_walk.x,
                     0 , 
                     (s_isSliding ? Mathf.Abs(in_walk.y) : in_walk.y))
                      * desiredSpeed
                      + Vector3.up * rb.velocity.y;
                
                // Adjust to camera rotation
                desiredVelocity = Quaternion.AngleAxis(camera.transform.eulerAngles.y,Vector3.up) * desiredVelocity;
                
                // Get the actual acceleration vector
                var acceleration = (desiredVelocity - rb.velocity).normalized * acc;

                // Apply force
                rb.AddForce(acceleration * Time.deltaTime ,ForceMode.Acceleration);
        }
        void applyCharacterRotation(){
                if(in_walk != Vector2.zero)
                {
                    // Rotate character
                    float angle = transform.eulerAngles.y; 
                    float desiredAngle = camera.transform.eulerAngles.y + 
                        (s_isSliding ? 0 : (Mathf.Atan2(in_walk.x,in_walk.y) * Mathf.Rad2Deg));
                    
                    float turnSpeed = movement.rotationSpeed * Time.deltaTime;
                    
                    float delta = Mathf.DeltaAngle(angle, desiredAngle);
                    
                    float turning = delta / 180 * turnSpeed;
                    transform.Rotate(0,turning,0);
                    

                } 

        }
        
        // Physics update
        void FixedUpdate()
        {
            CeilCheck();
            WallCheck();
            WallRunInput();
            groundCheck(); 

            // Wall running 
            if(s_onWall){
                WallRunUpdate();
            }

            // Otherwise 
            else{ 
                // Calculated acceleration and desired speed vars
                determineAccelerationAndDesiredSpeed(out float acc, out float desiredSpeed); 
                
                if(s_isGrounded){
                    applyFloatSpring(); 
                }
                
                applyLateralMovementForce(acc, desiredSpeed);
                

                applyCharacterRotation();
            }
            
            if(s_isSliding && rb.velocity.magnitude < slideMinVel)
            {
                OnSlideEnd();
            }

            

            // Reset Angular velocity
            rb.angularVelocity = Vector3.zero;
            
            // Enable/Disable walking sound
            //walkSounds.setPlaying(s_isGrounded && in_walk != Vector2.zero);
      
            
            // Save previous grounded state
            s_wasGrounded = s_isGrounded;


        }
         
        // Physics/collision checks
        bool groundCheck(){
            // Check if the the feet are hovering above some ground 
            s_isGrounded = detection[DIR.DOWN]; 
            if(s_isGrounded){
                currentInAirActions=0;
                if(!s_wasGrounded){
                    s_isJumping = false;
                }  

                if(movement.jump.consumeBufferedJump()){
                    // Add jumpSpeed as velocity
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(transform.up * movement.jump.velocity * rb.mass,ForceMode.Impulse);
                    s_isJumping = true;
                }
            }
            else if(!s_isJumping){
                movement.jump.onFall();
            } 
            
            //if(!s_isGrounded){
            //    walkSounds.setPlaying(false);
            // }
            
            
            return s_isGrounded;
        }


        void applyFloatSpring(){
                if(!s_isJumping){
                var newY = detection[DIR.DOWN].getPoint().y + floatHeight + col.bounds.extents.y;
                var difference = newY - transform.position.y;
                var springForce = floatSpring* difference; 
                var dampingForce= floatDamping* -Mathf.Clamp(rb.velocity.y,-floatDampingClamp,floatDampingClamp);
                rb.AddForce(Vector3.up * (springForce + dampingForce) * Time.deltaTime);
               
                if(!s_isSliding){
                rb.velocity = new Vector3(rb.velocity.x,
                                         Mathf.Clamp(rb.velocity.y,-maxVerticalVelocityOnGround, maxVerticalVelocityOnGround),
                                         rb.velocity.z);
               

                    }
                }
        }

        void WallRunInput(){
            if (in_walk.x != 0 && !s_isGrounded &
                ((in_walk.x * in_walk.y > 0 && detection[DIR.RIGHT]) ||  
                 (in_walk.x * in_walk.y < 0 && detection[DIR.LEFT]))) 
            {        
                StartWallRunning();   
            } 
        }
        void StartWallRunning(){
            rb.useGravity = false; 
            s_onWall = true;
            s_isGrounded = false; 
            s_isJumping  = false; 
            //walkSounds.setPlaying(true);
        }
        void WallRunUpdate(){
            rb.AddForce(-transform.up * rb.velocity.y * movement.wall.friction * Time.deltaTime);
            if(rb.velocity.magnitude <= movement.wall.maxSpeed){
                rb.AddForce(transform.forward * movement.wall.runForce * Mathf.Abs(in_walk.y) * Time.deltaTime);
            } 

            if(false && in_walk.y < 1 ){
                StopWallRunning();
                Debug.Log("Going backwards: Cancelling wall run");
            }

            float desiredAngle = 0; 
            if(detection[DIR.LEFT]){
                var normal = detection[DIR.LEFT].getNormal();
                rb.AddForce(-normal* movement.wall.runForce / 3 * Time.deltaTime);
                desiredAngle  = (normal.sqrMagnitude > 0 ?
                    Quaternion.LookRotation(normal, Vector3.up).eulerAngles.y - 90 
                    : 
                    0  
                );
            }
            else if(detection[DIR.RIGHT]){
                var normal = detection[DIR.RIGHT].getNormal();
                rb.AddForce(-normal * movement.wall.runForce / 3 * Time.deltaTime);
                desiredAngle  = (normal.sqrMagnitude > 0 ?
                    Quaternion.LookRotation(normal, Vector3.up).eulerAngles.y + 90
                    : 
                    0  
                );
            } 
            float angle = transform.eulerAngles.y;
            float turnSpeed = movement.rotationSpeed * Time.deltaTime;
            float delta = Mathf.DeltaAngle(angle, desiredAngle);
            //Debug.Log(string.Format("WallAngle d{0} {1} -> {2}",delta, angle, desiredAngle));
            
            float turning = delta / 180 * turnSpeed;
            transform.Rotate(0,turning,0);
        }
        void StopWallRunning(){
            rb.useGravity = true; 
            s_onWall = false; 
        }

        void WallCheck(){ 
            if(detection[DIR.LEFT] == false && detection[DIR.RIGHT] == false){
                StopWallRunning();
            } 
            if ((detection[DIR.LEFT] == true || detection[DIR.RIGHT] == true) && s_onWall) {
                currentInAirActions = 0;
            }
        }

        void CeilCheck(){
            s_isCloseToCeiling = detection[DIR.UP];
        }

#endregion
        // Input
#region inputAxes
        void OnMove(InputValue val){
            in_walk = val.Get<Vector2>();
        }

        void OnLook(InputValue val){
            in_look = val.Get<Vector2>() * input.mouseSensitivity;
        }
        #endregion
#region movementKeys
        
        void OnSprintStart(){
            in_isSprinting = true;
        }
        
        void OnSprintEnd(){
            in_isSprinting = false;
        } 
        
        void OnSlideStart(){
            if(rb.velocity.magnitude > slideMinVel && !s_onWall){
                s_isSliding = true;
                hb_anim.SetBool("a_isSliding" ,true);
            }
        }
        
        void OnSlideEnd(){
            if(!s_isCloseToCeiling){
                s_isSliding = false;
                hb_anim.SetBool("a_isSliding" ,false);
            }
        }

        void OnJumpStart(){
            if(s_onWall){
                if(detection[DIR.LEFT] == true){
                    rb.AddForce((detection[DIR.LEFT].getNormal() + transform.up)*movement.wall.jumpVelocity/2 * rb.mass,ForceMode.Impulse);
                    hb_anim.SetBool("a_WallRunRight", false);
                    StopWallRunning();
                }
                else if(detection[DIR.RIGHT] == true){
                    rb.AddForce((detection[DIR.RIGHT].getNormal() + transform.up)*movement.wall.jumpVelocity/2 * rb.mass,ForceMode.Impulse);
                    hb_anim.SetBool("a_WallRunRight", true);
                    StopWallRunning();
                }
                else{
                    rb.AddForce(transform.up * movement.jump.velocity * rb.mass,ForceMode.Impulse);
                    StopWallRunning();
                }
            }
            else if(s_isGrounded || movement.jump.consumeCoyoteTime()){
                // Add jumpSpeed as velocity
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

                rb.AddForce(transform.up * movement.jump.velocity * rb.mass,ForceMode.Impulse);
                s_isJumping = true;
                hb_anim.SetTrigger("a_tJump");
            }
            else if(currentInAirActions < movement.maxInAirActions){
                // Cancel out any vertical speed if it is negative 
                if(rb.velocity.y < 0)
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                // Add jumpSpeed as velocity when in Air
                rb.AddForce(transform.up * movement.jump.velocity * rb.mass,ForceMode.Impulse);
                // Increment action counter
                currentInAirActions ++;
                s_isJumping = true;
                hb_anim.SetTrigger("a_tJump");
            }
            
            else{
                movement.jump.bufferJump();
            }
        }
        void OnDash(){
            if(!s_isGrounded && currentInAirActions < movement.maxInAirActions){
                rb.AddForce(camera.transform.forward * movement.jump.dashVelocity, ForceMode.Impulse);
                currentInAirActions ++;
            }
        }    
        #endregion 
    }

#region CategoryClasses 

    [System.Serializable]
    public class MovementClass
    {
        public WallRunningClass wall;
        public WalkingClass     walk;
        public JumpingClass     jump;
        [Range(60,3600)]
        [Tooltip("The speed at which the character rotates to the movement")]
        public float rotationSpeed = 720; 
        [Tooltip("How much double jumps or dashes can the charater do before having to land again ")]
        public int   maxInAirActions = 1;  
    }

    [System.Serializable]
    public class WalkingClass
    {
        [Tooltip("Acceleration for walking ")]
        public float walkAcc = 900;
        [Tooltip("Acceleration for running")]
        public float runAcc  = 1200; 
        [Tooltip("Max walking speed ")]
        public float walkSpeedMax = 6; 
        [Tooltip("Max sprinting speed ")]
        public float runSpeedMax = 12; 

    }

    [System.Serializable]
    public class JumpingClass
    {
        [Tooltip("Upwards velocity which gets added to the current velocity when jumping ")]
        public float velocity  = 9;  
        [Tooltip("Forwards velocity which gets added to the current velocity when dashing ")]
        public float dashVelocity  = 12; 

        [Tooltip("Acceleration that is applied in-air")]
        public float inAirAcceleration = 50;
        [Tooltip("Maximal speed that is added to in-air movements")]
        public float inAirMaxAddedSpeed = 5;

        #region  coyoteTime 
        [Tooltip("How long one can jump after falling of the edge")]
        public float coyoteTimeDelay = 1.1f;
        private float coyoteTime = 0; 
        private bool coyote_consumed = false; 

        public void onFall(){
            coyoteTime = Time.time + coyoteTimeDelay;
            coyote_consumed = false;
            }
        public bool consumeCoyoteTime() {
            if(!coyote_consumed && Time.time < coyoteTime)
            {
                coyote_consumed = true;
                return true;
            }
            
            return false;
        }
        #endregion

        #region buffering
        public float maxBufferingDelay = 1f; 
        private float bufferTime = 0; 
        private bool bufferedJumpConsumed = false;
        
        public void bufferJump(){
            bufferTime = Time.time + maxBufferingDelay;
            bufferedJumpConsumed = false;
        }

        public bool consumeBufferedJump(){
            if(!bufferedJumpConsumed && Time.time < bufferTime)
            {
                bufferedJumpConsumed = true;
                return true;
            }
            
            return false;
        }

        #endregion 

    }

    [System.Serializable]
    public class WallRunningClass
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

    [System.Serializable]
    public class InputValuesClass
    {
        [Tooltip("How sensitive is the mouse in both axes")]
        public Vector2 mouseSensitivity = new Vector2(1,1);  
    }


#endregion 
}
