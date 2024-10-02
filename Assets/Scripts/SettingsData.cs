using UnityEngine;

[CreateAssetMenu(fileName = "SettingsData", menuName = "Settings/Settings Data")]
public class SettingsData : ScriptableObject
{
    public float musicVolume = 1f;
    public float soundEffectsVolume = 1f; 
    public bool isVibrationOn = true; 

    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundEffectsVolumeKey = "SoundEffectsVolume";
    private const string VibrationKey = "Vibration";

    public void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f); 
        soundEffectsVolume = PlayerPrefs.GetFloat(SoundEffectsVolumeKey, 1f);
        isVibrationOn = PlayerPrefs.GetInt(VibrationKey, 1) == 1; 
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(SoundEffectsVolumeKey, soundEffectsVolume);
        PlayerPrefs.SetInt(VibrationKey, isVibrationOn ? 1 : 0);
        PlayerPrefs.Save(); 
    }

    public void SetVibration(bool isOn)
    {
        isVibrationOn = isOn;
        SaveSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        SaveSettings();
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectsVolume = volume;
        SaveSettings();
    }
}
