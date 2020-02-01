using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public float startingHealth = 100.0f;
    private float currentHealth;

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
        Debug.Log("Damaging enemy");
        currentHealth -= damageValue;

        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
