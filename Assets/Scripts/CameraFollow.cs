using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // Kamera ile karakter arasýndaki mesafe
    public float xrotation = 66.4f; // Sabit X rotasyonu
    public float yrotation = 102.4f;
    public float zrotation = 0f;

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
            transform.eulerAngles = new Vector3(xrotation, yrotation, zrotation);
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
                transform.eulerAngles = new Vector3(xrotation, yrotation, zrotation);

                followStarted = true; // Takip sistemini baþlat
                break;
            }
            else
            {
                Debug.Log("Player not found in the scene");
            }
        }

        if (target == null)
        {
            Debug.LogError("Target not found. Ensure that the local player's character is tagged correctly as 'Player'.");
        }
    }
}
