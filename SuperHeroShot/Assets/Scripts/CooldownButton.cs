using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CooldownButton : MonoBehaviour
{
    [Tooltip("Button to control during cooldown (auto-assigned to this GameObject's Button)")]
    public Button button;

    [Tooltip("Image used as the fill indicator. Should be Image.Type = Filled")]
    public Image fillImage;

    [Tooltip("Optional TextMeshPro label to show remaining seconds")]
    public TMP_Text cooldownText;

    [Tooltip("Cooldown duration in seconds")]
    public float cooldownDuration = 5f;

    [Tooltip("If true, clicking the button will automatically start the cooldown")]
    public bool startOnClick = true;

    Coroutine running;
    float remaining;

    void Reset()
    {
        button = GetComponent<Button>();
    }

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null && startOnClick) button.onClick.AddListener(OnButtonClicked);
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    void OnDestroy()
    {
        if (button != null && startOnClick) button.onClick.RemoveListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        if (startOnClick) StartCooldown();
    }

    public void StartCooldown()
    {
        if (cooldownDuration <= 0f) return;
        if (running != null) return;
        running = StartCoroutine(CooldownRoutine());
    }

    public void ResetCooldown()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
        if (fillImage != null) fillImage.fillAmount = 0f;
        if (cooldownText != null) cooldownText.text = string.Empty;
        if (button != null) button.interactable = true;
    }

    IEnumerator CooldownRoutine()
    {
        remaining = cooldownDuration;
        if (button != null) button.interactable = false;

        while (remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            float clamped = Mathf.Clamp01(remaining / cooldownDuration); // 1 -> 0
            if (fillImage != null) fillImage.fillAmount = clamped;
            if (cooldownText != null) cooldownText.text = Mathf.CeilToInt(Mathf.Max(0f, remaining)).ToString();
            yield return null;
        }

        if (fillImage != null) fillImage.fillAmount = 0f;
        if (cooldownText != null) cooldownText.text = string.Empty;
        if (button != null) button.interactable = true;
        running = null;
    }
}
