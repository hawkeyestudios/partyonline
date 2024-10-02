using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GraphicSettingsManager : MonoBehaviour
{
    public Dropdown qualityDropdown; // UI Dropdown
    public GraphicsSettings graphicsSettings; // ScriptableObject referans�

    void Start()
    {
        // Grafik ayarlar�n� y�kle ve uygula.
        LoadGraphicsSettings();

        // Dropdown'u kalite seviyeleriyle doldur.
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

        // ScriptableObject'teki de�eri Dropdown'a yans�t.
        qualityDropdown.value = graphicsSettings.selectedQualityIndex;
        qualityDropdown.onValueChanged.AddListener(SetGraphicsQuality);
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        // Se�ilen kalite seviyesini uygula ve ScriptableObject'e kaydet.
        graphicsSettings.selectedQualityIndex = qualityIndex;
        graphicsSettings.ApplyQualitySettings();

        // Ayarlar� PlayerPrefs'e kaydet.
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
        // Daha �nce kaydedilmi� grafik kalitesi var m� diye kontrol et.
        if (PlayerPrefs.HasKey("GraphicsQuality"))
        {
            // Kaydedilen kaliteyi y�kle.
            graphicsSettings.selectedQualityIndex = PlayerPrefs.GetInt("GraphicsQuality");
            graphicsSettings.ApplyQualitySettings();
        }
        else
        {
            // E�er kaydedilmi� bir ayar yoksa, varsay�lan kaliteyi uygula.
            graphicsSettings.ApplyQualitySettings();
        }
    }
}
