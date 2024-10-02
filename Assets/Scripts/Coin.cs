using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CoinManager", menuName = "Game/CoinManager", order = 1)]
public class Coin : ScriptableObject
{
    public int currentCoins;

    public void Initialize()
    {
        LoadCoinsFromPlayFab();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        SaveCoinsToPlayFab();
    }

    public void SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            SaveCoinsToPlayFab();
        }
        else
        {
            Debug.Log("Yeterli coin yok.");
        }
    }
    private void SaveCoinsToPlayFab()
    {
        var data = new Dictionary<string, string>
        {
            { "Coins", currentCoins.ToString() }
        };

        var request = new UpdateUserDataRequest
        {
            Data = data
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    private void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Coin bilgileri baþarýyla PlayFab'e kaydedildi.");
    }

    private void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Coin bilgileri kaydedilemedi: " + error.GenerateErrorReport());
    }

    public void LoadCoinsFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataLoadSuccess, OnDataLoadFailure);
    }

    private void OnDataLoadSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("Coins"))
        {
            currentCoins = int.Parse(result.Data["Coins"].Value);
            Debug.Log("Coin bilgileri PlayFab'den baþarýyla yüklendi: " + currentCoins);
        }
        else
        {
            Debug.LogWarning("PlayFab'den coin bilgisi bulunamadý.");
        }
    }

    private void OnDataLoadFailure(PlayFabError error)
    {
        Debug.LogError("Coin bilgileri yüklenemedi: " + error.GenerateErrorReport());
    }
}
