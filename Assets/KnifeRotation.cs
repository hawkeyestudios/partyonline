using UnityEngine;

public class KnifeRotation : MonoBehaviour
{
    public float rotationSpeed = 100f; // Dönüþ hýzý (derece/saniye)

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime,0, 0);
    }
}
