using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public GameObject[] characterPrefabs; 
    public Transform previewPosition; 
    public Button purchaseButton; 
    public Button selectButton; 
    public Text purchaseButtonText; 
    public float[] characterPrices; 
    public Image[] catalogCharacterButtons; 
    private GameObject currentPreview; 
    public Text warningText;
    public GameObject itemButtonPrefab; 
    public Transform itemsContainer;
    public Coin coin;
    public Text coinText;

    private int lastSelectedCharacterIndex;
    void Start()
    {
        OfferFirstCharacterForFree();
        LoadPurchasedCharactersFromPlayFab();

        lastSelectedCharacterIndex = PlayerPrefs.GetInt("LastEquippedCharacterIndex", 0);
        ShowCharacterPreview(lastSelectedCharacterIndex);

        
        UpdateCatalogUI(lastSelectedCharacterIndex);
        CheckAndUpdateLockedImages();
    }
    public void ShowCharacterPreview(int characterIndex)
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        currentPreview = Instantiate(characterPrefabs[characterIndex], previewPosition.position, Quaternion.Euler(0, 165, 0));
        currentPreview.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

        UpdateButtons(characterIndex);
        InstantiateCharacterItems(characterIndex);

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(() => TryPurchaseCharacter(characterIndex));

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => SelectCharacter(characterIndex));
    }
    private void InstantiateCharacterItems(int characterIndex)
    {
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        Item[] items = currentPreview.GetComponentsInChildren<Item>(true); 

        if (items.Length == 0)
        {
            Debug.LogError("No items found on character!");
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            GameObject itemButton = Instantiate(itemButtonPrefab, itemsContainer);

            int currentItemIndex = i; 

            if (i == 0)
            {
                items[i].gameObject.SetActive(true);
            }
            else
            {
                items[i].gameObject.SetActive(false);
            }

            itemButton.transform.Find("ItemIcon").GetComponent<Image>().sprite = items[i].itemIcon;
            itemButton.transform.Find("ItemName").GetComponent<Text>().text = items[i].itemName;

            itemButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ActivateItem(items, currentItemIndex); 
            });
        }
    }

    private void ActivateItem(Item[] items, int itemIndex)
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(i == itemIndex);
        }
    }

    public void TryPurchaseCharacter(int characterIndex)
    {
        int characterPrice = (int)characterPrices[characterIndex];
        int currentCoins = coin.currentCoins;

        if (currentCoins >= characterPrice)
        {
            coin.SpendCoins(characterPrice);
            PurchaseCharacter(characterIndex);
            coinText.text = coin.currentCoins.ToString();
        }
        else
        {
            if (warningText != null)
            {
                warningText.text = "Not Enough Coin!";
            }
        }
    }
    public void UpdateButtons(int characterIndex)
    {
        if (PlayerPrefs.GetInt("CharacterPurchased_" + characterIndex) == 1)
        {
            purchaseButton.gameObject.SetActive(false);
            selectButton.gameObject.SetActive(true); 
        }
        else 
        {
            purchaseButton.gameObject.SetActive(true); 
            selectButton.gameObject.SetActive(false); 
            purchaseButtonText.text = characterPrices[characterIndex].ToString("F0") + " Coins"; 
        }
    }
    public void PurchaseCharacter(int characterIndex)
    {
        PlayerPrefs.SetInt("CharacterPurchased_" + characterIndex, 1);
        PlayerPrefs.SetInt("CharacterLocked_" + characterIndex, 0);

        UpdateButtons(characterIndex);
        HideLockedImage(characterIndex);

        SavePurchasedCharacterToPlayFab(characterIndex);
    }

    private void SavePurchasedCharacterToPlayFab(int characterIndex)
    {
        var purchasedCharacters = new List<int>();
        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            if (PlayerPrefs.GetInt("CharacterPurchased_" + i) == 1)
            {
                purchasedCharacters.Add(i);
            }
        }
        var data = new Dictionary<string, string>
        {
            { "PurchasedCharacters", string.Join(",", purchasedCharacters) }
        };

        var request = new PlayFab.ClientModels.UpdateUserDataRequest
        {
            Data = data
        };

        PlayFab.PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    private void OnDataSaveSuccess(PlayFab.ClientModels.UpdateUserDataResult result)
    {
        Debug.Log("Satýn alýnan karakter bilgileri baþarýyla PlayFab'e kaydedildi.");
    }

    private void OnDataSaveFailure(PlayFab.PlayFabError error)
    {
        Debug.LogError("Satýn alýnan karakter bilgileri kaydedilemedi: " + error.GenerateErrorReport());
    }
    public void LoadPurchasedCharactersFromPlayFab()
    {
        PlayFab.PlayFabClientAPI.GetUserData(new PlayFab.ClientModels.GetUserDataRequest(), OnDataLoadSuccess, OnDataLoadFailure);
    }

    private void OnDataLoadSuccess(PlayFab.ClientModels.GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("PurchasedCharacters"))
        {
            string[] purchasedCharacters = result.Data["PurchasedCharacters"].Value.Split(',');

            foreach (var characterIndex in purchasedCharacters)
            {
                int index = int.Parse(characterIndex);
                PlayerPrefs.SetInt("CharacterPurchased_" + index, 1);
                PlayerPrefs.SetInt("CharacterLocked_" + index, 0);

                UpdateButtons(index);
                HideLockedImage(index);
            }

            Debug.Log("Satýn alýnan karakterler baþarýyla PlayFab'den geri yüklendi.");
        }
        else
        {
            Debug.LogWarning("PlayFab'de karakter bilgisi bulunamadý.");
        }
    }

    private void OnDataLoadFailure(PlayFab.PlayFabError error)
    {
        Debug.LogError("Satýn alýnan karakter bilgileri yüklenemedi: " + error.GenerateErrorReport());
    }
    private void HideLockedImage(int characterIndex)
    {
        Transform characterButton = catalogCharacterButtons[characterIndex].transform;
        Transform lockedImageTransform = characterButton.Find("Locked");

        if (lockedImageTransform != null)
        {
            lockedImageTransform.gameObject.SetActive(false);
        }
    }
    public void CheckAndUpdateLockedImages()
    {
        for (int i = 0; i < catalogCharacterButtons.Length; i++)
        {
            if (PlayerPrefs.GetInt("CharacterLocked_" + i, 1) == 0)
            {
                HideLockedImage(i);
            }
        }
    }
    public void SelectCharacter(int characterIndex)
    {
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
        PlayerPrefs.SetString("LastEquippedCharacter", characterPrefabs[characterIndex].name);
        PlayerPrefs.SetInt("LastEquippedCharacterIndex", characterIndex);
        PlayerPrefs.Save();

        selectButton.gameObject.SetActive(false);
        UpdateCatalogUI(characterIndex); 
    }
    public void UpdateCatalogUI(int selectedCharacterIndex)
    {
        for (int i = 0; i < catalogCharacterButtons.Length; i++)
        {
            Transform tickTransform = catalogCharacterButtons[i].transform.Find("Tick");
            if (tickTransform != null)
            {
                tickTransform.gameObject.SetActive(false);
            }
        }

        Transform selectedTickTransform = catalogCharacterButtons[selectedCharacterIndex].transform.Find("Tick");
        if (selectedTickTransform != null)
        {
            selectedTickTransform.gameObject.SetActive(true);
        }

        PlayerPrefs.SetInt("SelectedCharacterTickIndex", selectedCharacterIndex);
        PlayerPrefs.Save();
    }
    public void OfferFirstCharacterForFree()
    {
        bool anyCharacterPurchased = false;

        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            if (PlayerPrefs.GetInt("CharacterPurchased_" + i) == 1)
            {
                anyCharacterPurchased = true;
                break;
            }
        }

        if (!anyCharacterPurchased)
        {
            PlayerPrefs.SetInt("CharacterPurchased_0", 1);
            PlayerPrefs.SetInt("SelectedCharacter", 0);
            PlayerPrefs.SetInt("LastEquippedCharacterIndex", 0);
            UpdateCatalogUI(0);
        }
    }
}
