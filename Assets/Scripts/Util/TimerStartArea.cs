using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimerEditArea : MonoBehaviour
{
    public enum AreaKind {
        START,
        RESTART, 
        PAUSE, 
        PAUSEPERM,
        STOP,
    } 
    public AreaKind kind = AreaKind.RESTART; 
    public UI.TimerUIText timerui; 
    public string PlayerTag = "Player";
    public void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == PlayerTag){
            switch (kind) {
                case AreaKind.START: 
                    timerui.timer.StartTimer();
                    break;
                case AreaKind.RESTART:
                    timerui.timer.Restart();
                break; 
                case AreaKind.PAUSE:
                case AreaKind.PAUSEPERM:
                    timerui.timer.Pause();
                break; 
                case AreaKind.STOP:
                    timerui.timer.Stop();
                break; 

            } 
        } 
    }
    public void OnTriggerExit(Collider other){
        if(other.gameObject.tag == PlayerTag){
            switch (kind) {
                case AreaKind.PAUSE:
                    timerui.timer.Unpause();
                break; 
                default:
                break;
            } 
        } 
    }
}

