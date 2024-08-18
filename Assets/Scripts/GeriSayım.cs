using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class GeriSayım : MonoBehaviourPunCallbacks
{
    public Text countdownText; // Geri sayımın gösterileceği Text
    public GameObject gameOverPanel; // Game Over paneli
    public float preparationTime = 10f; // Hazırlık süresi (saniye)
    public float gameDuration = 120f; // Oyun süresi (saniye)
    public bool isGameStarted = false;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        // 10 saniyelik hazırlık geri sayımı
        while (preparationTime > 0)
        {
            countdownText.text = "Get Ready " + preparationTime.ToString("0");
            preparationTime -= Time.deltaTime;
            yield return null;
        }

        // 2 dakikalık oyun geri sayımı
        isGameStarted = true;
        while (gameDuration > 0)
        {
            int minutes = Mathf.FloorToInt(gameDuration / 60);
            int seconds = Mathf.FloorToInt(gameDuration % 60);

            countdownText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            gameDuration -= Time.deltaTime;
            yield return null;
        }

        // Oyun bittiğinde "Game Over" panelini göster ve sonuçları güncelle
        GameOver();
    }

    private void GameOver()
    {
        countdownText.text = "00:00";
        isGameStarted = false;
        photonView.RPC("ShowGameOverPanel", RpcTarget.All);
    }

    [PunRPC]
    private void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }
}
