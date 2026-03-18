using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LosePopup : BasePopup
{
    [Header("UI")]
    [SerializeField] Text titleText;
    [SerializeField] Text messageText;
    [SerializeField] Button retryButton;
    [SerializeField] Button exitButton;
    [SerializeField] UnityEvent onRetry;
    [SerializeField] UnityEvent onExit;

    void Reset()
    {
        titleText = transform.Find("Title")?.GetComponent<Text>();
        messageText = transform.Find("Message")?.GetComponent<Text>();
        retryButton = transform.Find("RetryButton")?.GetComponent<Button>();
        exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
    }

    void Awake()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(() => onRetry?.Invoke());
            retryButton.onClick.AddListener(Close);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => onExit?.Invoke());
            exitButton.onClick.AddListener(Close);
        }
    }

    public void Setup(string title, string message, UnityAction retryCallback = null, UnityAction exitCallback = null)
    {
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        onRetry.RemoveAllListeners();
        if (retryCallback != null) onRetry.AddListener(retryCallback);

        onExit.RemoveAllListeners();
        if (exitCallback != null) onExit.AddListener(exitCallback);

        Open();
    }
}
