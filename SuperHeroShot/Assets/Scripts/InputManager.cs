using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace rescueforce
{
    public class InputManager : MonoBehaviour
{
    public VirtualJoystick moveJoystick;
    public static InputManager Instance { get; private set; }
    private PlayerController _player;

    public Button sprintButton;
    public Button flightUpButton;
    public CooldownButton laserButton;
    public CooldownButton rocketButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _player = GameManager.Player;
        // Use HoldButton component for pointer down/up (shorter and reusable)
        if (sprintButton != null)
        {
            var hb = sprintButton.gameObject.GetComponent<HoldButton>() ?? sprintButton.gameObject.AddComponent<HoldButton>();
            hb.onDown.AddListener(() => { 
                if (_player != null) _sprintButtonHeld = true; 
                Debug.Log("Sprint button down, player sprinting: " + (_player != null ? _player.CurrentState.ToString() : "null player"));
                });
            hb.onUp.AddListener(() => { if (_player != null) _sprintButtonHeld = false; });
        }

        if (flightUpButton != null)
        {
            var hb2 = flightUpButton.gameObject.GetComponent<HoldButton>() ?? flightUpButton.gameObject.AddComponent<HoldButton>();
            hb2.onDown.AddListener(() => { _flightUpButtonHeld = true; });
            hb2.onUp.AddListener(() => { _flightUpButtonHeld = false; });
        }
        laserButton.button.onClick.AddListener(() => _player.ShootLaser());
        rocketButton.button.onClick.AddListener(() => _player.ShootRocket());
        laserButton.cooldownDuration = _player.LaserCooldown;
        rocketButton.cooldownDuration = _player.RocketCooldown;
    }

    [SerializeField] private Vector3 _moveInput;
    [SerializeField] private Vector2 _lookInput;
    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 1f;
    [SerializeField] private float touchSensitivity = 0.02f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private bool useMouseLook = true;
    private bool _canDash = true;
    private int _activeTouchId = -1;

    public float LookSensitivity => lookSensitivity;
    public float TouchSensitivity => touchSensitivity;
    public const float MinTouchSensitivity = 0.01f;
    public const float MaxTouchSensitivity = 0.1f;

    [SerializeField] private bool _isFlyingUp;
    private bool _navButtonPressed;
    private bool _sprintButtonHeld;
    private bool _flightUpButtonHeld;


    void Update()
    {
        if(GameManager.Instance.GameState != GameState.Playing) return;
        HandleMoveInput();
        HandleLookInput();
    }

    private void HandleMoveInput()
    {
        _isFlyingUp = false;
        _navButtonPressed = false;
        Vector3 kb = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) kb.z += 1f;
        if (Input.GetKey(KeyCode.S)) kb.z -= 1f;
        if (Input.GetKey(KeyCode.D))
        {
            _navButtonPressed = true;
            kb.x += 1f;
        } 
        if (Input.GetKey(KeyCode.A))
        {
            _navButtonPressed = true;
            kb.x -= 1f;
        }

        if (kb.sqrMagnitude > 0.001f)
        {
            _moveInput = kb.normalized;
        }else if (moveJoystick != null)
        {
            _moveInput = new Vector3(moveJoystick.Horizontal, _moveInput.y, moveJoystick.Vertical);
            if(Math.Abs(_moveInput.x) > 0.01f)
            {
                _navButtonPressed = true;
            }
        }
        else
        {
            _moveInput = Vector3.zero;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            _moveInput.y = 1f;
        } 

        _player.SetSprinting(_sprintButtonHeld || Input.GetKey(KeyCode.LeftShift));
        if(_player.CurrentState == PlayerState.Sprint && !_player.IsDashing)
        {
            if(_navButtonPressed && _canDash)
            {
                _player.SetDashing(true , (int)kb.x);
                _canDash = false;
            }
        }
        
        if(!_navButtonPressed)
        {
            _canDash = true;
        }
        _navButtonPressed = false;

        if (Input.GetKey(KeyCode.Mouse1))
        {
            _player.ShootLaser();
            if (laserButton != null) laserButton.StartCooldown();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            _player.ShootRocket();
            if (rocketButton != null) rocketButton.StartCooldown();
        }

        HandleFlyingUp();
    }
    private void HandleFlyingUp()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            _isFlyingUp = true;
        }

        if(_isFlyingUp || _flightUpButtonHeld)
        {
            _moveInput.y = 1f;
        }else
        {
            _moveInput.y = 0f;
        }
    }

    public void SetTouchSensitivity(float value)
    {
        touchSensitivity = value;
    }

    private void HandleLookInput()
    {
        Vector2 look = Vector2.zero;

        // Mouse look: use mouse delta only while right mouse button is held (when enabled)
        if (useMouseLook)
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            look = new Vector2(mx, my) * lookSensitivity;
        }else
        // Mobile / touch mode: single-touch drag to look (only when touch mode selected)
        if (!useMouseLook)
        {
            if (Input.touchCount > 0)
            {
                Touch t;
                if (_activeTouchId >= 0)
                {
                    bool found = false;
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        if (Input.touches[i].fingerId == _activeTouchId)
                        {
                            t = Input.touches[i];
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        _activeTouchId = -1;
                    }
                }

                if (_activeTouchId < 0)
                {
                    t = Input.touches[0];
                    if (t.phase == TouchPhase.Began)
                    {
                        _activeTouchId = t.fingerId;
                    }
                }

                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.touches[i].fingerId == _activeTouchId)
                    {
                        t = Input.touches[i];
                        if (t.phase == TouchPhase.Moved)
                        {
                            float dx = t.deltaPosition.x / Screen.width;
                            float dy = t.deltaPosition.y / Screen.height;
                            look = new Vector2(dx, dy) / touchSensitivity;
                        }
                        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                        {
                            _activeTouchId = -1;
                        }
                        break;
                    }
                }
            }
        }

        if (invertY) look.y = -look.y;
        _lookInput = look;
    }

    public Vector3 MoveInput => _moveInput;
    public Vector2 LookInput => _lookInput;

    public void SetSkillButtonInteractable(bool canUseLaser, bool canUseRocket)
    {
        if (laserButton != null) laserButton.SetInteractable(canUseLaser);
        if (rocketButton != null) rocketButton.SetInteractable(canUseRocket);
    }

    public void Reset()
    {
        _moveInput = Vector3.zero;
        _lookInput = Vector2.zero;
        _sprintButtonHeld = false;
        _flightUpButtonHeld = false;
        laserButton.ResetCooldown();
        rocketButton.ResetCooldown();
    }
}

}

