using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterShop : MonoBehaviour
{
    public Text coinText;
    public Text gemText; 

    private void Start()
    {
        CoinManager.Instance.SetCoinText(coinText);
        GemManager.Instance.SetGemText(gemText);
    }
}
