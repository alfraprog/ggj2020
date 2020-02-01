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


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        distanceToGround = GetComponent<BoxCollider2D>().bounds.extents.y;
    }


    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
    }

    public void Fire(InputAction.CallbackContext context)
    {
        Debug.Log("Fire!");
    }


    public void Jump(InputAction.CallbackContext context)
    {
       // Debug.Log(" rb.velocity.y " + rb.velocity.y);


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
        Debug.Log("landed");
    }


    void FixedUpdate()
    {
        

        var gamepad = Gamepad.current;
        if (gamepad == null)
            return; // No gamepad connected.

        if (gamepad.rightTrigger.wasPressedThisFrame)
        {
            // 'Use' code here
        }

        // Vector2 move = gamepad.leftStick.ReadValue();
        // rb.AddForce(Vector3.forward * driveForce * input, ForceMode.Force);
        /*
        if (gamepad.dpad.left.isPressed)
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
        else if (gamepad.dpad.right.isPressed)
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        else
            rb.velocity = new Vector2(0, rb.velocity.y);
            */

   
      //  if (gamepad.aButton.wasPressedThisFrame)
           // rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        // 'Move' code here
    }
}
