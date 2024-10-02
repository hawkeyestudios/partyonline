using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public GameObject leaderboardUIEntryPrefab;
    public Transform leaderboardContent;
    public string statisticName = "PlayerScore";
    public PlayerScoreData playerScoreData;

    private void Start()
    {
        GetPlayerScoreFromPlayFab();
    }

    public void GetPlayerScoreFromPlayFab()
    {
        var request = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(request, OnGetPlayerScoreSuccess, OnGetPlayerScoreFailure);
    }

    private void OnGetPlayerScoreSuccess(GetPlayerStatisticsResult result)
    {
        foreach (var stat in result.Statistics)
        {
            if (stat.StatisticName == statisticName)
            {
                playerScoreData.totalScore = stat.Value;
                Debug.Log("PlayerScore PlayFab'den baþarýyla alýndý: " + stat.Value);
                break;
            }
        }
        UpdateLeaderboardFromPlayerScore();
        GetLeaderboard(50);
    }

    private void OnGetPlayerScoreFailure(PlayFabError error)
    {
        Debug.LogError("PlayerScore'u alýrken hata oluþtu: " + error.GenerateErrorReport());
    }

    public void GetLeaderboard(int maxResults = 50)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0,
            MaxResultsCount = maxResults
        };

        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGetSuccess, OnLeaderboardGetFailure);
    }

    private void OnLeaderboardGetSuccess(GetLeaderboardResult result)
    {
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in result.Leaderboard)
        {
            GameObject entry = Instantiate(leaderboardUIEntryPrefab, leaderboardContent);

            Text rankText = entry.transform.Find("RankText").GetComponent<Text>();
            Text nameText = entry.transform.Find("NameText").GetComponent<Text>();
            Text scoreText = entry.transform.Find("ScoreText").GetComponent<Text>();

            rankText.text = (item.Position + 1).ToString();
            nameText.text = string.IsNullOrEmpty(item.DisplayName) ? item.PlayFabId : item.DisplayName;
            scoreText.text = item.StatValue.ToString();
        }
    }

    private void OnLeaderboardGetFailure(PlayFabError error)
    {
        Debug.LogError("Leaderboard alýnýrken hata: " + error.GenerateErrorReport());
    }

    public void UpdateLeaderboardFromPlayerScore()
    {
        int totalScore = (int)playerScoreData.totalScore;

        if (totalScore > 0)
        {
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate { StatisticName = statisticName, Value = totalScore }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdateSuccess, OnLeaderboardUpdateFailure);
        }
    }

    private void OnLeaderboardUpdateSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Leaderboard baþarýyla güncellendi.");
    }

    private void OnLeaderboardUpdateFailure(PlayFabError error)
    {
        Debug.LogError("Leaderboard güncelleme hatasý: " + error.GenerateErrorReport());
    }
}
