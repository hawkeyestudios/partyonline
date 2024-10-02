using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Level Data", menuName = "Level/Level Data")]
public class LevelData : ScriptableObject
{
    public int[] xpRequirements;
    public int currentXP = 0;
    public int currentLevel = 1;

    public void AddXP(int amount)
    {
        currentXP += amount;
        CheckLevelUp();
        SaveLevelDataToPlayFab();
    }
    private void CheckLevelUp()
    {
        while (currentXP >= xpRequirements[currentLevel - 1])
        {
            currentXP -= xpRequirements[currentLevel - 1];
            currentLevel++;
            Debug.Log("Level atladý: " + currentLevel);
        }
    }

    public void SaveLevelDataToPlayFab()
    {
        var data = new Dictionary<string, string>
        {
            { "Level", currentLevel.ToString() },
            { "XP", currentXP.ToString() }
        };

        var request = new UpdateUserDataRequest
        {
            Data = data
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    private void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Level ve XP bilgileri baþarýyla PlayFab'e kaydedildi.");
    }

    private void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Level ve XP bilgileri PlayFab'e kaydedilemedi: " + error.GenerateErrorReport());
    }

    public void LoadLevelDataFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataLoadSuccess, OnDataLoadFailure);
    }

    private void OnDataLoadSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("Level") && result.Data.ContainsKey("XP"))
        {
            currentLevel = int.Parse(result.Data["Level"].Value);
            currentXP = int.Parse(result.Data["XP"].Value);
            Debug.Log("Level ve XP bilgileri PlayFab'den baþarýyla yüklendi.");
        }
        else
        {
            Debug.LogWarning("Level veya XP bilgisi PlayFab'den bulunamadý.");
        }
    }

    private void OnDataLoadFailure(PlayFabError error)
    {
        Debug.LogError("Level ve XP bilgileri PlayFab'den yüklenemedi: " + error.GenerateErrorReport());
    }
}
