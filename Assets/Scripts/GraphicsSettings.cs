using UnityEngine;

[CreateAssetMenu(fileName = "GraphicsSettings", menuName = "Settings/GraphicsSettings")]
public class GraphicsSettings : ScriptableObject
{
    public int selectedQualityIndex; // Seçilen kalite seviyesini tutar.

    public void ApplyQualitySettings()
    {
        QualitySettings.SetQualityLevel(selectedQualityIndex);
    }
}
