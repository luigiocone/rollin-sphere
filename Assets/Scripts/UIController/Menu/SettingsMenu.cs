using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : Menu
{
    [Header("Sphere trail")]
    [SerializeField] Button SphereTrailButton;
    [SerializeField] TMP_Text SphereTrailValueText;

    [Header("Camera sensitivity")]
    [SerializeField] GameObject CameraSensitivityRoot;
    [SerializeField] float CameraSliderValueDivisor = 50f;

    [Header("Volume")]
    [SerializeField] GameObject VolumeRoot;
    [SerializeField] GameObject MusicVolumeRoot;
    [SerializeField] float VolumeSliderValueDivisor = 100f;

    [Header("Screen size")]
    [SerializeField] TMP_Dropdown ScreenSizeDropdown;

    Slider m_CameraSlider;
    TMP_Text m_CameraSliderText;

    Slider m_VolumeSlider;
    TMP_Text m_VolumeSliderText;

    Slider m_MusicVolumeSlider;
    TMP_Text m_MusicVolumeSliderText;


    protected override void Awake()
    {
        base.Awake();

        SphereTrailButton.onClick.AddListener(OnToggleSphereTrail);
        SetSphereTrailText();

        // Camera settings
        m_CameraSlider = CameraSensitivityRoot.GetComponentInChildren<Slider>();
        m_CameraSliderText = CameraSensitivityRoot.GetComponentInChildren<TMP_Text>();
        m_CameraSlider.onValueChanged.AddListener(OnCameraSensitivityChange);
        m_CameraSlider.value = GameSettings.CameraSensitivity * CameraSliderValueDivisor;
        OnCameraSensitivityChange(m_CameraSlider.value);

        // Volume settings 
        m_VolumeSlider = VolumeRoot.GetComponentInChildren<Slider>();
        m_VolumeSliderText = VolumeRoot.GetComponentInChildren<TMP_Text>();
        m_VolumeSlider.onValueChanged.AddListener(OnVolumeChange);
        m_VolumeSlider.value = GameSettings.GlobalVolume * VolumeSliderValueDivisor;
        OnVolumeChange(m_VolumeSlider.value);

        bool musicOn = FindObjectOfType<AudioManager>().IsMusicOn;
        MusicVolumeRoot.SetActive(musicOn);
        if (musicOn)
        {
            m_MusicVolumeSlider = MusicVolumeRoot.GetComponentInChildren<Slider>();
            m_MusicVolumeSliderText = MusicVolumeRoot.GetComponentInChildren<TMP_Text>();
            m_MusicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChange);
            m_MusicVolumeSlider.value = GameSettings.MusicVolume * VolumeSliderValueDivisor;
            OnMusicVolumeChange(m_MusicVolumeSlider.value);
        }

        (int width, int height) = GameSettings.GetScreenSize();
        AddScreenResolution(width, height);
        ScreenSizeDropdown.onValueChanged.AddListener(OnScreenSizeChange);
    }

    public void AddScreenResolution(int width, int height)
    {
        // Add current screen resolution to dropdown menu if not already present
        string currResolution = width + "x" + height;
        int index;
        for (index = 0; index < ScreenSizeDropdown.options.Count; index++) 
        {
            string res = ScreenSizeDropdown.options[index].text;
            if (res == currResolution)
            {
                ScreenSizeDropdown.value = index;
                return;
            }
        }
        
        ScreenSizeDropdown.AddOptions(new List<string>(){currResolution});
        ScreenSizeDropdown.value = index;
    }

    void OnToggleSphereTrail()
    {
        GameSettings.SphereTrail = !GameSettings.SphereTrail;
        SetSphereTrailText();
    }

    void SetSphereTrailText()
    { 
        string text = GameSettings.SphereTrail
            ? "<color=#8ADAB2>Yes</color>"
            : "<color=\"red\">No</color>";
        SphereTrailValueText.text = text;
    }

    void OnScreenSizeChange(int index)
    {
        var option = ScreenSizeDropdown.options[index];
        string[] text = option.text.Split('x');
        int width = int.Parse(text[0]);
        int height = int.Parse(text[1]);
        GameSettings.SetScreenSize(width, height);
    }

    void OnCameraSensitivityChange(float value)
    { 
        const string prefix = "Camera sensitivity: ";
        m_CameraSliderText.text = prefix + Mathf.RoundToInt(value);
        GameSettings.CameraSensitivity = value / CameraSliderValueDivisor;
    }

    void OnVolumeChange(float value)
    { 
        const string prefix = "Volume: ";
        m_VolumeSliderText.text = prefix + Mathf.RoundToInt(value);
        GameSettings.GlobalVolume = value / VolumeSliderValueDivisor;
    }

    void OnMusicVolumeChange(float value)
    { 
        const string prefix = "Music volume: ";
        m_MusicVolumeSliderText.text = prefix + Mathf.RoundToInt(value);
        GameSettings.MusicVolume = value / VolumeSliderValueDivisor;
    }

}
