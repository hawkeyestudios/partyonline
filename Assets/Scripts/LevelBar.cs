using UnityEngine;
using UnityEngine.UI;

public class LevelBar : MonoBehaviour
{
    public Image xpBar;
    public Text xpText;
    public Text levelText;
    public LevelData levelData;

    private void Start()
    {
        levelData.LoadLevelDataFromPlayFab();

    }
    private void Update()
    {
        UpdateXPBar();
    }

    private void UpdateXPBar()
    {

        int maxXP = levelData.xpRequirements[levelData.currentLevel - 1];

        float fillAmount = (float)levelData.currentXP / maxXP;
        xpBar.fillAmount = fillAmount;

        xpText.text = $"{levelData.currentXP} / {maxXP} XP";
        levelText.text = $"{levelData.currentLevel}";
    }
}
