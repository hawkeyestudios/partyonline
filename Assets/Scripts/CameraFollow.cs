using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset; 
    public float xrotation = 66.4f;
    public float yrotation = 0f;
    public float zrotation = 0f;

    private Transform target;
    private bool followStarted = false; 

    private void LateUpdate()
    {

        if (followStarted && target != null)
        {
            Vector3 desiredPosition = target.position + offset;

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            Vector3 currentRotation = transform.eulerAngles;
            transform.eulerAngles = new Vector3(xrotation, yrotation, zrotation);
        }
    }

    public void StartCameraFollow()
    {
        foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView photonView = playerObject.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                target = playerObject.transform;

                Vector3 currentRotation = transform.eulerAngles;
                transform.eulerAngles = new Vector3(xrotation, yrotation, zrotation);

                followStarted = true;
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
