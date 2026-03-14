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
    public float dashCooldown = 1f;
    public Vector3 cameraForward;
    [SerializeField] private Rigidbody rigidbody;
    private Vector3 _sprintDirection;
    [SerializeField] private float _dashTimer;
    private Vector3 _moveDirection;
    [SerializeField] private bool _isDashing;
    [SerializeField] private bool _isSprinting;
    [SerializeField] private float lookRotationSpeed = 200f;
    private int _dashDirectionInput;
    private float _dashCooldownTimer;
    private Vector3 _dashVel;
    public bool IsDashing => _isDashing;
    public bool CanDash => _dashCooldownTimer <= 0f;
    [SerializeField] private AnimatorManager animatorManager;
    public AnimatorManager AnimatorManager => animatorManager;
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
        PlayerState newState = currentState;
        if(!isSprinting && !isDashing)
        {
            if (movementInput.sqrMagnitude < 0.0001f)
            {
                newState = PlayerState.Idle;
            }
            else
            {
                newState = PlayerState.Move;
            }
        }
        else if (isSprinting)
        {
            newState = PlayerState.Sprint;
        }
        if(newState != currentState)
        {
            ChangeState(newState);
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
        forward.Normalize();

        // Camera-relative right — always perpendicular to forward,
        // so dash velocity won't cancel any of the forward component.
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        // Snap player to face sprint direction immediately
        if (forward.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(forward);
        _dashVel = Vector3.zero;        
        if(_isDashing && _dashTimer > 0f)
        {
            _dashTimer -= Time.deltaTime;
            _dashVel = right * _dashDirectionInput * dashSpeed;
        }else
        {
            _isDashing = false;
        }      
        if(_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }

        Vector3 targetVel = forward * sprintSpeed + _dashVel;
        rigidbody.velocity = targetVel;
    }


    public void SetSprinting(bool sprinting)
    {
        _isSprinting = sprinting;
    }

    public void SetDashing(bool dashing, int directionInput = 0)
    {
        _isDashing = dashing;
        _dashDirectionInput = directionInput;
        _dashTimer = dashDuration;
        animatorManager.SetTriggerDash(_dashDirectionInput);
        _dashCooldownTimer = dashCooldown;
    }

    public void ChangeState(PlayerState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case PlayerState.Idle:
            animatorManager.SetTriggerIdle();
                break;
            case PlayerState.Move:
            animatorManager.SetTriggerMove();
                break;
            case PlayerState.Sprint:
            animatorManager.SetTriggerSprint();
                break;
        }
    }
}

public enum PlayerState
{
    Idle = 0,
    Move = 1,
    Sprint = 2,
}
