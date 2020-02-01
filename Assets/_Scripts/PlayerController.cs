using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float moveSpeed;
    public float jumpForce;
    public float offset = 0.5f;
    private float distanceToGround;
    private bool isGrounded = false;
    private Vector2 moveDirection;
    public int playerIndex = 0;
    public bool canMove = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

    public void Fire(InputAction.CallbackContext context)
    {
        Debug.Log("Fire!");
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
        if ( isGrounded  && Mathf.Abs(rb.velocity.y) < 0.01f)
        { 
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
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
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        isGrounded = true;
    }

}
