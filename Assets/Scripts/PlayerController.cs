using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
public class PlayerController : MonoBehaviourPun
{
    public GameObject ghostPrefab;
    public void BecomeGhost()
    {
        Vector3 currentPosition = transform.position;  
        Quaternion currentRotation = transform.rotation;

        
        GameObject ghost = PhotonNetwork.Instantiate(ghostPrefab.name, currentPosition, currentRotation);

        
        PhotonNetwork.Destroy(gameObject);
    }
}

