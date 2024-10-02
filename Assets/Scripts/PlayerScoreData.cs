using UnityEngine;

[CreateAssetMenu(fileName = "PlayerScoreData", menuName = "PlayerScoreSystem/PlayerScoreData", order = 1)]
public class PlayerScoreData : ScriptableObject
{
    public float totalScore;

    public void AddScore(float score)
    {
        totalScore += score;
    }

    public void ResetScore()
    {
        totalScore = 0f;
    }
}
