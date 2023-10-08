using UnityEngine;

namespace SRLegacy { 
    public class CursorDisabler : MonoBehaviour
    {
        public void OnEnable() {
            LockCursor();
        }
        
        public void OnExecute(){
            LockCursor();
        }

        public void OnDisable(){
            UnlockCursor();
        }
        
        public void LockCursor(){
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void UnlockCursor(){
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
