using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Collections;

public class CharacterButton : MonoBehaviour
{
    public string characterPrefabName;
    public Transform spawnPoint;
    public int characterPrice;
    public int gemPrice;
    [SerializeField] private Text buyText;
    [SerializeField] private Text gemText;
    public Button buyButton;
    public Button gemButton;
    private bool isPurchased;
    private bool isEquipped;

    public Button equipButton;
    public Image tickImage;
    public Text uyarýText;
    private bool isWarningTextVisible = false;

    private ButtonManager buttonManager;
    public Coin coin;
    public Gem gem;

    public bool IsEquipped
    {
        get => isEquipped;
        set
        {
            isEquipped = value;
            if (tickImage != null)
            {
                tickImage.gameObject.SetActive(isEquipped);
            }
        }
    }

    private void Start()
    {
        buttonManager = FindObjectOfType<ButtonManager>();

        bool isFirstCharacter = characterPrefabName == "Yaþlý";

        isPurchased = PlayerPrefs.GetInt(characterPrefabName + "_Purchased", 0) == 1 || isFirstCharacter;
        isEquipped = PlayerPrefs.GetInt(characterPrefabName + "_Equipped", 0) == 1;

        buyButton.gameObject.SetActive(!isPurchased);
        gemButton.gameObject.SetActive(!isPurchased);

        if (equipButton != null)
        {
            tickImage = equipButton.transform.Find("Image")?.GetComponent<Image>();
            if (tickImage != null)
            {
                tickImage.gameObject.SetActive(isEquipped);
            }
            else
            {
                Debug.LogError("TickImage not found.");
            }

            equipButton.gameObject.SetActive(isPurchased);
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        }
        else
        {
            Debug.LogError("EquipButton not found.");
        }

        if (isFirstCharacter && !AnyOtherCharacterEquipped())
        {
            SpawnCharacter();
            EquipCharacter();
        }

        if (isEquipped)
        {
            buttonManager.SetLastEquippedCharacter(this);
        }
        else
        {

            string lastEquippedCharacter = PlayerPrefs.GetString("LastEquippedCharacter", "");
            if (characterPrefabName == lastEquippedCharacter)
            {
                EquipCharacter();
            }
        }
    }

    private bool AnyOtherCharacterEquipped()
    {
        foreach (var button in buttonManager.characterButtons)
        {
            if (button != this && PlayerPrefs.GetInt(button.characterPrefabName + "_Equipped", 0) == 1)
            {
                return true;
            }
        }
        return false;
    }

    public void SpawnCharacter()
    {

        foreach (Transform child in spawnPoint)
        {
            Destroy(child.gameObject);
        }

        GameObject characterPrefab = Resources.Load<GameObject>(characterPrefabName);
        if (characterPrefab != null)
        {
            GameObject characterInstance = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            characterInstance.transform.rotation = Quaternion.Euler(0, 170, 0);
            characterInstance.transform.localScale = Vector3.one * 2.5f; 

            buyText.text = $"{characterPrice}";
            gemText.text = $"{gemPrice}";

            buttonManager.UpdateActiveButton(this);

            isPurchased = PlayerPrefs.GetInt(characterPrefabName + "_Purchased", 0) == 1 || characterPrefabName == "Yaþlý";

            buyButton.gameObject.SetActive(!isPurchased);
            gemButton.gameObject.SetActive(!isPurchased);

            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(isPurchased);
                if (tickImage != null)
                {
                    tickImage.gameObject.SetActive(isEquipped);
                }
            }
        }
        else
        {
            Debug.LogError("Prefab not found: " + characterPrefabName);
        }
    }

    public void BuyCharacterWithCoins()
    {
        if (coin.currentCoins >= characterPrice)
        {
            coin.SpendCoins(characterPrice);
            isPurchased = true;
            buyButton.gameObject.SetActive(false);
            gemButton.gameObject.SetActive(false);
            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(true);
                if (tickImage != null)
                {
                    tickImage.gameObject.SetActive(false);
                }
            }
            SavePurchaseStatus();
            UpdateCharacterPurchaseStatus(); // Güncelleme
        }
        else
        {
            Debug.Log("Not enough coins to purchase the character.");
            uyarýText.text = "Not enough coins to purchase the character!";
            if (!isWarningTextVisible)
            {
                StartCoroutine(ShowWarningTextForSeconds(5f));
            }
        }
    }
    public void BuyCharacterWithGems()
    {
        if (gem.currentGems >= gemPrice)
        {
            gem.SpendGems(gemPrice);
            isPurchased = true;
            buyButton.gameObject.SetActive(false);
            gemButton.gameObject.SetActive(false);
            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(true);
                if (tickImage != null)
                {
                    tickImage.gameObject.SetActive(false);
                }
            }
            SavePurchaseStatus();
            UpdateCharacterPurchaseStatus();
        }
        else
        {
            Debug.Log("Not enough coins to purchase the character.");
            uyarýText.text = "Not enough coins to purchase the character!";
            if (!isWarningTextVisible)
            {
                StartCoroutine(ShowWarningTextForSeconds(5f));
            }
        }
    }
    private IEnumerator ShowWarningTextForSeconds(float duration)
    {
        isWarningTextVisible = true; 

        uyarýText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration); 

        uyarýText.gameObject.SetActive(false);
        isWarningTextVisible = false; 
    }

    private void UpdateCharacterPurchaseStatus()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { characterPrefabName + "_Purchased", isPurchased ? "1" : "0" },
            { characterPrefabName + "_Equipped", isEquipped ? "1" : "0" },
            { "LastEquippedCharacter", PlayerPrefs.GetString("LastEquippedCharacter", "") },
        }
        };

        if (isEquipped)
        {
            request.Data[characterPrefabName + "_TickImage"] = "1";
        }
        else
        {
            request.Data[characterPrefabName + "_TickImage"] = "0";
        }

        PlayFabClientAPI.UpdateUserData(request, OnDataUpdateSuccess, OnFailure);
    }

    private void OnDataUpdateSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Character purchase status updated successfully on PlayFab.");
    }
    private void OnPurchaseSuccess()
    {
        UpdateCharacterPurchaseStatus(); 
    }

    private void OnFailure(PlayFabError error)
    {
        Debug.Log($"Error: {error.GenerateErrorReport()}");
    }

    private void SavePurchaseStatus()
    {
        PlayerPrefs.SetInt(characterPrefabName + "_Purchased", isPurchased ? 1 : 0);
        PlayerPrefs.SetInt(characterPrefabName + "_Equipped", isEquipped ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnEquipButtonClicked()
    {
        EquipCharacter();
        buttonManager.UpdateEquipButtons(this);

        UpdateCharacterPurchaseStatus();
    }

    private void EquipCharacter()
    {
        foreach (var button in buttonManager.characterButtons)
        {
            if (button != this)
            {
                button.IsEquipped = false;
                PlayerPrefs.SetInt(button.characterPrefabName + "_Equipped", 0);
            }
        }

        IsEquipped = true;
        PlayerPrefs.SetInt(characterPrefabName + "_Equipped", 1);
        PlayerPrefs.SetString("LastEquippedCharacter", characterPrefabName);
        PlayerPrefs.Save();

        UpdateCharacterPurchaseStatus();
    }



}
