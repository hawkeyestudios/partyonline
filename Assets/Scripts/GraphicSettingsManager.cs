using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GraphicSettingsManager : MonoBehaviour
{
    public Dropdown qualityDropdown; // UI Dropdown
    public GraphicsSettings graphicsSettings; // ScriptableObject referansý

    void Start()
    {
        // Grafik ayarlarýný yükle ve uygula.
        LoadGraphicsSettings();

        // Dropdown'u kalite seviyeleriyle doldur.
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

        // ScriptableObject'teki deðeri Dropdown'a yansýt.
        qualityDropdown.value = graphicsSettings.selectedQualityIndex;
        qualityDropdown.onValueChanged.AddListener(SetGraphicsQuality);
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        // Seçilen kalite seviyesini uygula ve ScriptableObject'e kaydet.
        graphicsSettings.selectedQualityIndex = qualityIndex;
        graphicsSettings.ApplyQualitySettings();

        // Ayarlarý PlayerPrefs'e kaydet.
        SaveGraphicsSettings();
    }

    public void SaveGraphicsSettings()
    {
        // PlayerPrefs ile grafik kalitesini kaydet.
        PlayerPrefs.SetInt("GraphicsQuality", graphicsSettings.selectedQualityIndex);
        PlayerPrefs.Save();
    }

    public void LoadGraphicsSettings()
    {
        // Daha önce kaydedilmiþ grafik kalitesi var mý diye kontrol et.
        if (PlayerPrefs.HasKey("GraphicsQuality"))
        {
            // Kaydedilen kaliteyi yükle.
            graphicsSettings.selectedQualityIndex = PlayerPrefs.GetInt("GraphicsQuality");
            graphicsSettings.ApplyQualitySettings();
        }
        else
        {
            // Eðer kaydedilmiþ bir ayar yoksa, varsayýlan kaliteyi uygula.
            graphicsSettings.ApplyQualitySettings();
        }
    }
}
