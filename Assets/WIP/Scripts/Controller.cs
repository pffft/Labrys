using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Controller : MonoBehaviour
{
    #region Public fields
    [Tooltip("The camera that this controller will control.")]
    public Camera AttachedCamera;

    // Note: this default is the height 5'8.5" (=1.75m), minus the correction for
    // standard top of head to eye level (=11.2cm = 0.112m).
    [Tooltip("How tall is the player, in meters?")]
    public float playerHeight = 1.628f;

    [Tooltip("How fast does the controller move when walking, in meters per second?")]
    public float movementSpeed = 4f;

    [Tooltip("How fast does the controller move when sprinting?")]
    public float sprintingSpeed = 6f;

    [Tooltip("How sensitive is the mouse?")]
    public float rotationScale = 5f;

    [Tooltip("How high can the player jump, in meters?")]
    public float jumpHeight = 1f;

    [Tooltip("How zoomed in can the screen be? E.g., 2 = 2x max zoom.")]
    public float MaxZoom = 1.5f;

    [Tooltip("How long does it take to zoom fully in or out? In seconds.")]
    public float TimeToZoom = 0.2f;

    [Tooltip("If true, we can only jump off of GameObjects with a \"Floor\" tag.")]
    public bool RespectFloorTags = false;

    #endregion

    #region Private fields
    // The unzoomed FOV of the camera.
    private float baseFOV;

    // The current zoom level.
    private float zoom = 1f;

    // How much time we've spent zooming (to lerp between min/max zoom)
    private float currentZoomTime = 0f;

    // A reference to the rigidbody. Needed for moving and jumping.
    private Rigidbody body;

    // Are we on the ground?
    private bool grounded = false;

    // Used for determining current status of jump.
    private int jumpCooldown;

    // The normal vector of the ground; or up, if not on the ground.
    private Vector3 normal;

    // How far away from the ground can we be, and still be considered "grounded"?
    private const float groundDelta = 0.1f;

    private Vector3 gravityVector;

    // Pitch defines up/down angle; for Unity it's in the range [-90, 90], where
    // +90 is straight down and -90 is straight up.
    private float pitch;

    // Yaw defines left/right angle. We make sure it's between [0, 360] to avoid
    // floating point inconsistencies.
    private float yaw;

    // The camera isn't at the exact center of the gameobject- it's a bit up
    private Vector3 cameraOffset;

    // Are we currently sprinting?
    private bool sprinting = false;

    private Vector3 movementVector;

    #endregion

    // Use this for initialization
    void Start()
    {
        baseFOV = AttachedCamera.fieldOfView;
        body = GetComponent<Rigidbody>();

        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        gameObject.transform.localScale = playerHeight / 2f * Vector3.one;
        cameraOffset = playerHeight / 2f * Vector3.up;
    }

    // Update is called once per frame
    void Update()
    {
        #region Zoom
        if (Input.GetKey(KeyCode.Mouse1))
        {
            currentZoomTime += Time.deltaTime;
        }
        else
        {
            currentZoomTime -= Time.deltaTime;
        }
        currentZoomTime = Mathf.Clamp(currentZoomTime, 0f, TimeToZoom);

        zoom = Mathf.Lerp(1f, MaxZoom, currentZoomTime / TimeToZoom);
        AttachedCamera.fieldOfView = baseFOV / Mathf.Log(1 + zoom, 2f);
        #endregion

        #region Looking around with mouse
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Constrain pitch and yaw
        pitch = Mathf.Clamp(pitch + (rotationScale * mouseY), -90, 90);
        yaw = Mathf.Repeat(yaw + (rotationScale * mouseX), 360);

        // Create quaternions from the pitch/yaw values
        Quaternion xRotation = Quaternion.AngleAxis(pitch, Vector3.left);
        Quaternion yRotation = Quaternion.AngleAxis(yaw, Vector3.up);

        // Combine them in the right order to avoid rotation along z axis
        Quaternion viewRotation = yRotation * xRotation;
        #endregion

        #region Movement
        // Sprinting status
        this.sprinting = Input.GetKey(KeyCode.LeftShift);

        // Create a vector in the direction of where we're looking
        Vector3 viewVector = viewRotation * Vector3.forward;

        // Create a new vector that's orthogonal to normal, but in the direction
        // of the view vector. Normalize it as well.
        float scalarProjection = Vector3.Dot(viewVector, normal.normalized);
        Vector3 vectorProjection = scalarProjection * normal.normalized;
        Vector3 vectorRejection = viewVector - vectorProjection;

        Vector3 forwardVector = vectorRejection.normalized;
        Vector3 rightVector = Vector3.Cross(-forwardVector, normal).normalized;

        // Forward/backward motion
        float vertical = Input.GetAxis("Vertical");

        // Left/right motion (strafe)
        float horizontal = Input.GetAxis("Horizontal");

        // Vectors for movement in the respective directions
        Vector3 verticalVector = vertical * forwardVector;
        Vector3 horizontalVector = horizontal * rightVector;

        float movementStrength = Mathf.Sqrt(verticalVector.sqrMagnitude + horizontalVector.sqrMagnitude);

        // Prevent diagonal movement being faster; adjust based on sprinting bool
        this.movementVector = (sprinting ? sprintingSpeed : movementSpeed) * movementStrength * (verticalVector + horizontalVector).normalized;
        #endregion

        // Try to scoot the player in the intended direction. Physics can stop this
        // due to either collisions or slopes.
        // TODO MovePosition isn't accurate. Can jump into walls and stall falling,
        // due to this script and physics computations not agreeing.
        // Idea: have a target max velocity, and apply force in that direction until
        // that velocity is (roughly) attained. Constrain to a max acceleration.
        //body.MovePosition(transform.position + (Time.deltaTime * movementVector));
        //body.MovePosition(transform.position + (Time.deltaTime * movementVector));
        //Debug.Log("Delta time: " + Time.deltaTime);


        // Apply the new changes to the camera.
        AttachedCamera.transform.rotation = viewRotation;
        AttachedCamera.transform.position = transform.position + cameraOffset;

    }

    // TODO use four computations in cardinal directions to get a better picture
    // of where we are in the air. If at least one collides, we're grounded and
    // the normal is the average of the normals found. Else, we're flying.
    private void ComputeNormal()
    {
        this.grounded = false;
        this.normal = Vector3.up;

        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, -gameObject.transform.up, out hit))
        {
            if (hit.distance <= (playerHeight / 2f) + groundDelta)
            {
                // If we should respect floor tags, then check for that first
                // Else, any collider is valid.
                if (!RespectFloorTags || (RespectFloorTags && hit.collider.gameObject.tag.Equals("Floor")))
                {
                    this.grounded = true;
                    this.normal = hit.normal;
                }
            }
        }
    }

    // TODO make sure the collision is with something tagged as floor
    // Right now, if the player collides with something mid-air they can
    // jump immediately afterwards.
    // 
    // At the very least, add a minimum duration before jumping twice.
    private void OnCollisionEnter(Collision collision)
    {
        // Recompute the normal of what we're standing on
        ComputeNormal();

        // Force the grounded property to be true, and reset our jump status
        //grounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        // Recompute the normal of what we're standing on
        ComputeNormal();

        // We know we're not on the ground
        //grounded = false;
    }

    // Updated once per physics step.
    private void FixedUpdate()
    {
        // Up/down motion (flying or jump/crouch)
        float jump = Input.GetAxis("Jump");

        // Update cooldown; update "jumping" to reflect if we're off cooldown or not.
        if (jumpCooldown > 0)
        {
            jumpCooldown--;
        }
        bool jumping = jumpCooldown > 0;

        // If we have a jump request, and we're on the ground, and we're not
        // already jumping, then make the character jump up.
        if (jump > 0 && grounded && !jumping)
        {
            body.AddForce(Vector3.up * Mathf.Sqrt(2f * 9.8f * jumpHeight) * body.mass, ForceMode.Impulse);

            // Add 0.5 seconds to the cooldown so we avoid double jumping
            jumpCooldown = (int)(0.5f / Time.fixedDeltaTime);
        }

        // We compute this value in Update (user input is not frame dependent), but apply it here
        // (physics only updates on fixed update).
        body.MovePosition(transform.position + (Time.fixedDeltaTime * movementVector));
    }
}