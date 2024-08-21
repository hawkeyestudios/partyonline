using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public GameObject friendSystemPanel;
    public Animator FriendSystemanim;
    public GameObject addfriendPanel;
    public Animator addfriendPanelanim;
    public GameObject friendRequestPanel;
    public Animator friendRequestPanelanim;
    public GameObject friendsPanel;
    public Animator friendsPanelanim;
    public GameObject notificationPanel;
    public GameObject settingsPanel;
    public GameObject gamemodepanel;
    public GameObject signuppanel;
    public GameObject loginpanel;
    public GameObject CoinStore;
    public GameObject CharacterStore;
    public GameObject PartyPass;
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
    private void Start()
    {
        characterImage.color = blueColor;
    }
    public static void LoadScene(string name)
    { 
        SceneManager.LoadScene(name);
    }

    public void OpenFriendSystem()
    {
        FriendSystemanim.SetTrigger("FriendSystem");
        friendSystemPanel.SetActive(true);
    }
    public void OpenAddFriend()
    {
        addfriendPanel.SetActive(true);
        addfriendPanelanim.SetTrigger("AddFriend");
    }
    public void OpenRequestFriend()
    {
        friendRequestPanel.SetActive(true);
        friendRequestPanelanim.SetTrigger("FriendRequest");
    }

    public void OpenFriends()
    {
        friendsPanel.SetActive(true);
        friendsPanelanim.SetTrigger("Friends");
    }

    public void CloseFriendSystem()
    {
        FriendSystemanim.SetTrigger("FriendSystemClose");
    }
    public void CloseAddFriend()
    {
        addfriendPanelanim.SetTrigger("AddFriendClose");
    }
    public void CloseRequestFriend()
    {
        friendRequestPanelanim.SetTrigger("FriendRequestClose");
    }
    public void CloseFriends()
    {
        friendsPanelanim.SetTrigger("FriendsClose");
    }

    public void NotificationsPanel()
    {
        notificationPanel.SetActive(true);
    }
    public void CloseNotificationsPanel()
    {
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
    }
    public void ClosePartyPass()
    {
        PartyPass.SetActive(false);
    }
    public void CharacterStoreScene()
    {
        SceneManager.LoadScene("CharacterShop");
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
