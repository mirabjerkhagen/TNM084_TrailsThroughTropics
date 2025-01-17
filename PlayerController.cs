using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Transform head;
    public Animator animator;  // Reference to the Animator

    [Header("Configurations")]
    public float walkSpeed = 5f;
    public float runSpeed = 25f;
    public float rotationSpeed = 5f;

    private float currentSpeed;

    void Start()
    {
        // Ensure we have a reference to the Animator component
        if (animator == null) {
            animator = GetComponent<Animator>();
        }
    }

   void Update()
{
    // Check if there's any movement input (any direction key pressed)
    bool hasMovementInput = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    animator.SetBool("girlGo", hasMovementInput);

    // Determine if the player is running (Shift key is held down)
    bool isRunning = Input.GetKey(KeyCode.LeftShift) && hasMovementInput;
    animator.SetBool("girlRun", isRunning);

    bool jump = Input.GetKey(KeyCode.Space); 

    if(jump) {

        animator.SetTrigger("girlJump"); 
        jump = false; 
    }

    // Adjust the speed based on whether we're running or walking
    currentSpeed = isRunning ? runSpeed : walkSpeed;

    // Get input for movement direction and calculate its magnitude
    float horizontalInput = Input.GetAxis("Horizontal");
    float verticalInput = Input.GetAxis("Vertical");
    Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);

    // Set the Speed parameter in the animator to reflect movement input magnitude
    animator.SetFloat("Speed", movementDirection.magnitude);
}


    void FixedUpdate()
    {
        // Calculate the new velocity based on movement input and current speed
        Vector3 newVelocity = Vector3.up * rb.velocity.y;
        newVelocity.x = Input.GetAxis("Horizontal") * currentSpeed;
        newVelocity.z = Input.GetAxis("Vertical") * currentSpeed;

        // Update the rigidbody velocity for movement
        rb.velocity = newVelocity;

        // Rotate the player to face the movement direction
        Vector3 movementDirection = new Vector3(newVelocity.x, 0, newVelocity.z);
        if (movementDirection.magnitude > 0.1f)
        {
            Quaternion uprightRotation = Quaternion.Euler(-90f, 0f, 0f);
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection) * uprightRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

//     void FixedUpdate()
// {
//     // Calculate the new velocity based on movement input and current speed
//     Vector3 newVelocity = Vector3.up * rb.velocity.y;
//     newVelocity.x = Input.GetAxis("Horizontal") * currentSpeed;
//     newVelocity.z = Input.GetAxis("Vertical") * currentSpeed;

//     // Update the rigidbody velocity for movement
//     rb.velocity = newVelocity;

//     // Rotate the player to face the movement direction
//     Vector3 movementDirection = new Vector3(newVelocity.x, 0, newVelocity.z);
//     if (movementDirection.magnitude > 0.1f)
//     {
//         Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
//         transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
//     }
// }

}
