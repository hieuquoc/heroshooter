using UnityEngine;
using UnityEngine.UI;

public class RocketAimUI : MonoBehaviour
{
    public static RocketAimUI Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }  // 3 marker sprite transforms

    [Header("Fill Bars (UI Image - Filled)")]
    public Image[] fillBars;             // 3 Image components, ImageType = Filled

    [Header("Aim Duration per Slot")]
    public float aimDuration = 3f;       // mỗi slot tồn tại bao lâu (giây)

    // runtime state — mỗi slot lưu time còn lại
    [SerializeField] private float[] _timers = new float[3];
    private int _currentShotIndex = 0;   // slot nào sẽ được bắn tiếp theo (0-2)

    void Start()
    {
        ResetAllBars();
        gameObject.SetActive(false);
    }


    /// <summary>Kích hoạt slot aim thứ index (0-2), hiển thị.</summary>
    public void ActivateSlot(int index, float initialFill = 1f)
    {
        if (index < 0 || index >= 3) return;
        Debug.Log($"Activating slot {index} with initial fill {initialFill}");
        _timers[index] = aimDuration;
        SetSlotVisible(index, true);
        SetFill(index, initialFill);
    }

    /// <summary>Tắt thủ công một slot (ví dụ khi rocket đã phóng).</summary>
    public void DeactivateSlot(int index)
    {
        if (index < 0 || index >= 3) return;
        _timers[index] = 0f;
        SetSlotVisible(index, false);
    }

    /// <summary>Launcher gọi để drive fill bar trực tiếp (0-1).</summary>
    public void SetSlotFill(int index, float value)
    {
        if (index < 0 || index >= 3) return;
        SetFill(index, value);
    }

    // ── Helpers ────────────────────────────────────────────

    void SetSlotVisible(int i, bool visible)
    {
        if (fillBars != null && i < fillBars.Length && fillBars[i] != null)
            fillBars[i].gameObject.SetActive(visible);
    }

    void SetFill(int i, float value)
    {
        if (fillBars != null && i < fillBars.Length && fillBars[i] != null)
            fillBars[i].fillAmount = Mathf.Clamp01(value);
    }

    void ResetAllBars()
    {
        for (int i = 0; i < 3; i++)
            SetSlotVisible(i, false);
    }
}
