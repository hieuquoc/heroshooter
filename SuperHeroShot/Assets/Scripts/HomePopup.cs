using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomePopup : BasePopup
{
    [Tooltip("Text component to display the current level")]
    public TMP_Text levelText;

    [Tooltip("PlayerPrefs key to read the current level from")]
    public string levelKey = "Level";

    [Tooltip("Name to register this popup with UIManager")]
    public string popupName = PopupNames.Home;

    public Button buttonPlay;
    public Button buttonSettings;
    public Button freeSkillAds;
    public Button invulnerabilityAds;
    public GameObject freeSkillAdsIcon;
    public GameObject invulnerabilityAdsIcon;
    public GameObject UICamera;

    protected override void Awake()
    {
        base.Awake();
        if (UIManager.Instance != null)
            UIManager.Instance.Register(popupName, this);
        buttonPlay.onClick.AddListener(OnPlayButton);
        buttonSettings.onClick.AddListener(OnSettingsButton);
        freeSkillAds.onClick.AddListener(OnFreeSkillAdsButton);
        invulnerabilityAds.onClick.AddListener(OnInvulnerabilityAdsButton);
    }
    public override void Open()
    {
        base.Open();
        UpdateLevelText();
        freeSkillAdsIcon.SetActive(!PlayerData.IsFreeFire);
        invulnerabilityAdsIcon.SetActive(!PlayerData.IsInvincible);
        UICamera.SetActive(true);
    }

    void UpdateLevelText()
    {
        if (levelText == null) return;
        int level = PlayerPrefs.GetInt(levelKey, 1);
        levelText.text = $"LEVEL: {level}";
    }

    void OnDestroy()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.Unregister(popupName);
    }

    public void OnPlayButton()
    {
        GameManager.Instance.StartGame();
    }

    public void OnSettingsButton()
    {
        UIManager.Instance.Open("SettingPopup");
    }

    public void OnFreeSkillAdsButton()
    {
        PlayerData.IsFreeFire = true;
        if (freeSkillAdsIcon != null)
            freeSkillAdsIcon.SetActive(false);

    }
    
    public void OnInvulnerabilityAdsButton()
    {
        PlayerData.IsInvincible = true;
        if (invulnerabilityAdsIcon != null)
            invulnerabilityAdsIcon.SetActive(false);
    }

    public void OnCloseButton()
    {
        Close();
        UICamera.SetActive(false);
    }
}
