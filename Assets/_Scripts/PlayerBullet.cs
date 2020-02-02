using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public Rigidbody2D rigidBody;

    public float bulletSpeed = 7.5f;

    //public GameObject impactEffect;

    public int damage = 50;

    [FMODUnity.EventRef]
    public string bulletImpact;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        rigidBody.velocity = transform.right * bulletSpeed;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("On trigger enter with " + other.tag + "game object name " + other.gameObject.name);
        if (other.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyController>().DamageEnemy(damage);
        }

        Destroy(gameObject);


        if (bulletImpact != "")
            AudioPlayer.PlaySFX(bulletImpact);
    }

    private void OnBecameInvisible()
    {

        Destroy(gameObject);
    }
}
