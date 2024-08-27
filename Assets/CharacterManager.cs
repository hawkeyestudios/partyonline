using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public GameObject[] characterPrefabs; // Her karakterin 3D prefabs
    public Transform previewPosition; // Karakter önizlemesinin yapýlacaðý pozisyon
    public Button purchaseButton; // Satýn Al butonu
    public Button selectButton; // Seç butonu
    public Text purchaseButtonText; // Satýn Al butonunun text bileþeni
    public float[] characterPrices; // Karakter fiyatlarý
    public Image[] catalogCharacterButtons; // Katalogdaki karakter butonlarý
    private GameObject currentPreview; // Þu anda gösterilen karakter önizleme objesi
    public Text warningText;
    public GameObject itemButtonPrefab; // Her item için kullanýlacak buton prefabý
    public Transform itemsContainer;
    void Start()
    {
        OfferFirstCharacterForFree(); // Ýlk karakteri ücretsiz sunma kontrolü

        // En son seçilen karakteri al
        int lastSelectedCharacterIndex = PlayerPrefs.GetInt("LastEquippedCharacter", 0); // Eðer hiç seçim yapýlmadýysa default olarak 0 döner

        // En son seçilen karakteri instantiate et
        ShowCharacterPreview(lastSelectedCharacterIndex);

        // Tick iþaretlerini güncelle
        UpdateCatalogUI(lastSelectedCharacterIndex);
        CheckAndUpdateLockedImages();
    }
    public void ShowCharacterPreview(int characterIndex)
    {
        // Mevcut önizleme objesini temizle
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        // Yeni karakter prefabýný oluþtur ve doðru yön, ölçek ve pozisyonda ayarla
        currentPreview = Instantiate(characterPrefabs[characterIndex], previewPosition.position, Quaternion.Euler(0, 165, 0)); // Rotasyon ayarý

        // Karakterin boyutunu ayarlamak için localScale'i kullanabilirsin
        currentPreview.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f); // Boyut ayarý
        // Butonlarý güncelle
        UpdateButtons(characterIndex);
        InstantiateCharacterItems(characterIndex);
        // Satýn Al Butonu Ýçin Dinamik Baðlama
        purchaseButton.onClick.RemoveAllListeners(); // Önceki tüm dinleyicileri kaldýr
        purchaseButton.onClick.AddListener(() => TryPurchaseCharacter(characterIndex)); // Satýn al butonuna karakter index'ini baðla

        // Seç Butonu Ýçin Dinamik Baðlama
        selectButton.onClick.RemoveAllListeners(); // Önceki tüm dinleyicileri kaldýr
        selectButton.onClick.AddListener(() => SelectCharacter(characterIndex)); // Seç butonuna karakter index'ini baðla
    }
    private void InstantiateCharacterItems(int characterIndex)
    {
        // Önce paneldeki mevcut item butonlarýný temizle
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Karakter altýndaki tüm itemleri bulmak için GetComponentsInChildren kullan
        Item[] items = currentPreview.GetComponentsInChildren<Item>(true); // Tüm child'lardaki item bileþenlerini al

        if (items.Length == 0)
        {
            Debug.LogError("No items found on character!");
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            GameObject itemButton = Instantiate(itemButtonPrefab, itemsContainer);

            int currentItemIndex = i; // Lambda fonksiyonu için indexi yakala

            // Ýlk item varsayýlan olarak aktif, diðerleri deaktif
            if (i == 0)
            {
                items[i].gameObject.SetActive(true);
            }
            else
            {
                items[i].gameObject.SetActive(false);
            }

            // Butonun görüntüsünü ve ismini ayarla (opsiyonel)
            itemButton.transform.Find("ItemIcon").GetComponent<Image>().sprite = items[i].itemIcon;
            itemButton.transform.Find("ItemName").GetComponent<Text>().text = items[i].itemName;

            // Butona itemi aktifleþtirecek dinamik listener ekle
            itemButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ActivateItem(items, currentItemIndex); // Ýlgili itemi aktifleþtir
            });
        }
    }

    private void ActivateItem(Item[] items, int itemIndex)
    {
        // Tüm itemleri deaktif et, sadece seçilen itemi aktif et
        for (int i = 0; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(i == itemIndex);
        }
    }

    public void TryPurchaseCharacter(int characterIndex)
    {
        int characterPrice = (int)characterPrices[characterIndex];

        // CoinManager'dan mevcut coin miktarýný al
        int currentCoins = CoinManager.Instance.GetCurrentCoins();

        if (currentCoins >= characterPrice)
        {
            // Yeterli coin varsa karakteri satýn al
            CoinManager.Instance.SpendCoins(characterPrice);
            PurchaseCharacter(characterIndex);
        }
        else
        {
            // Yetersiz coin uyarýsý göster
            if (warningText != null)
            {
                warningText.text = "Not Enough Coin!";
            }
        }
    }

    // Butonlarýn Durumunu Güncelleme Fonksiyonu
    public void UpdateButtons(int characterIndex)
    {
        // Eðer karakter satýn alýndýysa
        if (PlayerPrefs.GetInt("CharacterPurchased_" + characterIndex) == 1)
        {
            purchaseButton.gameObject.SetActive(false); // Satýn Al butonunu gizle
            selectButton.gameObject.SetActive(true); // Seç butonunu göster
        }
        else // Karakter satýn alýnmadýysa
        {
            purchaseButton.gameObject.SetActive(true); // Satýn Al butonunu göster
            selectButton.gameObject.SetActive(false); // Seç butonunu gizle
            purchaseButtonText.text = characterPrices[characterIndex].ToString("F0") + " Coins"; // Satýn Al butonuna fiyat yazdýr
        }
    }

    // Karakter Satýn Alma Fonksiyonu
    // Karakter Satýn Alma Fonksiyonu
    public void PurchaseCharacter(int characterIndex)
    {
        PlayerPrefs.SetInt("CharacterPurchased_" + characterIndex, 1); // Karakteri satýn alýndý olarak iþaretle
        PlayerPrefs.SetInt("CharacterLocked_" + characterIndex, 0); // Karakterin kilidini açýk olarak iþaretle (0 = kilit açýk)

        UpdateButtons(characterIndex); // Butonlarý güncelle

        // Satýn alýnan karakterin LockedImage'ini gizle
        HideLockedImage(characterIndex);
    }

    // LockedImage'i Gizleme Fonksiyonu
    private void HideLockedImage(int characterIndex)
    {
        // Karakterin ilgili butonunu al
        Transform characterButton = catalogCharacterButtons[characterIndex].transform;

        // LockedImage bileþenini bul ve gizle
        Transform lockedImageTransform = characterButton.Find("Locked");

        if (lockedImageTransform != null)
        {
            lockedImageTransform.gameObject.SetActive(false); // LockedImage'i gizle
        }
        else
        {
            Debug.LogWarning("LockedImage not found for character " + characterIndex);
        }
    }

    // Oyun baþladýðýnda satýn alýnan karakterlerin LockedImage'lerini kontrol et
    public void CheckAndUpdateLockedImages()
    {
        for (int i = 0; i < catalogCharacterButtons.Length; i++)
        {
            // Eðer karakter satýn alýndýysa (kilidi açýk olarak iþaretlendiyse)
            if (PlayerPrefs.GetInt("CharacterLocked_" + i, 1) == 0) // 1 = kilitli, 0 = kilit açýk
            {
                HideLockedImage(i); // Karakterin LockedImage'ini gizle
            }
        }
    }

    // Karakter Seçme Fonksiyonu
    public void SelectCharacter(int characterIndex)
    {
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex); // Seçilen karakteri kaydet
        PlayerPrefs.SetString("LastEquippedCharacter", characterPrefabs[characterIndex].name); // Karakterin prefab adýný kaydet
        PlayerPrefs.SetInt("LastEquippedCharacterIndex", characterIndex); // En son seçilen karakterin index'ini kaydet
        selectButton.gameObject.SetActive(false); // Seç butonunu gizle
        UpdateCatalogUI(characterIndex); // Katalogda tick iþaretini güncelle
    }

    // Katalogdaki Tick Ýþaretini Güncelleme Fonksiyonu
    public void UpdateCatalogUI(int selectedCharacterIndex)
    {
        // Tüm butonlardaki tick iþaretlerini gizle
        for (int i = 0; i < catalogCharacterButtons.Length; i++)
        {
            Transform tickTransform = catalogCharacterButtons[i].transform.Find("Tick");

            if (tickTransform != null)
            {
                tickTransform.gameObject.SetActive(false); // Tick iþaretini gizle
            }
            else
            {
                Debug.LogWarning("Tick object not found on button " + i);
            }
        }

        // Seçilen karakterin butonuna tick iþaretini göster
        Transform selectedTickTransform = catalogCharacterButtons[selectedCharacterIndex].transform.Find("Tick");

        if (selectedTickTransform != null)
        {
            selectedTickTransform.gameObject.SetActive(true); // Seçilen karakterin tick iþaretini göster
        }
        else
        {
            Debug.LogWarning("Tick object not found on selected button");
        }
    }

    // Ýlk Karakteri Ücretsiz Sunma Fonksiyonu
    public void OfferFirstCharacterForFree()
    {
        // Hiçbir karakter satýn alýnmamýþsa
        bool anyCharacterPurchased = false;

        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            if (PlayerPrefs.GetInt("CharacterPurchased_" + i) == 1)
            {
                anyCharacterPurchased = true;
                break;
            }
        }

        // Eðer hiçbir karakter satýn alýnmamýþsa
        if (!anyCharacterPurchased)
        {
            PlayerPrefs.SetInt("CharacterPurchased_0", 1); // Ýlk karakteri ücretsiz olarak satýn alýnmýþ gibi iþaretle
            PlayerPrefs.SetInt("SelectedCharacter", 0); // Ýlk karakteri seçili olarak ayarla
            PlayerPrefs.SetInt("LastEquippedCharacter", 0); // Ýlk karakteri son kullanýlan karakter olarak ayarla
            PlayerPrefs.SetInt("defaultCharacter", 0); // Ýlk karakteri varsayýlan karakter olarak ayarla
            UpdateCatalogUI(0); // Katalogdaki tick iþaretini güncelle
        }
    }

    // Oyun baþlarken çalýþtýrýlacak fonksiyon
    
}
