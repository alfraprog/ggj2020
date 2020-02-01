using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FmodAudioData", fileName = "New Audio Sheet")]
public class FmodAudioData : ScriptableObject
{
    [Header("UI")]
    [FMODUnity.EventRef]
    public string roomTransition = null;


    [Header("Player")]
    [FMODUnity.EventRef]
    public string shotgun = null;
    [FMODUnity.EventRef]
    public string death = null;
    [FMODUnity.EventRef]
    public string overheat = null;
    [FMODUnity.EventRef]
    public string weaponPickup = null;
 

}
