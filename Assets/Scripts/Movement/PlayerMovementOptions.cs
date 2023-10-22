using UnityEngine;

namespace Movement{
    [System.Serializable]
    public class GroundClass 
    {
        [Header("Ground Movement")]
        [Tooltip("Acceleration for walking ")]
        public float walkAcc = 15;
        [Tooltip("Acceleration for running")]
        public float runAcc  = 20; 
        [Tooltip("Max walking speed ")]
        public float walkSpeedMax = 6; 
        [Tooltip("Max sprinting speed")]
        public float runSpeedMax = 12; 
        public float walkDrag = 10f; 

        [Header("Slide")]
        [Tooltip("Resistance of the ground when sliding")]
        public float slideDrag = 0.1f;
        [Tooltip("Minimal velocity allowed for sliding")]
        public float slideMinVel = 0.5f; 
        [Tooltip("The force used to turn while sliding")]
        public float slideTurnForce = 60000f;

    }

    [System.Serializable]
    public class AerialClass {
        [Header("General")]
        [Tooltip("How much double jumps or dashes can the charater do before having to land again ")]
        public int   maxInAirActions = 1;  

        [Tooltip("Upwards velocity which gets added to the current velocity when jumping ")]
        public float velocity  = 9;  

        [Header("Strafing")]
        [Tooltip("Acceleration that is applied in-air")]
        public float inAirAcceleration = 1;
        [Tooltip("Allow adding forward (or backward) movement in-air.\nDisable for Source-like strafing")]
        public bool  allowForward = false; 
        [Tooltip("Allow buffering jumps constantly when spacebar is held")]
        public bool  auto_bhop = true; 

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

    [System.Serializable] 
    public class SpringOptions {
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

    }
}

