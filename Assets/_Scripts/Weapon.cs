﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public GameObject playerBulletPrefab;
    private float shootCountdown = 0f;
    public float timeBetweenShots = 0.75f;
    public float ammo = 100.0f;
    public bool isInfinite = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Shoot(GameObject firePoint)
    {
        if (shootCountdown < 0 )
        {
            if (!isInfinite)
            {
                ammo--;
            }
            Instantiate(playerBulletPrefab, firePoint.transform.position, firePoint.transform.rotation);
            AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.shotgun);
            shootCountdown = timeBetweenShots;
        }
    }


    // Update is called once per frame
    void Update()
    {
        shootCountdown -= Time.deltaTime;
    }


}
