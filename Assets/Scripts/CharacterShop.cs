using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterShop : MonoBehaviour
{
    public Text coinText; // Karakter maðazasýnda coin miktarýný gösteren Text
    public Text gemText; // Karakter maðazasýnda gem miktarýný gösteren Text

    private void Start()
    {
        CoinManager.Instance.SetCoinText(coinText);
        GemManager.Instance.SetGemText(gemText);
    }
}
