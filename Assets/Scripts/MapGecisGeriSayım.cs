using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class MapGecisGeriSayim : MonoBehaviourPunCallbacks
{
    public Text countdownText;
    private float timeRemaining = 15f;
    private bool hasLoaded = false;

    public List<GameObject> loadingPanels;
    private Dictionary<string, GameObject> mapLoadingPanels = new Dictionary<string, GameObject>();
    private List<string> mapList;
    private int currentMapIndex = 0;

    void Start()
    {
        mapLoadingPanels.Add("TrapPG", loadingPanels[0]);
        mapLoadingPanels.Add("GhostPG", loadingPanels[1]);
        mapLoadingPanels.Add("TntPG", loadingPanels[2]);
        mapLoadingPanels.Add("CrownPG", loadingPanels[3]);
        mapLoadingPanels.Add("SumoPG", loadingPanels[4]);
        mapLoadingPanels.Add("RoulettePG", loadingPanels[5]);

        string selectedMaps = PlayerPrefs.GetString("SelectedMaps");
        mapList = selectedMaps.Split(',').ToList();
        currentMapIndex = PlayerPrefs.GetInt("currentMapIndex", 0);
        Debug.Log("Ba�lang�� harita listesi: " + string.Join(", ", mapList) + " - currentMapIndex: " + currentMapIndex);

        UpdateCountdownText();
    }

    void ShowLoadingPanel()
    {
        string currentMap = mapList[currentMapIndex];

        if (mapLoadingPanels.ContainsKey(currentMap))
        {
            Debug.Log("Loading panel ekrana verildi: " + currentMap);
            mapLoadingPanels[currentMap].SetActive(true);

            StartCoroutine(LoadMapAfterDelay(currentMap));
        }
        else
        {
            Debug.LogError("Harita bulunamad�: " + currentMap);
        }
    }

    IEnumerator LoadMapAfterDelay(string mapName)
    {
        yield return new WaitForSeconds(3f);

        if (mapLoadingPanels.ContainsKey(mapName))
        {
            mapLoadingPanels[mapName].SetActive(false);
        }

        // Haritay� y�kle
        Debug.Log("Harita y�kleniyor: " + mapName);
        PhotonNetwork.LoadLevel(mapName);
    }

    public void OnLevelCompleted()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentMapIndex++;
            PlayerPrefs.SetInt("currentMapIndex", currentMapIndex);
            PlayerPrefs.Save();

            Debug.Log("CurrentMapIndex g�ncellendi: " + currentMapIndex);

            if (currentMapIndex < mapList.Count)
            {
                Debug.Log("S�radaki harita: " + mapList[currentMapIndex]);
                ShowLoadingPanel();
            }
            else
            {
                Debug.Log("T�m haritalar tamamland�. Ana men�ye d�n�l�yor.");
                ReturnToMainMenu();
            }
        }
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateCountdownText();
        }
        else if (!hasLoaded && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("S�re bitti, bir sonraki harita y�kleniyor.");
            OnLevelCompleted();
            hasLoaded = true;
        }
    }

    public void ReturnToMainMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("MainMenu");
        Debug.Log("Ana men�ye d�n�ld�.");
    }

    void UpdateCountdownText()
    {
        countdownText.text = Mathf.Ceil(timeRemaining).ToString() + " saniye";
    }
}
