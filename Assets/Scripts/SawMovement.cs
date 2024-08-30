using UnityEngine;
using Photon.Pun;

public class SawMovement : MonoBehaviourPun, IPunObservable
{
    public float moveDistance = 5f;
    public float moveSpeed = 2f; 
    public float rotateSpeed = 100f; 
    public bool moveRightInitially = true; 

    private Vector3 startPos;
    private bool isMoving = true;
    private bool isReturning = false;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Start()
    {
        startPos = transform.position; // Baþlangýç pozisyonunu kaydet
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    void Update()
    {
        if (photonView.IsMine)
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
        else
        {
            // Eðer bu testerenin sahibi deðilse, pozisyonu ve rotasyonu senkronize et
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * rotateSpeed);
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

    // Bu metot, testere pozisyonu ve rotasyonunu diðer oyunculara göndermek için kullanýlýr
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Eðer bu objenin sahibi ise, pozisyon ve rotasyon verilerini gönder
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Eðer bu objenin sahibi deðilse, pozisyon ve rotasyon verilerini al
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
