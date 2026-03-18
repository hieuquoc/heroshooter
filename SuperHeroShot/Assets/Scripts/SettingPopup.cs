using UnityEngine;
using UnityEngine.UI;

namespace rescueforce
{
    public class SettingPopup : BasePopup
{
    [Header("UI References")]
    public Slider cameraSensitivitySlider;
    public Toggle soundToggle;
    public Toggle musicToggle;

    [Header("PlayerPrefs Keys")]
    public string cameraKey = "CameraSensitivity";
    public string soundKey = "SoundEnabled";
    public string musicKey = "MusicEnabled";

    [Tooltip("Name used to register with UIManager")]
    public string popupName = PopupNames.Setting;

    protected override void Awake()
    {
        base.Awake();
        if (UIManager.Instance != null)
            UIManager.Instance.Register(popupName, this);
    }

    public override void Open()
    {
        base.Open();
        LoadSettingsToUI();
    }

    void LoadSettingsToUI()
    {
        if (cameraSensitivitySlider != null && InputManager.Instance != null)
        {
            cameraSensitivitySlider.minValue = InputManager.MinTouchSensitivity;
            cameraSensitivitySlider.maxValue = InputManager.MaxTouchSensitivity;
            cameraSensitivitySlider.value = PlayerPrefs.GetFloat(cameraKey, InputManager.Instance.TouchSensitivity);
        }
        if (soundToggle != null)
            soundToggle.isOn = PlayerPrefs.GetInt(soundKey, 1) == 1;
        if (musicToggle != null)
            musicToggle.isOn = PlayerPrefs.GetInt(musicKey, 1) == 1;
    }

    public void OnCameraSensitivityChanged(float value)
    {
        // 'value' is in the same range as InputManager.MinTouchSensitivity..MaxTouchSensitivity
        PlayerPrefs.SetFloat(cameraKey, value);
        PlayerPrefs.Save();
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetTouchSensitivity(value);
        }
    }

    public void OnSoundToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt(soundKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnMusicToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt(musicKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.Unregister(popupName);
    }
}

}

