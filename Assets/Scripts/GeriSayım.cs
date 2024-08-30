using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class GeriSayım : MonoBehaviourPunCallbacks
{
    public Text countdownText;
    public float preparationTime = 10f;
    public float gameDuration = 120f;
    public bool isGameStarted = false;

    public delegate void GameOverAction();
    public static event GameOverAction OnGameOver;

    private bool isCountdownStarted = false; // Yeni kontrol değişkeni

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient && !isCountdownStarted)
        {
            StartCountdown();
        }
    }

    public void StartCountdown()
    {
        if (PhotonNetwork.IsMasterClient && !isCountdownStarted)
        {
            isCountdownStarted = true; // Coroutine başlatılmadan önce kontrolü etkinleştir
            StartCoroutine(CountdownRoutine());
        }
    }

    private IEnumerator CountdownRoutine()
    {
        // 10 saniyelik hazırlık geri sayımı
        while (preparationTime > 0)
        {
            photonView.RPC("UpdateCountdownText", RpcTarget.All, "Get Ready " + preparationTime.ToString("0"));
            preparationTime -= Time.deltaTime;
            yield return null;
        }

        // 2 dakikalık oyun geri sayımı
        isGameStarted = true;
        while (gameDuration > 0)
        {
            int minutes = Mathf.FloorToInt(gameDuration / 60);
            int seconds = Mathf.FloorToInt(gameDuration % 60);
            string countdownDisplay = string.Format("{0:0}:{1:00}", minutes, seconds);
            photonView.RPC("UpdateCountdownText", RpcTarget.All, countdownDisplay);
            gameDuration -= Time.deltaTime;
            yield return null;
        }

        photonView.RPC("GameOver_RPC", RpcTarget.AllBuffered); // GameOver'ı tüm oyuncular için tetikle
    }

    [PunRPC]
    private void GameOver_RPC()
    {
        UpdateCountdownText("00:00");
        isGameStarted = false;
        OnGameOver?.Invoke(); // Olayı tetikleyin
    }

    [PunRPC]
    private void UpdateCountdownText(string text)
    {
        countdownText.text = text;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient && !isGameStarted)
        {
            StartCountdown();
        }
    }
}
