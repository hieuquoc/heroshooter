using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace rescueforce
{
    public class WinPopup : BasePopup
{
    [SerializeField] Button exitButton;

    protected override void Awake()
    {

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => GameManager.Instance.BackToHome());
            exitButton.onClick.AddListener(Close);
        }
    }
}
}


