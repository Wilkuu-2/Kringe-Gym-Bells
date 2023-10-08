using UnityEngine;

namespace SRLegacy {
    public class UI_TImer : MonoBehaviour
    {
        // Timer management
        private float startTime;
        public float timerTime {
            get => Time.time - startTime;
        }

        public string timeString {
            get{
                System.TimeSpan span = System.TimeSpan.FromSeconds(timerTime);
                 return string.Format("Time: \n[{0}]",span.ToString("mm':'ss'.'ff"));
            }
        }

        public void OnEnable() {
            startTime = Time.time;
        }  
        private TMPro.TextMeshProUGUI text;
        

        // Start is called before the first frame update
        void Start()
        {
            text = GetComponent<TMPro.TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            text.text = timeString;
        } 

    }
}
