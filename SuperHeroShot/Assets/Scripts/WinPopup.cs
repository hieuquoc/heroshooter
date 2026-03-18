using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class WinPopup : BasePopup
{
    [Header("UI")]
    [SerializeField] Text titleText;
    [SerializeField] Text messageText;
    [SerializeField] Button confirmButton;
    [SerializeField] UnityEvent onConfirm;

    void Reset()
    {
        // Try to auto-assign common children when added in Editor
        titleText = transform.Find("Title")?.GetComponent<Text>();
        messageText = transform.Find("Message")?.GetComponent<Text>();
        confirmButton = transform.Find("ConfirmButton")?.GetComponent<Button>();
    }

    void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => onConfirm?.Invoke());
            confirmButton.onClick.AddListener(Close);
        }
    }

    public void Setup(string title, string message, UnityAction onConfirmCallback = null)
    {
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        onConfirm.RemoveAllListeners();
        if (onConfirmCallback != null) onConfirm.AddListener(onConfirmCallback);

        Open();
    }
}
