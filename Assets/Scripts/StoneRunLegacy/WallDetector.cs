using UnityEngine;
using System;
using System.Collections.Generic;

namespace SRLegacy { 
    public class ObstacleDetectorOld : MonoBehaviour
    {
        [Serializable]
        public struct WallDetectorDictStruct{ 
            public string[] keys; 
            public RayCastPlus[] raycasts; 

            public bool isCorrect(){
                return keys.Length == raycasts.Length && keys.Length != 0 && raycasts.Length != 0;
            }
        }

        public WallDetectorDictStruct detectionDirections;
        private Dictionary<string,RayCastPlus> detectionDict;

        public void Start(){
            Debug.Assert(detectionDirections.isCorrect(), 
                            string.Format("The detectionDirections struct has a unequal amount of keys({0}) and raycasts({1})", 
                                    detectionDirections.keys.Length,
                                    detectionDirections.raycasts.Length));

            for(int i = 0; i < detectionDirections.keys.Length; i++){
                detectionDict.Add(detectionDirections.keys[i],detectionDirections.raycasts[i]);
            }
        }

        public RayCastPlus this[string key]{
            get => detectionDict[key];
        }
    }
}
