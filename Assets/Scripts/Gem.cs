using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GemManager", menuName = "Game/GemManager", order = 2)]
public class Gem : ScriptableObject
{
    public int currentGems;

    public void Initialize()
    {
        LoadGemsFromPlayFab();
    }

    public void AddGems(int amount)
    {
        currentGems += amount;
        SaveGemsToPlayFab();
    }

    public void SpendGems(int amount)
    {
        if (currentGems >= amount)
        {
            currentGems -= amount;
            SaveGemsToPlayFab();
        }
        else
        {
            Debug.Log("Yeterli gem yok.");
        }
    }
    private void SaveGemsToPlayFab()
    {
        var data = new Dictionary<string, string>
        {
            { "Gems", currentGems.ToString() }
        };

        var request = new UpdateUserDataRequest
        {
            Data = data
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    private void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Gem bilgileri baþarýyla PlayFab'e kaydedildi.");
    }

    private void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Gem bilgileri kaydedilemedi: " + error.GenerateErrorReport());
    }
    public void LoadGemsFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataLoadSuccess, OnDataLoadFailure);
    }

    private void OnDataLoadSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("Gems"))
        {
            currentGems = int.Parse(result.Data["Gems"].Value);
            Debug.Log("Gem bilgileri PlayFab'den baþarýyla yüklendi: " + currentGems);
        }
        else
        {
            Debug.LogWarning("PlayFab'den gem bilgisi bulunamadý.");
        }
    }

    private void OnDataLoadFailure(PlayFabError error)
    {
        Debug.LogError("Gem bilgileri yüklenemedi: " + error.GenerateErrorReport());
    }
}
