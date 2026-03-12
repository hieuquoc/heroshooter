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
        switch (currentState)
        {
            case PlayerState.Idle:
                
                break;
            case PlayerState.Move:
                
                break;
            case PlayerState.Sprint:
                
                break;
            case PlayerState.Dash:
                
                break;
        }
    }


}

public enum PlayerState
{
    Idle,
    Move,
    Sprint,
    Dash,
}
