using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;

    public float moveSpeed;
    public float jumpForce;
    public float offset = 0.5f;
    private float distanceToGround;
    private Vector2 moveDirection;
    public int playerIndex = 0;
    public bool canMove = true;

    public LayerMask groundLayer;
    public LayerMask playerLayer;
    private float timeBeforeNextJump = 0.3f;
    private float waitTime = 0.0f;
    public GameObject firePoint;

    public GameObject playerBulletPrefab;

    private float shootCountdown = 0f;
    public float timeBetweenShots = 0.75f;
    private float firePressed = 0.0f;

    public Animator animator;
    private Vector2 oldVelocity;
    private bool oldGrounded;
    private bool isGrounded = false;
    public Text debugText;

    public void DamagePlayer()
    {
        Debug.Log("Player is being damaged");
    }

    //public List<Gun> availableGuns = new List<Gun>();

    // Start is called before the first frame update
    void Start()
    {   
        distanceToGround = GetComponent<BoxCollider2D>().bounds.extents.y;
    }


    // Update is called once per frame
    void Update()
    {
        CheckIsGrounded();

        debugText.text = "Is grounded " + isGrounded.ToString();

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

        if (firePressed > 0.0f)
        {
            if (shootCountdown <= 0)
            {
                Instantiate(playerBulletPrefab, firePoint.transform.position, firePoint.transform.rotation);
                AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.shotgun);
                shootCountdown = timeBetweenShots;
            }
        }
        
        if (rb.velocity.y <= 0 && oldVelocity.y > 0 && isGrounded == false )
        {
            PlayerStartsFalling();
        }

        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }

        if (shootCountdown > 0)
        {
            shootCountdown -= Time.deltaTime;
        }


        CheckIsLanded();


        oldVelocity = rb.velocity;
        oldGrounded = isGrounded;

    }

    public void Fire(InputAction.CallbackContext context)
    {
        firePressed = context.action.ReadValue<float>();
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

    public void Jump(InputAction.CallbackContext context)
    {
        // Make sure that we have a near 0 vertical velocity to avoid a bug when immediatly jumping when landing and borking the isGrounded
        if (waitTime <= 0f && isGrounded && Mathf.Abs(rb.velocity.y) < 0.01f)
        { 
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            waitTime = timeBeforeNextJump;
            animator.SetTrigger("Jump");
        }
    }

    void CheckIsLanded()
    {
        if (oldGrounded == false && isGrounded == true)
        {
            Debug.Log("We just landed");
            animator.SetTrigger("Land");
        }
    }

    void PlayerStartsFalling()
    {
        Debug.Log("Started falling");
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

    public void Repair(InputAction.CallbackContext context)
    {
        Debug.Log("Repair!");
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveDirection = context.action.ReadValue<Vector2>();

        if (moveDirection.x < 0.0f)
        {
            firePoint.transform.localPosition = new Vector2(-0.8f, 0);
            firePoint.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180.0f));
        }
        else if (moveDirection.x > 0.0f)
        {
            firePoint.transform.localPosition = new Vector2(0.8f, 0);
            firePoint.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }


}
