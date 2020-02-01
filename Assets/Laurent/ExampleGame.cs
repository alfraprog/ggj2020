using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.death);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
