using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Rigidbody theRB;
    public float forwardAccel = 8f;
    public float reverseAccel = 4f;
    public float maxSpeed = 50f;
    public float turnStrength = 180f;
    private float appliedTurn;
    public float gravityForce = 10f;
    public float dragOnGround = 3f;
    private float appliedDrag;

    [Header("Ground Check Settings")]
    public LayerMask whatIsGround;
    public float groundRayLength = 0.5f;
    public Transform groundRayPoint;

    [Header("Visual Elements")]
    public Transform leftFrontWheel;
    public Transform rightFrontWheel;
    public float maxWheelTurn = 25f;

    [Header("Drifting")]
    public float addedDrift = 20f;
    public float dragWhileDrifting = 1f;

    // Private variables to store states and inputs
    private float speedInput;
    private float turnInput;
    private bool grounded;
    private bool drifting;

    void Start()
    {
        // Unparent the sphere Rigidbody so it moves freely in the world
        // while the car mesh object follows it manually
        theRB.transform.parent = null;
    }

    void Update()
    {
        // Reset speed input every frame
        speedInput = 0f;

        // Get Vertical Input (W/S or Up/Down arrows)
        float verticalAxis = Input.GetAxisRaw("Vertical");
        if (verticalAxis > 0)
        {
            speedInput = verticalAxis * forwardAccel * maxSpeed;
        }
        else if (verticalAxis < 0)
        {
            speedInput = verticalAxis * reverseAccel * maxSpeed;
        }

        // Get Horizontal Input (A/D or Left/Right arrows)
        turnInput = Input.GetAxisRaw("Horizontal");

        // Only rotate the car if it is currently grounded
        if (grounded)
        {
            //Applying drifting conditions only if the player is on the ground and trying to drift
            if (drifting) {
                appliedTurn = turnStrength + addedDrift;
                appliedDrag = dragWhileDrifting;
                speedInput*=0.5f;
            }
            else {
                appliedTurn = turnStrength;
                appliedDrag = dragOnGround;
            }

            // Multiplying turnInput by VerticalAxis ensures we only turn when moving,
            // and reverses the steering direction correctly when driving backwards.
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y + (turnInput * appliedTurn * verticalAxis * Time.deltaTime),
                transform.rotation.eulerAngles.z
            );
        }

        if (leftFrontWheel != null && rightFrontWheel != null)
        {
            // Front Left Wheel handling
            leftFrontWheel.localRotation = Quaternion.Euler(
                (turnInput * maxWheelTurn)- 180f,
                leftFrontWheel.localRotation.eulerAngles.y,
                leftFrontWheel.localRotation.eulerAngles.x
            );

            // Front Right Wheel handling
            rightFrontWheel.localRotation = Quaternion.Euler(
                (turnInput * maxWheelTurn),
                rightFrontWheel.localRotation.eulerAngles.y,
                rightFrontWheel.localRotation.eulerAngles.x
            );
        }

        // Update the car's position to perfectly follow the rolling sphere's position
        transform.position = theRB.transform.position;
    }

    void FixedUpdate()
    {
        grounded = false;
        RaycastHit hit;

        // Shoot a raycast downward to check if the car is currently touching the ground
        if (Physics.Raycast(groundRayPoint.position, -transform.forward, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;

            // Align the car mesh's up vector with the surface normal vector of the ground
            transform.rotation = Quaternion.FromToRotation(transform.forward, hit.normal) * transform.rotation;
        }

        if (grounded)
        {
            // Set normal ground drag to keep the car handling feeling responsive
            theRB.linearDamping = appliedDrag;

            // Apply forward or reverse physics force if an input is active
            if (Mathf.Abs(speedInput) > 0)
            {
                theRB.AddForce(transform.up * speedInput);
            }
        }
        else
        {
            // Reduce drag in mid-air so the car flies cleanly and retains velocity 
            theRB.linearDamping = 0.1f; 

            theRB.AddForce(Vector3.up * -gravityForce * 100f); 

            //Make car tilt down slowly while in air 
            Vector3 currentForward = transform.forward; 

            float slopeIntensity = 0.3f; //How intense the slope is
            Vector3 slopedForward = (currentForward + (Vector3.up * slopeIntensity)); 
 
            float rotationSpeed = 5f; // How fast the car rotates midair 
            Quaternion targetRotation = Quaternion.LookRotation(slopedForward, transform.up).normalized; 
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        //Check if player is trying to drift
        if (Input.GetKey(KeyCode.Space)) {
            drifting = true;
        }
        else {
            drifting = false;
        }
    }
} 