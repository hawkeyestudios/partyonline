using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpinWheel : MonoBehaviour
{
    public Transform wheelTransform; 
    public float spinDuration = 8f; 
    public Button spinButton; 
    public Text rewardText; 

    private float finalAngle; 
    private bool isSpinning = false;

    private string[] rewards = { "100 Coin", "10 Gem", "500 Coin", "5 Gem", "Nothing", "1000 Coin", "1 Gem", "2500 Coin" };

    void Start()
    {
        spinButton.onClick.AddListener(StartSpin);
    }

    public void StartSpin()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            spinButton.interactable = false;
            StartCoroutine(Spin());
        }
    }

    private IEnumerator Spin()
    {
        float timeElapsed = 0f;
        float startAngle = wheelTransform.eulerAngles.z;
        finalAngle = Random.Range(0f, 360f); 
        float totalAngle = 360 * Random.Range(3, 6) + finalAngle; 

        float maxSpeed = 1000f;
        float minSpeed = 50f;

        float accelerationDuration = spinDuration * 0.3f;
        float decelerationDuration = spinDuration * 0.7f; 

        while (timeElapsed < spinDuration)
        {
            float angle = 0f;
            float speed = 0f;

            if (timeElapsed < accelerationDuration)
            {
                float t = timeElapsed / accelerationDuration;
                speed = Mathf.Lerp(0, maxSpeed, Mathf.SmoothStep(0, 1, t));
            }
            else
            {

                float t = (timeElapsed - accelerationDuration) / decelerationDuration;
                speed = Mathf.Lerp(maxSpeed, minSpeed, Mathf.SmoothStep(0, 1, t)); 
                speed = Mathf.Clamp(speed, 0, maxSpeed);
            }

            angle = startAngle + speed * timeElapsed;

            wheelTransform.eulerAngles = new Vector3(0, 0, angle % 360);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        int rewardIndex = GetRewardIndex(wheelTransform.eulerAngles.z);
        string wonReward = rewards[rewardIndex];
        rewardText.text = "Kazandýn: " + wonReward;

        GiveReward(wonReward);

        isSpinning = false;
        spinButton.interactable = true;
    }

    private int GetRewardIndex(float angle)
    {
        angle = angle % 360f;

        if (angle < 0)
        {
            angle += 360f;
        }
        angle = (angle + 22.5f) % 360f;

        float segmentSize = 360f / rewards.Length;
        int index = Mathf.FloorToInt(angle / segmentSize);

        return index;
    }

    private void GiveReward(string reward)
    {
        switch (reward)
        {
            case "100 Coin":
                break;
            case "10 Gem":
                break;
            case "500 Coin":
                break;
            case "5 Gem":
                break;
            case "Nothing":
                break;
            case "1000 Coin":
                break;
            case "1 Gem":
                break;
            case "2500 Coin":
                break;
        }
    }
}
