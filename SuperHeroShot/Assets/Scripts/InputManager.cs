using UnityEngine;

public class InputManager : MonoBehaviour
{
    public VirtualJoystick moveJoystick;
    public static InputManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if(Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            useMouseLook = true;
        }
        else
        {
            useMouseLook = false;
        }
    }

    [SerializeField] private Vector2 _moveInput;
    [SerializeField] private Vector2 _lookInput;
    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 1f;
    [SerializeField] private float touchSensitivity = 0.02f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private bool useMouseLook = true;
    private int _activeTouchId = -1;

    public float LookSensitivity => lookSensitivity;


    void Update()
    {
        HandleMoveInput();
        HandleLookInput();
    }

    private void HandleMoveInput()
    {
        Vector2 kb = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) kb.y += 1f;
        if (Input.GetKey(KeyCode.S)) kb.y -= 1f;
        if (Input.GetKey(KeyCode.D)) kb.x += 1f;
        if (Input.GetKey(KeyCode.A)) kb.x -= 1f;

        if (kb.sqrMagnitude > 0.001f)
        {
            _moveInput = kb.normalized;
            return;
        }

        if (moveJoystick != null)
        {
            _moveInput = moveJoystick.Direction;
        }
        else
        {
            _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
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

    public Vector2 MoveInput => _moveInput;
    public Vector2 LookInput => _lookInput;
}
