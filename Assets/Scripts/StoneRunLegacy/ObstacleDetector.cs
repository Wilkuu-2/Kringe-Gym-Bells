using UnityEngine;

namespace SRLegacy{
    public enum DIR{
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
    public class ObstacleDetector : MonoBehaviour
    {

        
        public RayCastPlus[] detectionDirections;

        public RayCastPlus this[int i]{
            get => detectionDirections[i];
        }

        public RayCastPlus this[DIR d]{
            get => detectionDirections[((int)d)];
        }
    }
}
