using UnityEngine;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

public class ButtonManager : MonoBehaviour
{
    public List<CharacterButton> characterButtons;

    private void Start()
    {
        string lastEquippedCharacter = PlayerPrefs.GetString("LastEquippedCharacter", "Yaþlý");
        CharacterButton lastEquippedButton = characterButtons.Find(button => button.characterPrefabName == lastEquippedCharacter);
        if (lastEquippedButton != null)
        {
            lastEquippedButton.SpawnCharacter();
            lastEquippedButton.IsEquipped = true;
        }
    }

    public void UpdateActiveButton(CharacterButton activeButton)
    {
        foreach (CharacterButton button in characterButtons)
        {
            button.buyButton.gameObject.SetActive(button == activeButton);
            button.gemButton.gameObject.SetActive(button == activeButton);
        }
    }

    public void UpdateEquipButtons(CharacterButton activeButton)
    {
        foreach (CharacterButton button in characterButtons)
        {
            if (button != activeButton)
            {
                if (button.tickImage != null)
                {
                    button.tickImage.gameObject.SetActive(false);
                }
            }
        }

        if (activeButton.tickImage != null)
        {
            activeButton.tickImage.gameObject.SetActive(true);
        }
        PlayerPrefs.Save();
    }

    public void SetLastEquippedCharacter(CharacterButton characterButton)
    {
        PlayerPrefs.SetString("LastEquippedCharacter", characterButton.characterPrefabName);

        // tickImage durumunu PlayerPrefs'e kaydet (örneðin, true olarak ayarlanmýþsa)
        bool tickImageState = true; // Örneðin, tickImage aktifse
        PlayerPrefs.SetInt("TickImageState", tickImageState ? 1 : 0);

        // PlayFab'e veri gönderme
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "LastEquippedCharacter", characterButton.characterPrefabName },
            { "TickImageState", tickImageState ? "1" : "0" } // tickImage durumunu string olarak gönderiyoruz
        }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("Character and tickImage data sent to PlayFab successfully!"),
            error => Debug.LogError("Error sending data to PlayFab: " + error.GenerateErrorReport())
        );

        PlayerPrefs.Save();
    }
}
