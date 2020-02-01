using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;

    public float moveSpeed;
    public float jumpForce;
    public float offset = 0.5f;
    private float distanceToGround;
    //private bool isGrounded = false;
    private Vector2 moveDirection;
    public int playerIndex = 0;
    public bool canMove = true;

    public LayerMask groundLayer;
    public LayerMask playerLayer;
    private float timeBeforeNextJump = 0.3f;
    private float waitTime = 0.0f;
    public GameObject firePoint;

    public GameObject playerBulletPrefab;

    public void DamagePlayer()
    {
        Debug.Log("Player is being damaged");
    }

    //public List<Gun> availableGuns = new List<Gun>();

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {   
        distanceToGround = GetComponent<BoxCollider2D>().bounds.extents.y;
    }


    // Update is called once per frame
    void Update()
    {
        if (canMove)
        { 
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }


        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            return;
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
        Debug.Log("Fire!");
        Instantiate(playerBulletPrefab, firePoint.transform.position, firePoint.transform.rotation);
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
        if (waitTime <= 0f && IsGrounded() && Mathf.Abs(rb.velocity.y) < 0.01f)
        { 
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            waitTime = timeBeforeNextJump;
            //isGrounded = false;
        }
    }

    bool IsGrounded()
    {

        Vector2 position = new Vector2(transform.position.x ,transform.position.y + 0.51f);
        Vector2 direction = Vector2.down;
        float distance = 1.0f;

        Debug.DrawRay(position, direction, Color.green, 5000.0f);

        RaycastHit2D hitGround = Physics2D.Raycast(position, direction, distance, groundLayer);
        if (hitGround.collider != null )
        {
            //Debug.Log("hit ground collider name " + hitGround.collider.gameObject.name);
        }

        RaycastHit2D hitPlayer = Physics2D.Raycast(position, direction, distance, playerLayer);
        if (hitPlayer.collider != null)
        {
            //Debug.Log("hit player collider name " + hitPlayer.collider.gameObject.name);
        }

        //RaycastHit2D hit = Physics2D.Raycast(position, direction, distance);
        if (hitPlayer.collider != null || hitGround.collider != null)
        {
            return true;
        }

        return false;
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

    public void OnTriggerEnter2D(Collider2D collision)
    {
      // isGrounded = true;
    }

}
