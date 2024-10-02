using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public SettingsData settingsData;

    public Image onImage;
    public Image offImage;
    public Text onText;
    public Text offText;

    public Image musicVolumeBar;
    public Image soundEffectsVolumeBar;
    public Image musicImage;
    public Image musicImage2;

    private float maxVolume = 1f;

    private void Start()
    {
        settingsData.LoadSettings();
        UpdateVibrationUI(); 
        UpdateSoundUI();
    }
    private void Update()
    {
        if(musicVolumeBar.fillAmount ==  0)
        {
            musicImage.enabled = false;
            musicImage2.enabled = true;
        }
        else
        {
            musicImage.enabled=true;
            musicImage2.enabled=false;
        }
    }
    public void ToggleVibration()
    {
        settingsData.SetVibration(!settingsData.isVibrationOn); 
        UpdateVibrationUI();
    }

    private void UpdateVibrationUI()
    {
        if (settingsData.isVibrationOn)
        {
            onImage.gameObject.SetActive(true);
            onText.gameObject.SetActive(true);
            offImage.gameObject.SetActive(false);
            offText.gameObject.SetActive(false);
        }
        else
        {
            onImage.gameObject.SetActive(false);
            onText.gameObject.SetActive(false);
            offImage.gameObject.SetActive(true);
            offText.gameObject.SetActive(true);
        }
    }
    public void OnMusicBarTouch(BaseEventData eventData)
    {
        PointerEventData pointerData = eventData as PointerEventData;
        Vector2 localPoint;
        RectTransform rt = musicVolumeBar.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerData.position, null, out localPoint))
        {
            float normalizedValue = Mathf.InverseLerp(rt.rect.xMin, rt.rect.xMax, localPoint.x);
            SetMusicVolume(normalizedValue);
        }
    }
    public void OnEffectsBarTouch(BaseEventData eventData)
    {
        PointerEventData pointerData = eventData as PointerEventData;
        Vector2 localPoint;
        RectTransform rt = soundEffectsVolumeBar.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerData.position, null, out localPoint))
        {

            float normalizedValue = Mathf.InverseLerp(rt.rect.xMin, rt.rect.xMax, localPoint.x);
            SetSoundEffectsVolume(normalizedValue);
        }
    }
    public void SetMusicVolume(float volume)
    {
        settingsData.SetMusicVolume(volume);
        UpdateSoundUI();
    }

    public void SetSoundEffectsVolume(float volume)
    {
        settingsData.SetSoundEffectsVolume(volume);
        UpdateSoundUI();
    }

    private void UpdateSoundUI()
    {
        musicVolumeBar.fillAmount = settingsData.musicVolume / maxVolume;
        soundEffectsVolumeBar.fillAmount = settingsData.soundEffectsVolume / maxVolume;
    }
}
