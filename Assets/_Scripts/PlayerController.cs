﻿using System;
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
    public LayerMask enemyLayer;
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

    public float disabledModifier = 2.0f;

    private Vector2 hitPlayerOffset = new Vector2(0f, -1f);
    public bool isAI;
    private bool tryingToJump;

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
                // set it to exactly half
                playerHealth = maxHealth / 2.0f;

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
        
    }

    public void StopSounds()
    {
        sfx.StoppingOverheat();
    }

    void OnGUI()
    {
        Vector3 point = new Vector3();
        Event currentEvent = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = currentEvent.mousePosition.x;
        mousePos.y = Camera.main.pixelHeight - currentEvent.mousePosition.y;

        point = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

        GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        GUILayout.Label("Screen pixels: " + Camera.main.pixelWidth + ":" + Camera.main.pixelHeight);
        GUILayout.Label("Mouse position: " + mousePos);
        GUILayout.Label("World position: " + point.ToString("F3"));
        GUILayout.EndArea();
    }

    void DoAI()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            if (player != gameObject)
            {
                moveDirection = (player.transform.position - transform.position);
                moveDirection.Normalize();
                HandleMoveInput();
            }
        }

        repairValue = 1;
        firePressed = 1;

        if (UnityEngine.Random.Range(0, 100) > 98)
        {
            tryingToJump = true;
        }


    }

    void HandleRepairInput()
    {
        if (repairValue > 0 && oldRepairValue == 0)
        {
            startedRepairing = true;
        }

        if (repairValue == 0 && oldRepairValue > 0)
        {
            interruptRepairing = true;
        }
    }


    void HandleJumpInput()
    {
            if (tryingToJump == false)
                return;

        tryingToJump = false;

        // Make sure that we have a near 0 vertical velocity to avoid a bug when immediatly jumping when landing and borking the isGrounded
        if (waitTime <= 0f && isGrounded)
        {
            float localJumpforce = jumpForce;

            if (currentState == PlayerState.Disabled)
            {
                localJumpforce = localJumpforce / disabledModifier;
            }

            if (currentState == PlayerState.Disabled)
                AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.repairMe);

            rb.AddForce(new Vector2(0, localJumpforce), ForceMode2D.Impulse);
            waitTime = timeBeforeNextJump;
            animator.SetTrigger("Jump");
        }
    }

    // Update is called once per frame
    void Update()
    {

        HandleRepairInput();
        HandleJumpInput();

        if (isAI)
        {
            DoAI();
        }

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
        HandleMovement();

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
            
            float localMoveSpeed = moveSpeed;

            if (currentState == PlayerState.Disabled)
                localMoveSpeed = moveSpeed / disabledModifier;

            rb.velocity = new Vector2(moveDirection.x * localMoveSpeed, rb.velocity.y);
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

            bool hasShot = activeWeapon.Shoot(firePoint);


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
        tryingToJump = true;
    }

    void HandleMoveInput()
    {
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

    public void Move(InputAction.CallbackContext context)
    {
        moveDirection = context.action.ReadValue<Vector2>();

        HandleMoveInput();
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
    

    void CheckStandingOnPlayer()
    {
        // We need an offset for player on player check
        Vector2 position = new Vector2(transform.position.x + hitPlayerOffset.x, transform.position.y + hitPlayerOffset.y);
        Vector2 direction = Vector2.down;
        float distance = 1.0f;

        RaycastHit2D hitPlayer = Physics2D.Raycast(position, direction, distance, playerLayer);

        if (hitPlayer.collider != null)
        {
            isGrounded = true;
            debugText.text = "Standing on player";
        }else
        {
            debugText.text = "";
        }
    }

    void CheckStandingOnGround()
    {

        // No offset for ground check
        Vector2 position = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = Vector2.down;
        float distance = 1.0f;

        RaycastHit2D hitGround = Physics2D.Raycast(position, direction, distance, groundLayer);

        if (hitGround.collider != null)
        {
            isGrounded = true;
           // debugText.text = "Standing on ground";
        }
    }

    void CheckStandingOnEnemy()
    {
        // We need an offset for player on enemy check
        Vector2 position = new Vector2(transform.position.x + hitPlayerOffset.x, transform.position.y + hitPlayerOffset.y);
        Vector2 direction = Vector2.down;
        float distance = 1.0f;

        RaycastHit2D hitEnemy = Physics2D.Raycast(position, direction, distance, enemyLayer);

        if (hitEnemy.collider != null)
        {
            isGrounded = true;
            //debugText.text = "Standing on enemy";
        }
    }

    void CheckIsGrounded()
    {
        // sets the isGrounded boolean
        // The problem with the logic is that for some cases we need an offset and for others we don't. The origin of the ray has to be within the player for ground check and outside the player for player and enemy checks.
        // This is a bit perculiar. Now if we chose RayCastAll, the checks are also not being triggered the way I expect it. This is the only managable solution for now.
        isGrounded = false;
        CheckStandingOnPlayer();
        CheckStandingOnEnemy();
        CheckStandingOnGround();
    }
}
