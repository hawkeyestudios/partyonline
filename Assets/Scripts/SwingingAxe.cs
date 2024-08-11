using UnityEngine;

public class SwingingAxe : MonoBehaviour
{
    public float swingAngle = 30f; // Balta saða ve sola ne kadar dönecek
    public float swingSpeed = 2f; // Baltanýn döneceði hýz

    private float startAngle;
    private bool swingingRight = true;

    void Start()
    {
        // Baþlangýç açýsýný kaydet
        startAngle = transform.eulerAngles.z;
    }

    void Update()
    {
        Swing();
    }

    private void Swing()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0, 0, startAngle + angle);
    }
}
