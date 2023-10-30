using UnityEngine;

namespace Movement {
[RequireComponent(typeof(Collider))]
public class EffectAdder : MonoBehaviour
    {
        public MovementEffectValues effect; 

        void OnTriggerEnter(Collider other){
            if (other.CompareTag("Player")){
                Debug.Log("Found a player");
                var player = other.gameObject.GetComponent<PlayerController>();
                Debug.Log(player);
                player.movementVals.stack.AddEffect(effect.createEffect());
            }
        }
    }
}
