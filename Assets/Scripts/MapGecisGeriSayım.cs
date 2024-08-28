using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MapGecisGeriSayim : MonoBehaviour
{
    public Text countdownText;
    private float timeRemaining = 15f;
    private bool hasLoaded = false;

    public List<GameObject> loadingPanels;
    private string mainMenu = "MainMenu";

    private string[] mapNames = { "TrapPG", "GhostPG", "TntPG", "CrownPG" };

    void Start()
    {
        SaveCurrentScene();
        UpdateCountdownText();
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateCountdownText();
        }
        else if (!hasLoaded)
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
            int mapIndex = System.Array.IndexOf(mapNames, nextMap);

            if (mapIndex >= 0 && mapIndex < loadingPanels.Count)
            {
                loadingPanels[mapIndex].SetActive(true);
            }

            yield return new WaitForSeconds(3f);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextMap);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        else
        {
            ClearVisitedMapRecords();
            SceneManager.LoadScene(mainMenu);
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
}
