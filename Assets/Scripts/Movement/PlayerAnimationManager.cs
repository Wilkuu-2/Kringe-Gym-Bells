using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement {
    public class PlayerAnimationManager : MonoBehaviour
    {
        [SerializeField] bool isMoving;
        [SerializeField] bool isJumping;
        [SerializeField] bool isSliding; 
        [SerializeField] bool isGrounded; 
        
        public float minimum_speed = 0.8f;

        private Animator anim; 
        private PlayerController player; 
        private Rigidbody playerRb; 

        void Start(){
            anim = GetComponent<Animator>();
            player = GetComponentInParent<PlayerController>();
            playerRb = GetComponentInParent<Rigidbody>();
        }

        void Update()
        {
            isJumping = player.state.mode == MovementMode.JUMP; 
            isSliding = player.state.mode == MovementMode.SLIDE; 
            isGrounded = player.state.isGrounded; 
            isMoving = playerRb.velocity.magnitude > minimum_speed; 

            anim.SetBool("IsJumping", isJumping);
            anim.SetBool("IsSliding", isSliding);
            anim.SetBool("IsGrounded", isGrounded);
            anim.SetBool("IsMoving", isMoving);
        }
    }
} 

