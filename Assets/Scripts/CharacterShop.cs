using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterShop : MonoBehaviour
{
    public Text coinText;
    public Text gemText; 
    public Coin coin;
    public Gem gem;

    private void Start()
    {
        coinText.text = coin.currentCoins.ToString();
        gemText.text = gem.currentGems.ToString();
    }
}
