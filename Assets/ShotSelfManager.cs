using UnityEngine;
using UnityEngine.UI;

public class ShotSelfManager : MonoBehaviour
{
    public Button selfShootButton;
    public RouletteManager rouletteManager;
    void Start()
    {
        selfShootButton.onClick.AddListener(() => rouletteManager.OnSelfShootButtonPressed());
    }

    public void ToggleSelfShootButton(bool show)
    {
        selfShootButton.gameObject.SetActive(show);
    }
}
