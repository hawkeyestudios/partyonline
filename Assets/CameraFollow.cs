using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // Kamera ile karakter arasýndaki mesafe
    public float fixedXRotation = 36f; // Sabit X rotasyonu

    private Transform target;
    private bool followStarted = false; // Kamera takip sisteminin baþlatýlýp baþlatýlmadýðýný kontrol etmek için

    private void LateUpdate()
    {
        if (followStarted && target != null)
        {
            // Kamera hedefin arkasýnda ve yukarýsýnda bir mesafede olacak þekilde hesapla
            Vector3 desiredPosition = target.position + offset;

            // Kamerayý hedefin yeni pozisyonuna yumuþak bir þekilde kaydýr
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Kamerayý hedefe bakacak þekilde ayarla
            // Ancak X rotasyonunu sabit tut
            Vector3 currentRotation = transform.eulerAngles;
            transform.eulerAngles = new Vector3(fixedXRotation, currentRotation.y, currentRotation.z);
        }
    }

    public void StartCameraFollow()
    {
        // Sahnedeki tüm karakterleri kontrol et ve yerel oyuncuya ait olaný bul
        foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView photonView = playerObject.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                target = playerObject.transform;

                // Kamera'nýn X rotasyonunu sabit tut
                Vector3 currentRotation = transform.eulerAngles;
                transform.eulerAngles = new Vector3(fixedXRotation, currentRotation.y, currentRotation.z);

                followStarted = true; // Takip sistemini baþlat
                break;
            }
        }

        if (target == null)
        {
            Debug.LogError("Target not found. Ensure that the local player's character is tagged correctly as 'Player'.");
        }
    }
}
