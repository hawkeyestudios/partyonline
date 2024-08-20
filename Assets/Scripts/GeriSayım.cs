using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GeriSayım : MonoBehaviour
{
    public Text countdownText; // Geri sayımın gösterileceği Text
    public float preparationTime = 10f; // Hazırlık süresi (saniye)
    public float gameDuration = 120f; // Oyun süresi (saniye)
    public bool isGameStarted = false;

    public delegate void GameOverAction();
    public static event GameOverAction OnGameOver;

    private void Start()
    {
        // Geri sayım başlatılmadan önce bir işlem yapılacaksa buraya eklenebilir
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
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

        GameOver();
    }

    private void GameOver()
    {
        countdownText.text = "00:00";
        isGameStarted = false;
        OnGameOver?.Invoke(); // Olayı tetikleyin
    }
}
