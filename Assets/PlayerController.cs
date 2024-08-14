using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
public class PlayerController : MonoBehaviourPun
{
    public GameObject ghostPrefab;

    public void BecomeGhost()
    {
        // Hayalet prefabýný instantiate edip mevcut karakteri yok etme
        GameObject ghost = PhotonNetwork.Instantiate(ghostPrefab.name, transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(gameObject); // Mevcut oyuncu karakterini yok et
    }
}
