using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{

    //public FMODUnity.StudioEventEmitter repairEvent;
    public FMODUnity.StudioEventEmitter disabledEvent;
    public FMODUnity.StudioEventEmitter overheatEvent;
    public FMODUnity.StudioParameterTrigger triggerParameters;

    [FMODUnity.EventRef]
    public string repairEventName;

    FMOD.Studio.EventInstance repairEvent;

    void Start()
    {
      
    }
    public void DisablePlayer()
    {
        disabledEvent.Play();
    }

    public void StartRepairing()
    {
        repairEvent = FMODUnity.RuntimeManager.CreateInstance(repairEventName);
        repairEvent.start();
        //repairEvent.setParameterByName("end", 0f);
        Debug.Log("Starting repair");
    }

    public void InterruptRepairing()
    {
         repairEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
         Debug.Log("Stopping repair");
    }

    public void FullHealth()
    {
        //repairEvent.SetParameter("end", 1f);
        repairEvent.setParameterByName("end", 1f);
        Debug.Log("Stopping repair");
    }


    public void StartingToOverheat()
    {
        overheatEvent.Play();
    }

    public void StoppingOverheat()
    {
        overheatEvent.Stop();
    }

    public void PlayerExploded()
    {
        // HACKI HACKI KUUCHEN
        triggerParameters.enabled = true;
    }


}
