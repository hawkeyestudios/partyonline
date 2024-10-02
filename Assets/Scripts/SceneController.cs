using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using Photon.Chat.Demo;

public class SceneController : MonoBehaviour
{
    // Panel ve animasyonlar
    public GameObject friendSystemPanel;
    public Animator FriendSystemanim;
    public GameObject addfriendPanel;
    public Animator addfriendPanelanim;
    public GameObject friendRequestPanelUI;
    public Animator friendRequestPanelanim;
    public GameObject friendsPanel;
    public Animator friendsPanelanim;
    public GameObject profilePanel;
    public Animator profilePanelAnim;
    public GameObject notificationPanel;
    public Animator notificationsAnim;
    public GameObject settingsPanel;
    public GameObject gamemodepanel;
    public GameObject signuppanel;
    public GameObject loginpanel;
    public GameObject CoinStore;
    public GameObject CharacterStore;
    public GameObject PartyPass;
    public Animator PartyPassAnim;
    public GameObject characterpanel;
    public GameObject wingspanel;
    public GameObject auraspanel;
    public GameObject itemspanel;
    public Image characterImage;
    public Image wingsImage;
    public Image aurasImage;
    public Image itemsImage;
    public Color blueColor;
    public Color whiteColor;


    public GameObject friendRequestPanel;    // Arkadaşlık isteği paneli
    public Text friendRequestText;           // Arkadaşlık isteği mesajı
    public Button acceptFriendButton;        // Kabul etme butonu
    public Button rejectFriendButton;        // Reddetme butonu
    public InputField addFriendInputField;   // Arkadaş eklemek için kullanıcı adı girişi
    public Button addFriendButton;           // Arkadaş ekleme butonu
    private string pendingFriendPlayFabId;   // Gelen arkadaşlık isteğinin PlayFab ID'si
    public FriendManager friendManager;     // FriendManager scriptine referans
    public GameObject friendRequestPrefab; // Arkadaşlık isteği için Prefab referansı
    public Transform requestListParent; // İsteklerin yerleşeceği UI alanı (örneğin bir ScrollView içi)
    public GameObject friendItemPrefab; // Prefab bağlantısı
    public Transform friendListContainer; // Arkadaş listesi konteyneri

    private void Start()
    {
        characterImage.color = blueColor;

        acceptFriendButton.onClick.AddListener(() => AcceptFriendRequest(pendingFriendPlayFabId));
        rejectFriendButton.onClick.AddListener(() => RejectFriendRequest(pendingFriendPlayFabId));
        addFriendButton.onClick.AddListener(OnAddFriendButtonClicked);

    }
    private void Awake()
    {

       // LoadFriendList();
        //CheckForFriendRequestsOnStartup();
    }


    // Scene yüklenecek metot
    public static void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    
    // Arkadaş ekleme butonuna basıldığında çağrılır
    public void OnAddFriendButtonClicked()
    {
        if (addFriendInputField == null)
        {
            Debug.LogError("addFriendInputField is not assigned.");
            return;
        }

        if (friendManager == null)
        {
            Debug.LogError("FriendManager is not assigned or found.");
            return;
        }

        string friendDisplayName = addFriendInputField.text;
        if (!string.IsNullOrEmpty(friendDisplayName))
        {
            friendManager.AddFriendByDisplayName(friendDisplayName);
            addFriendInputField.text = "";
        }
        else
        {
            Debug.Log("Lütfen bir arkadaş adı girin.");
        }
    }

    private void LoadFriendList()
    {
        if (friendManager != null)
        {
            Debug.Log("Arkadaş listesi yükleniyor...");
            friendManager.GetFriendsList(OnFriendListLoaded);
        }
        else
        {
            Debug.LogError("FriendManager bulunamadı.");
        }
    }

    private void OnFriendListLoaded(List<FriendInfo> friends)
    {
        if (friends != null)
        {
            Debug.Log(friends.Count + " arkadaş bulundu.");
            DisplayFriends(friends); // Arkadaş listesini UI'da göster
        }
        else
        {
            Debug.Log("Arkadaş listesi boş.");
        }
    }

    public void DisplayFriends(List<FriendInfo> friends)
    {
        Debug.Log("Arkadaşlar UI'ya ekleniyor...");

        // Önce mevcut arkadaş listesi UI öğelerini temizleyin
        foreach (Transform child in friendListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var friend in friends)
        {
            GameObject friendItem = Instantiate(friendItemPrefab, friendListContainer);
            Text friendNameText = friendItem.GetComponentInChildren<Text>();
            if (friendNameText != null)
            {
                friendNameText.text = friend.TitleDisplayName;
            }
        }

        Debug.Log("Arkadaşlar başarıyla eklendi.");
    }
    // Arkadaşlık isteklerini kontrol et ve UI'ya ekle
    private void DisplayPendingFriendRequests()
    {
        friendManager.GetFriendRequests();
    }
    private void OnPendingFriendRequestsLoaded(List<string> friendRequests)
    {
        if (friendRequests != null && friendRequests.Count > 0)
        {
            DisplayFriendRequests(friendRequests); // Arkadaşlık isteklerini UI'da göster
        }
        else
        {
            Debug.Log("Arkadaşlık isteği bulunamadı.");
        }
    }
    // Arkadaş listesi ve arkadaşlık isteklerini kontrol etme fonksiyonu
    public void CheckForFriendRequestsOnStartup()
    {
        GetPendingFriendRequests(OnPendingFriendRequestsLoaded); // OnPendingFriendRequestsLoaded callback olarak iletiliyor
    }
    public void GetPendingFriendRequests(System.Action<List<string>> callback)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetPendingFriendRequests" // Cloud Script fonksiyon adı
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                if (result.FunctionResult != null)
                {
                    var functionResult = result.FunctionResult as IDictionary<string, object>;
                    if (functionResult.ContainsKey("pendingRequests"))
                    {
                        var pendingRequests = functionResult["pendingRequests"] as List<object>;
                        List<string> friendRequests = new List<string>();

                        if (pendingRequests != null)
                        {
                            foreach (var requestObj in pendingRequests)
                            {
                                friendRequests.Add(requestObj.ToString());
                            }
                        }

                        callback(friendRequests);
                    }
                    else
                    {
                        Debug.LogError("Cloud Script'ten 'pendingRequests' bulunamadı.");
                        callback(null);
                    }
                }
                else
                {
                    Debug.LogError("Cloud Script sonucu boş.");
                    callback(null);
                }
            },
            error =>
            {
                Debug.LogError("Arkadaşlık istekleri alınamadı: " + error.GenerateErrorReport());
                callback(null);
            });
    }

    // Arkadaşlık isteklerini UI'da gösterir
    public void DisplayFriendRequests(List<string> friendRequests)
    {
        // Önce mevcut arkadaşlık isteklerini temizle
        foreach (Transform child in requestListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var request in friendRequests)
        {
            GameObject requestItem = Instantiate(friendRequestPrefab, requestListParent);
            Text requestText = requestItem.GetComponentInChildren<Text>();
            if (requestText != null)
            {
                requestText.text = $"Yeni arkadaşlık isteği: {request}";
                // İstek paneli tıklama olayları ekleyin
                Button requestButton = requestItem.GetComponentInChildren<Button>();
                if (requestButton != null)
                {
                    requestButton.onClick.AddListener(() => ShowFriendRequestPanel(request));
                }
            }
        }
    }

    // Arkadaşlık isteğini kabul et
    public void AcceptFriendRequest(string friendPlayFabId)
    {
        if (string.IsNullOrEmpty(friendPlayFabId))
        {
            Debug.LogError("Geçersiz PlayFab ID.");
            return;
        }

        friendManager.AcceptFriendRequest(friendPlayFabId);
        friendRequestPanel.SetActive(false);
        DisplayPendingFriendRequests(); // İstekleri güncelle
    }

    // Arkadaşlık isteği reddedilirse çağrılır
    public void RejectFriendRequest(string friendPlayFabId)
    {
        if (string.IsNullOrEmpty(friendPlayFabId))
        {
            Debug.LogError("Geçersiz PlayFab ID.");
            return;
        }

        friendManager.RejectFriendRequest(friendPlayFabId);
        friendRequestPanel.SetActive(false);
        DisplayPendingFriendRequests(); // İstekleri güncelle
    }

    // İsteği göstermek için ayarları yapar
    public void ShowFriendRequestPanel(string friendPlayFabId)
    {
        pendingFriendPlayFabId = friendPlayFabId;
        friendRequestPanel.SetActive(true);
    }

    // UI panelini gizlemek için (istek kabul veya reddedildikten sonra)
    public void HideFriendRequestPanel(GameObject friendRequestInstance)
    {
        Destroy(friendRequestInstance); // İlgili istek panelini yok et
    }
    

    // UI panel açma/kapama metotları
    public void OpenFriendSystem()
    {
        friendSystemPanel.SetActive(true);
        FriendSystemanim.SetTrigger("FriendSystem");
    }

    public void OpenAddFriend()
    {
        addfriendPanel.SetActive(true);
        addfriendPanelanim.SetTrigger("AddFriend");
    }

    public void OpenRequestFriend()
    {
        friendRequestPanelUI.SetActive(true);
        friendRequestPanelanim.SetTrigger("FriendRequest");
    }

    public void OpenFriends()
    {
        friendsPanel.SetActive(true);
        friendsPanelanim.SetTrigger("Friends");
    }

    public void OpenProfile()
    {
        profilePanel.SetActive(true);
        profilePanelAnim.SetTrigger("Profile");
    }

    public void CloseFriendSystem()
    {
        FriendSystemanim.SetTrigger("FriendSystemClose");
        friendSystemPanel.SetActive(false);
    }

    public void CloseAddFriend()
    {
        addfriendPanelanim.SetTrigger("AddFriendClose");
        addfriendPanel.SetActive(false);
    }

    public void CloseRequestFriend()
    {
        friendRequestPanelanim.SetTrigger("FriendRequestClose");
        friendRequestPanelUI.SetActive(false);
    }

    public void CloseFriends()
    {
        friendsPanelanim.SetTrigger("FriendsClose");
        friendsPanel.SetActive(false);
    }

    public void CloseProfile()
    {
        profilePanelAnim.SetTrigger("ProfileClose");
        profilePanel.SetActive(false);
    }

    public void NotificationsPanel()
    {
        notificationPanel.SetActive(true);
        notificationsAnim.SetTrigger("Notifications");
    }

    public void CloseNotificationsPanel()
    {
        notificationsAnim.SetTrigger("NotificationsClose");
        notificationPanel.SetActive(false);
    }

    public void SettingsPanel()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    public void GameModePanel()
    {
        gamemodepanel.SetActive(true);
    }

    public void CloseGameModePanel()
    {
        gamemodepanel.SetActive(false);
    }

    public void SignUpPanel()
    {
        signuppanel.SetActive(true);
        loginpanel.SetActive(false);
    }

    public void LoginPanel()
    {
        signuppanel.SetActive(false);
        loginpanel.SetActive(true);
    }

    public void OpenCoinStore()
    {
        CoinStore.SetActive(true);
    }

    public void CloseCoinStore()
    {
        CoinStore.SetActive(false);
    }

    public void OpenCharacterStore()
    {
        CharacterStore.SetActive(true);
    }

    public void CloseCharacterStore()
    {
        CharacterStore.SetActive(false);
    }

    public void OpenPartyPass()
    {
        PartyPass.SetActive(true);
        PartyPassAnim.SetTrigger("PartyPass");
    }

    public void ClosePartyPass()
    {
        PartyPassAnim.SetTrigger("PartyClose");
        PartyPass.SetActive(false);
    }

    public void CharacterStoreScene()
    {
        SceneManager.LoadScene("Customize");
    }

    public void OpenCharacters()
    {
        characterpanel.SetActive(true);
        wingspanel.SetActive(false);
        auraspanel.SetActive(false);
        itemspanel.SetActive(false);
        characterImage.color = blueColor;
        wingsImage.color = whiteColor;
        aurasImage.color = whiteColor;
        itemsImage.color = whiteColor;
    }

    public void OpenWings()
    {
        wingspanel.SetActive(true);
        characterpanel.SetActive(false);
        auraspanel.SetActive(false);
        itemspanel.SetActive(false);
        characterImage.color = whiteColor;
        wingsImage.color = blueColor;
        aurasImage.color = whiteColor;
        itemsImage.color = whiteColor;
    }

    public void OpenAuras()
    {
        auraspanel.SetActive(true);
        characterpanel.SetActive(false);
        wingspanel.SetActive(false);
        itemspanel.SetActive(false);
        characterImage.color = whiteColor;
        wingsImage.color = whiteColor;
        aurasImage.color = blueColor;
        itemsImage.color = whiteColor;
    }

    public void OpenItems()
    {
        itemspanel.SetActive(true);
        characterpanel.SetActive(false);
        wingspanel.SetActive(false);
        auraspanel.SetActive(false);
        characterImage.color = whiteColor;
        wingsImage.color = whiteColor;
        aurasImage.color = whiteColor;
        itemsImage.color = blueColor;
    }
}
