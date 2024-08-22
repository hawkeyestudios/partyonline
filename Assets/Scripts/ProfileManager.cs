using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] private Text usernameDisplayText;
    [SerializeField] private Text profilenameDisplayText;
    [SerializeField] private Text requestnameDisplayText;
    [SerializeField] private Text profileNameText;

    void Start()
    {
        if (PlayerPrefs.HasKey("DISPLAYNAME"))
        {
            string displayName = PlayerPrefs.GetString("DISPLAYNAME");
            UpdateUsernameDisplay(displayName);
            UpdateProfilenameDisplay(displayName);
        }
        else
        {
            Debug.LogWarning("No DisplayName found in PlayerPrefs.");
            // Gerekirse kullanýcýyý PlayFab'den DisplayName'i yeniden almasý için yönlendirin
        }
    }



    private void UpdateUsernameDisplay(string username)
    {
        if (usernameDisplayText != null)
        {
            usernameDisplayText.text = username;
            profileNameText.text = username;
        }
    }

    private void UpdateProfilenameDisplay(string username)
    {
        if (profilenameDisplayText != null)
        {
            profilenameDisplayText.text = username;
            requestnameDisplayText.text = username;
        }
    }
}
