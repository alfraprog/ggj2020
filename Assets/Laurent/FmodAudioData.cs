using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FmodAudioData", fileName = "New Audio Sheet")]
public class FmodAudioData : ScriptableObject
{
    [Header("Music")]
    [FMODUnity.EventRef]
    public string musicTune01 = null;
    [FMODUnity.EventRef]
    public string musicTune02 = null;

    [Header("UI")]
    [FMODUnity.EventRef]
    public string roomTransition = null;

    [Header("Player")]
    [FMODUnity.EventRef]
    public string shotgun = null;
    [FMODUnity.EventRef]
    public string death = null;
    [FMODUnity.EventRef]
    public string playerOverheat = null;
    [FMODUnity.EventRef]
    public string playerWeaponPickup = null;
    [FMODUnity.EventRef]
    public string playerRaygun = null;
    [FMODUnity.EventRef]
    public string playerRepair = null;
    [FMODUnity.EventRef]
    public string revive = null;



    [Header("Enemy")]
    [FMODUnity.EventRef]
    public string enemyDeath = null;
    [FMODUnity.EventRef]
    public string enemyBulletImpact = null;

}
