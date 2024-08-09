using UnityEngine;
using UnityEngine.UI;

public class MainMenuCoin : MonoBehaviour
{
    public Text coinText; // Ana menüdeki coin miktarýný gösteren Text
    public Text gemText; // Ana menüdeki gem miktarýný gösteren Text
    public GameObject announcementPanel;
    public Button okButton;

    private void Start()
    {
        CoinManager.Instance.SetCoinText(coinText);
        GemManager.Instance.SetGemText(gemText);

        if (PlayerPrefs.GetInt("FirstTimeRegistration", 0) == 1)
        {
            // Paneli aç
            if (announcementPanel != null)
            {
                announcementPanel.SetActive(true);
            }

            // Ýlk kaydýn tamamlandýðýný belirt
            PlayerPrefs.SetInt("FirstTimeRegistration", 0);
            PlayerPrefs.Save();
        }
        else
        {
            // Daha önce kayýt olmuþsa paneli gizle
            if (announcementPanel != null)
            {
                announcementPanel.SetActive(false);
            }
        }

        if (okButton != null)
        {
            okButton.onClick.AddListener(CloseAnnouncementPanel);
        }
        else
        {
            Debug.LogError("OK Button not assigned.");
        }
    }
    private void CloseAnnouncementPanel()
    {
        if (announcementPanel != null)
        {
            announcementPanel.SetActive(false);
        }
    }
}
