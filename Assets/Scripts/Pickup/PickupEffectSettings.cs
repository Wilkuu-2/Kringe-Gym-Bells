using UnityEngine;
using Movement; 
namespace Powerup{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PickupEffectSettings", order = 1)]
    public class PickupEffectSettings : ScriptableObject
    {
        public PickupSettingsValue proteinEffects;
        public PickupSettingsValue creatineEffects;
        public PickupSettingsValue preworkoutEffects;
        public PickupSettingsValue gripEffects;
        public PickupSettingsValue beltEffects;
        
        public void TrySendMessage(PickupType type, GameObject target){
            switch(type){
                case PickupType.Protein:
                    proteinEffects.TrySendMessage(type,target);
					break;

                case PickupType.Creatine:
                    creatineEffects.TrySendMessage(type,target);
					break;

                case PickupType.Preworkout:
                    preworkoutEffects.TrySendMessage(type,target);
					break;

                case PickupType.Grip:
                    gripEffects.TrySendMessage(type,target);
					break;

                case PickupType.Belt:
                    beltEffects.TrySendMessage(type,target);
					break;

                default:
					break;
            }

        } 

        public MovementEffect GetEffect(PickupType type){
            switch(type){
                case PickupType.Protein:
                return proteinEffects.createEffect();

                case PickupType.Creatine:
                return creatineEffects.createEffect();

                case PickupType.Preworkout:
                return preworkoutEffects.createEffect();

                case PickupType.Grip:
                return gripEffects.createEffect();

                case PickupType.Belt:
                return beltEffects.createEffect();

                default:
                return MovementEffectValues.newEmpty("error").createEffect();
            }
        }
    }

    [System.Serializable]
    public class PickupSettingsValue {
        public MovementEffectValues values;
        public MovementEffect createEffect()  { return values.createEffect(); } 

        public bool sendMessage = false; 
        public bool sendType = false; 
        public string messageName = ""; 

        public void TrySendMessage(PickupType type, GameObject target){
            if(sendMessage){
                if (sendType)
                    target.SendMessage(messageName,type,SendMessageOptions.DontRequireReceiver);
                else 
                    target.SendMessage(messageName,SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
