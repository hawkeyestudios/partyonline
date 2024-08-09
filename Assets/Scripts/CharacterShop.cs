using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterShop : MonoBehaviour
{
    public Text coinText; // Karakter mağazasında coin miktarını gösteren Text
    public Text gemText; // Karakter mağazasında gem miktarını gösteren Text

    private void Start()
    {
        CoinManager.Instance.SetCoinText(coinText);
        GemManager.Instance.SetGemText(gemText);
    }
}
