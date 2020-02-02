using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float weaponOffset = 0.8f;
    public Rigidbody2D rb;
    public PlayerUIController uiController;

    public float moveSpeed;
    public float jumpForce;
    public float offset = 0.5f;
    private Vector2 moveDirection;
    public int playerIndex = 0;
    public bool canMove = true;


    public LayerMask groundLayer;
    public LayerMask playerLayer;
    private float timeBeforeNextJump = 0.3f;
    private float waitTime = 0.0f;
    public GameObject firePoint;

    private float firePressed = 0.0f;

    public Animator animator;
    private Vector2 oldVelocity;
    private bool oldGrounded;
    private bool isGrounded = false;
    public Text debugText;

    float playerHealth;
    public float maxHealth = 100.0f;

    public float healAmount = 30.0f;

    public SFXController sfx;

    public GameObject healEffect;

    private enum PlayerState
    {
        Normal,Repairing,Disabled
    }

    private PlayerState currentState = PlayerState.Normal;
    public float damageOverTime = 5f;
    private float repairValue = 0f;
    public float healDistance = 3.0f;

    private Weapon activeWeapon;
    public Weapon starterWeapon;
    private float oldRepairValue;
    private bool startedRepairing;
    private bool interruptRepairing;

    private GameObject currentHealEffect;

    public void AddWeapon(Weapon weapon)
    {
        // destroy previous weapon
        foreach (Transform child in firePoint.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        activeWeapon = Instantiate(weapon, firePoint.transform);
    }


    public void DamagePlayer(float damageValue)
    {
        // only take damage when normal
        if (currentState == PlayerState.Normal)
        {
            playerHealth -= damageValue;
            //animator.SetFloat("health", playerHealth);

            uiController.UpdateHealth(playerHealth);

            if (playerHealth < maxHealth / 2.0f)
            {
                sfx.StartingToOverheat();
                sfx.DisablePlayer();
                currentState = PlayerState.Disabled;
                interruptRepairing = true;
                // stop horizontal movement
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // create as child of this object
        currentHealEffect = Instantiate(healEffect, transform);
        currentHealEffect.SetActive(false);

        if (starterWeapon == null)
        {
            Debug.LogError("We have no weapon!!!!");
        }

        AddWeapon(starterWeapon);

        playerHealth = maxHealth;
        playerIndex = GameController.instance.GetSpawnedPlayerCount();
        GetComponent<PlayerUIController>().SetColor(playerIndex);
    }



    public void Repair(InputAction.CallbackContext context)
    {
        if (currentState == PlayerState.Disabled)
            return;

        Debug.Log("The repair value " + repairValue);
        repairValue = context.action.ReadValue<float>();
        if (repairValue > 0 && oldRepairValue == 0)
        {
            startedRepairing = true;
        }

        if (repairValue == 0 && oldRepairValue > 0)
        {
            interruptRepairing = true;
        }


    }
    // Update is called once per frame
    void Update()
    {
        if (startedRepairing )
        {
            sfx.StartRepairing();

            currentHealEffect.SetActive(true);
            //currentHealEffect.transform.position = transform.position;
        }

        if (interruptRepairing)
        {
            sfx.InterruptRepairing();
            currentHealEffect.SetActive(false);
        }


        if (repairValue > 0f  && currentState == PlayerState.Normal)
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
        
            foreach (GameObject gameObjectToTest in gameObjects)
            {
                if (gameObjectToTest == gameObject)
                {
                    continue;
                }else
                {
                    if (Vector2.Distance(gameObjectToTest.transform.position, gameObject.transform.position) < healDistance)
                    {
                        
                        gameObjectToTest.GetComponent<PlayerController>().ReceiveHealing( healAmount * Time.deltaTime );
                    }
                }
            }
        }

        CheckIsGrounded();

        if (currentState == PlayerState.Disabled)
        {
            DoDisabledLogic();
        }

        debugText.text = "Is grounded " + isGrounded.ToString();

        if (currentState == PlayerState.Normal)
        {
            DoNormalPlayerLogic();
        }

        UpdateTimers();

        CheckIsLanded();

        oldVelocity = rb.velocity;
        oldGrounded = isGrounded;

        // Repair needs to be button bashed
        //repairValue = 0f;
        //Debug.Log("Old repair " + oldRepairValue);
        //Debug.Log("repair value " + repairValue);

        oldRepairValue = repairValue;
        interruptRepairing = false;
        startedRepairing = false;
        // repairValue = 0;



    }

    void UpdateTimers()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
    }

    void DoDisabledLogic()
    {
        playerHealth -= damageOverTime * Time.deltaTime;
        animator.SetFloat("health", playerHealth);
        uiController.UpdateHealth(playerHealth);
        if (playerHealth <= 0)
        {
            sfx.PlayerExploded();
            GameController.instance.GameOver();
        }
    }

    void HandleMovement()
    {
        //Debug.Log("y velocity "+rb.velocity.y);
        if (canMove)
        {
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
            animator.SetFloat("HorizonalSpeed", moveDirection.x);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void HandleShooting()
    {
        if (firePressed > 0.0f)
        {

            activeWeapon.Shoot(firePoint);
            if (activeWeapon.ammo <= 0 && !activeWeapon.isInfinite)
            {
                Debug.Log("Ran out of bullets");
                AddWeapon(starterWeapon);
            }
        }
    }

    void DoNormalPlayerLogic()
    {
        HandleMovement();
        HandleShooting();

        if (rb.velocity.y <= 0 && oldVelocity.y > 0 && isGrounded == false)
        {
            PlayerStartsFalling();
        }
    }

    public void ReceiveHealing(float healAmount)
    {
        if (playerHealth < maxHealth)
        {
            playerHealth += healAmount;

            // maybe we should re enable the player when above half health

            if (playerHealth >= maxHealth)
            { 
                playerHealth = maxHealth;
                currentState = PlayerState.Normal;
                sfx.FullHealth();
                sfx.StoppingOverheat();
                Debug.Log("Went to maximum");
            }


        }

        uiController.UpdateHealth(playerHealth);
        animator.SetFloat("health", playerHealth);
    }

    public void EnableMovement()
    {
       canMove = true;
       GetComponent<Rigidbody2D>().isKinematic = false;
       GetComponent<BoxCollider2D>().enabled = true;
    }

    public void DisableMovement()
    {
        canMove = false;
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<BoxCollider2D>().enabled = false;
    }

    /* Input handling */

    public void Fire(InputAction.CallbackContext context)
    {
        firePressed = context.action.ReadValue<float>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (currentState == PlayerState.Disabled)
        {
            AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.repairMe);
            return;
        }


        // Make sure that we have a near 0 vertical velocity to avoid a bug when immediatly jumping when landing and borking the isGrounded
        if (waitTime <= 0f && isGrounded && Mathf.Abs(rb.velocity.y) < 0.01f)
        { 
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            waitTime = timeBeforeNextJump;
            animator.SetTrigger("Jump");
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveDirection = context.action.ReadValue<Vector2>();

        if (moveDirection.x < 0.0f)
        {
            firePoint.transform.localPosition = new Vector2(-weaponOffset, 0);
            firePoint.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180.0f));
            activeWeapon.transform.localScale = new Vector2(1, -1);
        }
        else if (moveDirection.x > 0.0f)
        {
            firePoint.transform.localPosition = new Vector2(weaponOffset, 0);
            firePoint.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            activeWeapon.transform.localScale = new Vector2(1, 1);
        }
    }


    void CheckIsLanded()
    {
        if (oldGrounded == false && isGrounded == true)
        {
            //Debug.Log("We just landed");
            if (!animator.GetBool("Land"))
            {
                animator.SetTrigger("Land");
            } 
            
        }
    }

    void PlayerStartsFalling()
    {
        //Debug.Log("Started falling");
        animator.SetTrigger("Fall");
    }

    void CheckIsGrounded()
    {
        Vector2 position = new Vector2(transform.position.x , transform.position.y);
        Vector2 direction = Vector2.down;
        float distance = 1.0f; 

        Debug.DrawRay(position, direction, Color.green, 5000.0f);

        RaycastHit2D hitGround = Physics2D.Raycast(position, direction, distance, groundLayer);

        bool checkGround = false;
        bool checkPlayer = false;

        if (hitGround.collider != null )
        {
            checkGround = true;
        }

        RaycastHit2D hitPlayer = Physics2D.Raycast(position, direction, distance, playerLayer);
        if (hitPlayer.collider != null && hitPlayer.collider.gameObject != gameObject)
        {
            checkPlayer = true;
        }

        //RaycastHit2D hit = Physics2D.Raycast(position, direction, distance);
        if (checkPlayer || checkGround)
        {
            isGrounded = true;
        }else
        { 
            isGrounded = false;
        }
    }





}
