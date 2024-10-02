using UnityEngine;
using UnityEngine.UI;

public class MainMenuCoin : MonoBehaviour
{
    public Text coinText;
    public Text gemText; 
    public GameObject announcementPanel;
    public Button okButton;
    public Coin coin;
    public Gem gem;

    private void Start()
    {
        coinText.text = coin.currentCoins.ToString();
        gemText.text = gem.currentGems.ToString();

        if (PlayerPrefs.GetInt("FirstTimeRegistration", 0) == 1)
        {

            if (announcementPanel != null)
            {
                announcementPanel.SetActive(true);
            }


            PlayerPrefs.SetInt("FirstTimeRegistration", 0);
            PlayerPrefs.Save();
        }
        else
        {

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
