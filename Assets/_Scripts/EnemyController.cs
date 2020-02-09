using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public float startingHealth = 100.0f;
    private float currentHealth;
    public GameObject destructionEffect;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DamageEnemy(float damageValue)
    {
        currentHealth -= damageValue;

        if (currentHealth <= 0f)
        {
            if (destructionEffect != null)
                Instantiate(destructionEffect, transform.position, transform.rotation);

            Destroy(gameObject);
        }
    }

    public float GetCurrentHealth() 
    {
        return currentHealth;
    }
}
