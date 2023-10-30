using System.Collections.Generic;
using System; 
using UnityEngine;

namespace Movement {
    [Serializable]
    public struct MovementEffectValues{
        public string name; 
        public float groundSpeedChange; 
        public float groundAccelChange; 
        public float airSpeedChange; 
        public float airAccelChange; 
        public int inAirActionChange;
        public float duration; 
        public bool  blockSlide; 
        public bool  blockJump; 
        public bool  enableAutoBhop; 

        public static MovementEffectValues newEmpty(string name){
            MovementEffectValues empty = new MovementEffectValues();
            empty.name = name; 
            return empty; 
        } 
        public MovementEffect createEffect(){
            return new MovementEffect(this);
        }

        public void SumInPlace(MovementEffectValues b){
            groundSpeedChange    += b.groundSpeedChange;
            groundAccelChange    += b.groundAccelChange;
            airSpeedChange    += b.airSpeedChange;
            airAccelChange    += b.airAccelChange;
            inAirActionChange      += b.inAirActionChange;
            // Duration should not be used in summed effects, use the stack instead
            // duration             = duration < b.duration ? duration : b.duration 

            blockSlide = blockSlide || b.blockSlide;
            blockJump  = blockJump  || b.blockJump;
            enableAutoBhop  = enableAutoBhop  || b.enableAutoBhop;
       
        }

    }
    [System.Serializable]
    public class MovementEffect : TimeLimitedAction {
        public MovementEffectValues values; 
       
        public MovementEffect(MovementEffectValues effect_values, bool active = true){
            values = effect_values; 
            maxDelay = effect_values.duration; 
            if (active)
                Set();
        } 
    } 

    [System.Serializable]
    public class MovementEffectStack{
        [SerializeField][ReadOnly] List<MovementEffect> effects; 
        [SerializeField][ReadOnly] bool addedSinceLastRefresh = true; 

        public MovementEffectStack(){
            effects = new List<MovementEffect>();
        }

        public void AddEffect(MovementEffect effect){
            effects.Add(effect);
            addedSinceLastRefresh = true; 
        }
        
        // Cleans up used up effects and returns when the effects need to be restacked.
        // TODO: Restack more dynamically
        private bool RefreshEffects(){
            bool added = addedSinceLastRefresh; 
            addedSinceLastRefresh = false; 
            return 
                effects.RemoveAll(e => !e.Peek()) > 0
                || added;
        } 

        // Reasigns stacked effect values when stuff needs to be restacked  
        public bool StackedEffectValues(ref MovementEffectValues stacked){
            if (!RefreshEffects()){
                return false; 
            } 
            Debug.Log("Effect Stacking");
            // When some effects are out restack everything 
            stacked = MovementEffectValues.newEmpty(stacked.name);
            
            foreach (MovementEffect effect in effects){
                stacked.SumInPlace(effect.values);
            } 

            return true; 
            
        }

    }
}
