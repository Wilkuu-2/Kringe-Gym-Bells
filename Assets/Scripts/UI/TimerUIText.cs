using UnityEngine;
using TMPro; 

namespace UI { 
    public class TimerUIText : MonoBehaviour
    {
        // Text 
        private TextMeshProUGUI ui_text;  
        public TimeCounter timer; 

        // Unity methods
        public void Start(){
            timer = new TimeCounter(); 
            ui_text=GetComponent<TextMeshProUGUI>();
            Debug.Log(ui_text);
        }

        public void Update(){
            ui_text.text = timer.displayString;
        }

        
    }
}
