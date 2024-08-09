using UnityEngine;
using UnityEngine.UI;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance;

    private int currentGems;
    public Text gemText; // UI'daki gem miktarýný gösteren Text

    private void Awake()
    {
        // Singleton tasarým deseni
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Oyun sahnesi deðiþse bile bu objeyi yok etme
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentGems = PlayerPrefs.GetInt("Gems", 50); // Varsayýlan gem miktarý 100
        UpdateGemUI();
    }

    public int GetCurrentGems()
    {
        return currentGems;
    }

    public void AddGems(int amount)
    {
        currentGems += amount;
        SaveGems();
        UpdateGemUI();
    }

    public void SpendGems(int amount)
    {
        if (currentGems >= amount)
        {
            currentGems -= amount;
            SaveGems();
            UpdateGemUI();
        }
        else
        {
            Debug.Log("Not enough gems.");
        }
    }

    private void SaveGems()
    {
        PlayerPrefs.SetInt("Gems", currentGems);
        PlayerPrefs.Save();
    }

    private void UpdateGemUI()
    {
        if (gemText != null)
        {
            gemText.text = currentGems.ToString();
        }
    }
    public void SetCurrentGems(int gems)
    {
        currentGems = gems;
        UpdateGemUI(); // UI'yi güncelle
    }


    public void SetGemText(Text newGemText)
    {
        gemText = newGemText;
        UpdateGemUI();
    }
}
