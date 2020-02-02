using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class ExampleGame : MonoBehaviour
{
    public StudioParameterTrigger fmodParam;

    // Start is called before the first frame update
    void Start()
    {
        //AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.death);
        // fmod paramerter fix hack!
       // GetComponent<StudioParameterTrigger>().enabled = true;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
