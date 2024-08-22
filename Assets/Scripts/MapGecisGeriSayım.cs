using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MapGecisGeriSayım : MonoBehaviour
{
    public Text countdownText; // Geri sayım için UI Text bileşeni
    private float timeRemaining = 15f; // 15 saniye geri sayım
    public string levelName;
    private bool hasLoaded = false; // Sahne yüklendi mi kontrolü
    public GameObject loadingPanel;

    void Start()
    {
        UpdateCountdownText();
    }

    void Update()
    {
        // Geri sayım süresini azalt
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateCountdownText();
        }
        else if (!hasLoaded)
        {
            // Zaman sıfıra ulaştığında ve sahne henüz yüklenmediyse sahneyi yükle
            StartCoroutine(NextSceneLoading());
            hasLoaded = true; // Sahnenin sadece bir kez yüklenmesini sağla
        }
    }

    void UpdateCountdownText()
    {
        // UI Text bileşenini güncelle
        countdownText.text = Mathf.Ceil(timeRemaining).ToString() + " seconds";
    }

    IEnumerator NextSceneLoading()
    {
        loadingPanel.SetActive(true);

        yield return new WaitForSeconds(3f); // 3 saniye bekle
        // Sahneyi yükle
        PhotonNetwork.LoadLevel(levelName);
    }
}
