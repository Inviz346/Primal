using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement References")]
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;
    public Transform orientation;
    public LayerMask ground;

    [Header("Movement Variables")]
    public float moveSpeed;
    public float rotationSpeed;
    public float gravity;

    [Header("Ground Check")]
    public float playerHeight;
    public float groundDrag;
    bool isGrounded;

    [Header("Jump Variables")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool canJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    Vector3 moveDir;

    float horizontalInput;
    float verticalInput;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        rb.freezeRotation = true;
        canJump = true;
    }

    void Update() {
        //get rotation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        //rotate player object
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(moveDir != Vector3.zero) {
            playerObj.forward = Vector3.Slerp(playerObj.forward, moveDir, Time.deltaTime * rotationSpeed);
        }

        //ground check
        isGrounded = Physics.Raycast(player.transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);

        //handle drag
        if (isGrounded) {
            rb.drag = groundDrag;
        } else if (!isGrounded) {
            rb.drag = 0;
        }

        Physics.gravity = new Vector3(0f, gravity, 0f);

        SpeedControl();

        Inputs();
    }

    void FixedUpdate() {
        MovePlayer();
    }
    
    void Inputs() {
        if (Input.GetKey(jumpKey) && canJump && isGrounded) {
            canJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer() {
        rb.AddForce(moveDir.normalized * moveSpeed * 10, ForceMode.Force);

        //on ground
        if (isGrounded) {
            rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
        } //in air 
        else if (!isGrounded) {
            rb.AddForce(moveDir.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    void SpeedControl() {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //limit velocity if needed
        if (flatVel.magnitude > moveSpeed) {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(player.transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump() {
        canJump = true;
    }
}
