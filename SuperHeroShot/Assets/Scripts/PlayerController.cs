using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Idle;
    public Vector2 movementInput;
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

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // read movement input from InputManager (player only accepts input from it)
        if (inputManager != null)
            movementInput = inputManager.MoveInput;
        else
            movementInput = Vector2.zero;
        
        UpdateState(movementInput, _isSprinting, _isDashing);

        switch (currentState)
        {
            case PlayerState.Idle:
                Stopping();
                break;
            case PlayerState.Move:
                MoveNormal();
                break;
            case PlayerState.Sprint:
                break;
            case PlayerState.Dash:
                break;
        }
    }

    // Move the player on the X,Z plane using Rigidbody based on movementInput (Vector2)
    private void MoveNormal()
    {
        // Build local input vector (x = strafe, z = forward)
        Debug.Log($"Move Input: {movementInput}");
        Vector3 input = new Vector3(movementInput.x, 0f, movementInput.y);
        if (input.sqrMagnitude < 0.0001f)
            return;

        // Use the player's transform forward as the forward direction
        Vector3 forward = transform.forward;

        forward.y = 0f;
        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        Vector3 desired = (forward * input.z + right * input.x);
        if (desired.sqrMagnitude > 0.0001f)
            desired.Normalize();

        Vector3 targetVel = desired * moveSpeed;

        // Preserve existing vertical velocity (gravity, jump, etc.)
        targetVel.y = rigidbody.velocity.y;

        rigidbody.velocity = targetVel;
    }

    public void UpdateState(Vector2 movementInput, bool isSprinting, bool isDashing)
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
        if (currentState == PlayerState.Idle && rigidbody.velocity.sqrMagnitude < 0.01f)
            return;
        currentState = PlayerState.Idle;
        rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, Vector3.zero, Time.deltaTime * 5f);
    }


}

public enum PlayerState
{
    Idle,
    Move,
    Sprint,
    Dash,
}
