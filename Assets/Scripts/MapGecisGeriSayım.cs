using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapGecisGeriSayım : MonoBehaviour
{
    public Text countdownText; // Geri sayım için UI Text bileşeni
    private float timeRemaining = 15f; // 15 saniye geri sayım

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
        else
        {
            // Süre bittiğinde GhostPG sahnesine geçiş yap
            SceneManager.LoadScene("GhostPG");
        }
    }

    void UpdateCountdownText()
    {
        // UI Text bileşenini güncelle
        countdownText.text = Mathf.Ceil(timeRemaining).ToString() + " seconds";
    }
}
