using UnityEngine;

public class SawMovement : MonoBehaviour
{
    public float moveDistance = 5f; // Testerenin hareket edeceði mesafe
    public float moveSpeed = 2f; // Testerenin hareket etme hýzý
    public float rotateSpeed = 100f; // Testerenin dönme hýzý
    public bool moveRightInitially = true; // Testerenin baþlangýçta saða mý yoksa sola mý hareket edeceðini belirler

    private Vector3 startPos;
    private bool isMoving = true;
    private bool isReturning = false;

    void Start()
    {
        startPos = transform.position; // Baþlangýç pozisyonunu kaydet
    }

    void Update()
    {
        RotateSaw();

        if (isMoving)
        {
            MoveSaw();
        }
        else if (isReturning)
        {
            ReturnSaw();
        }
    }

    private void MoveSaw()
    {
        // Testerenin saða mý yoksa sola mý hareket edeceðine göre hareket ettir
        if (moveRightInitially)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x >= startPos.x + moveDistance)
            {
                isMoving = false;
                isReturning = true;
            }
        }
        else
        {
            transform.position -= Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x <= startPos.x - moveDistance)
            {
                isMoving = false;
                isReturning = true;
            }
        }
    }

    private void ReturnSaw()
    {
        // Testerenin geri dönmesini saðla
        if (moveRightInitially)
        {
            transform.position -= Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x <= startPos.x)
            {
                isReturning = false;
                isMoving = true;
            }
        }
        else
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x >= startPos.x)
            {
                isReturning = false;
                isMoving = true;
            }
        }
    }

    private void RotateSaw()
    {
        // Testerenin dönmesini saðla
        transform.Rotate(Vector3.right * rotateSpeed * Time.deltaTime);
    }
}
