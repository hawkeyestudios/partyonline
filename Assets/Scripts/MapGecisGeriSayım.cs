using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MapGecisGeriSayim : MonoBehaviourPunCallbacks
{
    public Text countdownText;
    private float timeRemaining = 15f;
    private bool hasLoaded = false;

    public List<GameObject> loadingPanels;
    private string mainMenu = "MainMenu";

    private string[] mapNames = { "TrapPG", "GhostPG", "TntPG", "CrownPG", "SumoPG" };

    private bool allPlayersReady = false;

    void Start()
    {
        SaveCurrentScene();
        UpdateCountdownText();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CheckAllPlayersReady());
        }
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateCountdownText();
        }
        else if (!hasLoaded && PhotonNetwork.IsMasterClient && allPlayersReady)
        {
            StartCoroutine(NextSceneLoading());
            hasLoaded = true;
        }
    }

    void UpdateCountdownText()
    {
        countdownText.text = Mathf.Ceil(timeRemaining).ToString() + " seconds";
    }

    IEnumerator NextSceneLoading()
    {
        string nextMap = GetRandomUnvisitedMap();
        if (!string.IsNullOrEmpty(nextMap))
        {
            photonView.RPC("LoadMapForAllPlayers", RpcTarget.All, nextMap);
        }
        else
        {
            ClearVisitedMapRecords();
            SceneManager.LoadScene(mainMenu);
        }

        yield break;
    }

    [PunRPC]
    void LoadMapForAllPlayers(string mapName)
    {
        int mapIndex = System.Array.IndexOf(mapNames, mapName);

        if (mapIndex >= 0 && mapIndex < loadingPanels.Count)
        {
            loadingPanels[mapIndex].SetActive(true);
        }

        StartCoroutine(LoadMapAsync(mapName));
    }

    IEnumerator LoadMapAsync(string mapName)
    {
        yield return new WaitForSeconds(3f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mapName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    void SaveCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetInt(currentSceneName, 1);
        PlayerPrefs.Save();
    }

    string GetRandomUnvisitedMap()
    {
        List<string> unvisitedMaps = new List<string>();

        foreach (string mapName in mapNames)
        {
            if (PlayerPrefs.GetInt(mapName, 0) == 0)
            {
                unvisitedMaps.Add(mapName);
            }
        }

        if (unvisitedMaps.Count > 0)
        {
            int randomIndex = Random.Range(0, unvisitedMaps.Count);
            return unvisitedMaps[randomIndex];
        }
        else
        {
            return null;
        }
    }

    void ClearVisitedMapRecords()
    {
        foreach (string mapName in mapNames)
        {
            PlayerPrefs.DeleteKey(mapName);
        }
        PlayerPrefs.Save();

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    IEnumerator CheckAllPlayersReady()
    {
        while (!allPlayersReady)
        {
            yield return new WaitForSeconds(1f);
            photonView.RPC("PlayerIsReady", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void PlayerIsReady()
    {
        allPlayersReady = PhotonNetwork.PlayerList.Length == PhotonNetwork.CurrentRoom.PlayerCount;
    }
}
