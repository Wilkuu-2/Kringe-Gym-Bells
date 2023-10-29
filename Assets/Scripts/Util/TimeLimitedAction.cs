using UnityEngine;

[System.Serializable]
public class TimeLimitedAction 
{
    public float maxDelay = 1f; 
    private float bufferTime = 0; 
    private bool consumed = false;
    
    public void Set(){
        bufferTime = Time.time + maxDelay;
        consumed = false;
    }

    public bool Consume(){
        if(Peek())
        {
            consumed = true;
            return true;
        }
        
        return false;
    }

    public bool Peek() {
        return !consumed && Time.time < bufferTime; 

    }

    public static bool isActive(TimeLimitedAction action){
        return action.Peek();
    } 
}
