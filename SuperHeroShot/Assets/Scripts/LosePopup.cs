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


    protected override void Awake()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(() => {
                GameManager.Instance.StartGame(); 
            });
            retryButton.onClick.AddListener(Close);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() => GameManager.Instance.BackToHome());
            exitButton.onClick.AddListener(Close);
        }
        base.Awake();
    }
}
