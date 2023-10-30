using System; 
using UnityEngine;

[Serializable]
public class TimeCounter
{
    public enum TimerState {
        STOPPED, 
        RUNNING, 
        PAUSED
    } 
                                                                                                                                                                                                                                                                                                                             
    private float startTime, adjustedTime, lastPauseTime;
    

    public TimerState state = TimerState.STOPPED;
    public float elapsedTime {get {
       switch (state){
           case TimerState.PAUSED:
               return lastPauseTime - adjustedTime; 
           
           case TimerState.STOPPED: 
               return 0.0f;

           default: 
                return Time.time - adjustedTime;  
       }
    }}

    public string displayString{get{
        TimeSpan span = TimeSpan.FromSeconds(elapsedTime);  
        return span.ToString("mm':'ss'.'ff");  
    }}

    private void _init_timer(){
        startTime = Time.time;
        adjustedTime = startTime;
        lastPauseTime = 0.0f;
        state = TimerState.RUNNING; 
    } 
    public void StartTimer(){
        if(state != TimerState.RUNNING){
            _init_timer(); 
        }
    }

    public void Restart(){
        switch (state){
            case TimerState.RUNNING:
                Debug.Log("Restarting running timer.");
            break; 
            case TimerState.PAUSED:
                Debug.Log("Restarting paused timer.");
            break;
            default:
            break; 
        }
        _init_timer(); 
    }
    public void Stop(){
        state = TimerState.STOPPED; 
        Debug.Log("Timer stopped with state: " + state);
    }

    public void Pause(){
        if(state == TimerState.RUNNING) {
            lastPauseTime = Time.time; 
            state = TimerState.PAUSED;
        }

    }

    public void Unpause(){
        if(state == TimerState.PAUSED) {
            adjustedTime += Time.time - lastPauseTime;
            state = TimerState.RUNNING;
        }
    }


}
