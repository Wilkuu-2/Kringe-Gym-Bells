using UnityEngine;

namespace SRLegacy { 
    public enum WALK_TEMPO_STATE{
        WALK,
        RUN,
        WALL
    }

    public class WalkSoundScript : MonoBehaviour
    {
        public string[] soundTags; 
        public ObstacleDetector obstacles;
        public float[] tempoPitches = {1f,2f,1.5f}; 
        public GameObject emitterObject; 
        public DIR[] directions = {DIR.DOWN, DIR.LEFT, DIR.RIGHT} ;
        private FMODUnity.StudioEventEmitter emitter;
        private int prev_sound = 0; 
        private WALK_TEMPO_STATE tempo = WALK_TEMPO_STATE.WALK;
        private bool isPlaying = true;
        // Start is called before the first frame update
        void Start()
        {
            emitter = emitterObject.GetComponent<FMODUnity.StudioEventEmitter>();
            emitter.SetParameter("Location",0);

            isPlaying = emitterObject.activeSelf;
            Debug.Log(emitterObject);
        }

        public void setTempo(WALK_TEMPO_STATE ts){
            if(ts != tempo)
            {
                tempo = ts; 
                emitter.EventInstance.setPitch(tempoPitches[(int)tempo]);
                
                // Get the DSP 
                // FIXME: Idk if this could be done at the start
                FMOD.DSP dsp = new FMOD.DSP();

                emitter.EventInstance.getChannelGroup(out FMOD.ChannelGroup cg);
                cg.getNumDSPs(out int NumDSPs);
                for(int i = 0; i < NumDSPs; i++){
                    cg.getDSP(i,out FMOD.DSP tmp);

                    tmp.getInfo(out string dspName, out uint version, out int channels, out int configWidth, out int configHeight);
                    if (dspName.Contains("Pitch Shifter")){
                        dsp = tmp;
                    }
                }

                dsp.setParameterFloat((int)FMOD.DSP_PITCHSHIFT.PITCH,
                                       Mathf.Clamp(1f/tempoPitches[(int)tempo],0.5f,2f));
            }
        }
        

        public void setPlaying(bool isActive)
        {
            isPlaying = isActive;
            if(emitterObject.activeSelf != isPlaying)
            {
                emitterObject.SetActive(isPlaying);
            }
        } 

        // Update is called once per frame
        void Update()
        {

            if(isPlaying){
                int selectedSound = -1;
                

                foreach(DIR dir in directions){
                    var obstacle = obstacles[dir];
                        
                    if(obstacle)
                    {
                        for(int i =0; i < soundTags.Length; i++)
                        {
                            if(obstacle.getCollider().CompareTag(soundTags[i]))
                            {
                                selectedSound = i; 
                                if(prev_sound != selectedSound)
                                emitter.SetParameter("Location", selectedSound);
                                break;
                            }
                        }
                    }
                    if(selectedSound != -1)
                        break;
                    
                }
                
                if(selectedSound < 0)
                    if(emitter.IsPlaying()) emitter.Stop();
                else 
                    if(!emitter.IsPlaying()) emitter.Play();

                prev_sound = selectedSound; 
            }
        }

    }
}
