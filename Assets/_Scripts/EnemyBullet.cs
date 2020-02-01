using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public Rigidbody2D rigidBody;

    public float bulletSpeed = 7.5f;

    //public GameObject impactEffect;

    public float damage = 50f;


    // Update is called once per frame
    void Update()
    {
        rigidBody.velocity = transform.right * bulletSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerController>().DamagePlayer(damage);
        }
        else
        {
            Debug.Log("Hitting something else " + other.gameObject.name);
        }


        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {

        Destroy(gameObject);
    }
}
