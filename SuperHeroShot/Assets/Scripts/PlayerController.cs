using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerState currentState = PlayerState.Idle;
    public PlayerState CurrentState => currentState;
    public Vector3 movementInput;
    public InputManager inputManager;
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.5f;   
    public Vector3 _dashDirection;
    public Vector3 cameraForward;
    [SerializeField] private Rigidbody rigidbody;
    private Vector3 _sprintDirection;
    private float _dashTimer;
    private Vector3 _moveDirection;
    [SerializeField] private bool _isDashing;
    [SerializeField] private bool _isSprinting;
    [SerializeField] private float lookRotationSpeed = 200f;
    public CameraFollow cameraFollow;

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        cameraFollow = GameManager.Camera;
    }

    void Update()
    {
        // read movement input from InputManager (player only accepts input from it)
        movementInput = InputManager.Instance.MoveInput;       
        
        UpdateState(movementInput, _isSprinting, _isDashing);

        switch (currentState)
        {
            case PlayerState.Idle:
                Stopping();
                UpdateLook();
                break;
            case PlayerState.Move:
                MoveNormal();
                UpdateLook();
                break;
            case PlayerState.Sprint:
                MoveSprint();
                UpdateLook();
                break;
            case PlayerState.Dash:
                break;
        }
    }

    // Move the player on the X,Z plane using Rigidbody based on movementInput (Vector2)
    private void MoveNormal()
    {
        Vector3 input = inputManager.MoveInput;
        if (input.sqrMagnitude < 0.0001f)
            return;

        // Flatten camera forward onto XZ plane before use
        Vector3 forward = GameManager.Camera.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        // XZ: driven by input.x (strafe) and input.z (forward/back)
        Vector3 desired = forward * input.z + right * input.x;
        if (desired.sqrMagnitude > 0.0001f)
            desired.Normalize();

        Vector3 targetVel = desired * moveSpeed;

        // Y: use input.y if provided (e.g. fly/jump axis), otherwise preserve rigidbody gravity
        targetVel.y = Mathf.Abs(input.y) > 0.0001f ? input.y * moveSpeed : rigidbody.velocity.y;

        rigidbody.velocity = targetVel;
    }

    public void UpdateState(Vector3 movementInput, bool isSprinting, bool isDashing)
    {
        if(!isSprinting && !isDashing)
        {
            if (movementInput.sqrMagnitude < 0.0001f)
            {
                currentState = PlayerState.Idle;
            }
            else
            {
                currentState = PlayerState.Move;
            }
        }
        else if (isSprinting)
        {
            currentState = PlayerState.Sprint;
        }
        else if (isDashing)
        {
            currentState = PlayerState.Dash;
        }
    }

    public void Stopping()
    {
        if (currentState != PlayerState.Idle || rigidbody.velocity.sqrMagnitude < 0.0001f)
            return;
        rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, Vector3.zero, Time.deltaTime * 5f);
    }

    // Rotate player yaw (Y) to follow camera yaw
    private void UpdateLook()
    {
        if (cameraFollow == null)
            return;
        Quaternion targetRot = Quaternion.Euler(0f, cameraFollow.Yaw, 0f);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            lookRotationSpeed * Time.deltaTime
        );
    }

    private void MoveSprint()
    {
        if (cameraFollow == null) return;

        // Lock sprint direction to camera forward (flat)
        Vector3 forward = cameraFollow.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        // Snap player to face sprint direction immediately
        if (forward.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(forward);

        Vector3 targetVel = cameraFollow.transform.forward.normalized * sprintSpeed;
        rigidbody.velocity = targetVel;
    }

    public void SetSprinting(bool sprinting)
    {
        _isSprinting = sprinting;
    }
}

public enum PlayerState
{
    Idle,
    Move,
    Sprint,
    Dash,
}
