using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
public class PlayerController : MonoBehaviourPun
{
    public GameObject ghostPrefab;

    public void BecomeGhost()
    {
        Vector3 currentPosition = transform.position;  // Mevcut pozisyonu kaydet
        Quaternion currentRotation = transform.rotation;

        // Hayalet prefabýný instantiate et
        GameObject ghost = PhotonNetwork.Instantiate(ghostPrefab.name, currentPosition, currentRotation);

        // Mevcut oyuncu karakterini yok et
        PhotonNetwork.Destroy(gameObject);
    }
}

