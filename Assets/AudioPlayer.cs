using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{

    [SerializeField]
    public static AudioPlayer instance;

    public FmodAudioData fmodAudio;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //FMODUnity.RuntimeManager.PlayOneShot(fmodAudio.roomTransition);
    }

    public static void PlaySFX(string nameFromFMODAudioData)
    {
        FMODUnity.RuntimeManager.PlayOneShot(nameFromFMODAudioData);
    } 




}


