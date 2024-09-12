using KnoxGameStudios;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int currentCoins;
    public Text coinText; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        currentCoins = PlayerPrefs.GetInt("Coins", 1000000);
        UpdateCoinUI();
    }

    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        SaveCoins();
        UpdateCoinUI();
    }

    public void SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            SaveCoins();
            UpdateCoinUI();
        }
        else
        {
            Debug.Log("Not enough coins.");
        }
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.Save();
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = currentCoins.ToString();
        }
    }
    public void SetCurrentCoins(int coins)
    {
        currentCoins = coins;
        UpdateCoinUI();
    }


    public void SetCoinText(Text newCoinText)
    {
        coinText = newCoinText;
        UpdateCoinUI();
    }
}
