using UnityEngine;
using UnityEngine.UI;

public class FriendPrefabController : MonoBehaviour
{
    public Text friendNameText;
    public Button inviteButton;
    public Button removeButton;

    private string friendPlayFabId;

    private void Start()
    {
        inviteButton.onClick.AddListener(OnInviteClicked);
        removeButton.onClick.AddListener(OnRemoveClicked);
    }

    public void SetFriendName(string friendName)
    {
        friendNameText.text = friendName;
        // Optionally, set the PlayFabId or other unique identifier
    }

    private void OnInviteClicked()
    {
        // Implement lobby invite logic here
    }

    private void OnRemoveClicked()
    {
        // Implement remove friend logic here
    }
}
