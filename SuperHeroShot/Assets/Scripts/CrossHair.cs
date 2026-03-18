using UnityEngine;
using UnityEngine.UI;

namespace rescueforce
{
    public class CrossHair : MonoBehaviour
{
    public static CrossHair Instance { get; private set; }

    [Header("Crosshair Parts")]
    [Tooltip("Top arm RectTransform (child Image)")]
    [SerializeField] private RectTransform top;
    [Tooltip("Bottom arm RectTransform (child Image)")]
    [SerializeField] private RectTransform bottom;
    [Tooltip("Left arm RectTransform (child Image)")]
    [SerializeField] private RectTransform left;
    [Tooltip("Right arm RectTransform (child Image)")]
    [SerializeField] private RectTransform right;

    [Header("Spread Settings")]
    [Tooltip("Resting distance (pixels) from centre to each arm")]
    [SerializeField] private float baseSpread = 30f;
    [Tooltip("Maximum allowed spread")]
    [SerializeField] private float maxSpread = 160f;
    [Tooltip("Spread added per pistol shot")]
    [SerializeField] private float pistolRecoil = 22f;
    [Tooltip("Spread added per second while laser is firing")]
    [SerializeField] private float laserRecoilPerSec = 60f;
    [Tooltip("Spread added when rocket is launched")]
    [SerializeField] private float rocketRecoil = 90f;
    [Tooltip("How fast the spread recovers back to base (higher = faster)")]
    [SerializeField] private float recoverySpeed = 7f;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color firingColor = new Color(1f, 0.35f, 0f, 1f);
    [SerializeField] private float colorLerpSpeed = 10f;

    private float _currentSpread;
    private bool _laserFiringThisFrame;
    private Image[] _images;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _currentSpread = baseSpread;
        _images = GetComponentsInChildren<Image>(true);
        UpdatePartsPosition();
    }

    private void Update()
    {
        // Continuous laser recoil: Weapon sets _laserFiringThisFrame each frame it fires
        if (_laserFiringThisFrame)
            _currentSpread = Mathf.Min(_currentSpread + laserRecoilPerSec * Time.deltaTime, maxSpread);

        _laserFiringThisFrame = false; // reset; Weapon.UpdateRayAim will re-set next frame if still firing

        // Recover spread toward baseSpread
        _currentSpread = Mathf.Lerp(_currentSpread, baseSpread, recoverySpeed * Time.deltaTime);

        // Never go below base
        if (_currentSpread < baseSpread) _currentSpread = baseSpread;

        UpdatePartsPosition();
        UpdateColor();
    }

    // ── Static notify methods (called from Weapon.cs) ─────────────────────

    /// <summary>Triggered once per pistol shot.</summary>
    public static void NotifyPistolShot()
    {
        if (Instance == null) return;
        Instance._currentSpread = Mathf.Min(Instance._currentSpread + Instance.pistolRecoil, Instance.maxSpread);
    }

    /// <summary>Called every frame while the laser beam is active.</summary>
    public static void NotifyLaserFiring()
    {
        if (Instance == null) return;
        Instance._laserFiringThisFrame = true;
    }

    /// <summary>Triggered once when a rocket volley is launched.</summary>
    public static void NotifyRocketFired()
    {
        if (Instance == null) return;
        Instance._currentSpread = Mathf.Min(Instance._currentSpread + Instance.rocketRecoil, Instance.maxSpread);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private void UpdatePartsPosition()
    {
        if (top)    top.anchoredPosition    = new Vector2(0f,  _currentSpread);
        if (bottom) bottom.anchoredPosition = new Vector2(0f, -_currentSpread);
        if (left)   left.anchoredPosition   = new Vector2(-_currentSpread, 0f);
        if (right)  right.anchoredPosition  = new Vector2( _currentSpread, 0f);
    }

    private void UpdateColor()
    {
        if (_images == null || _images.Length == 0) return;
        bool isFiring = _currentSpread > baseSpread + 4f;
        Color target = isFiring ? firingColor : normalColor;
        Color next = Color.Lerp(_images[0].color, target, colorLerpSpeed * Time.deltaTime);
        foreach (var img in _images)
            img.color = next;
    }
}

}

