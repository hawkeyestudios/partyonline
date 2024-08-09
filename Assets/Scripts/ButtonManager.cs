using UnityEngine;
using System.Collections.Generic;

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
    }

    public void SetLastEquippedCharacter(CharacterButton characterButton)
    {
        PlayerPrefs.SetString("LastEquippedCharacter", characterButton.characterPrefabName);
        PlayerPrefs.Save();
    }
}
