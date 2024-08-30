using UnityEngine;
using Photon.Pun;

public class SwingingAxe : MonoBehaviourPun, IPunObservable
{
    public float swingAngle = 30f; // Balta saða ve sola ne kadar dönecek
    public float swingSpeed = 2f; // Baltanýn döneceði hýz

    private float startAngle;
    private float currentAngle;

    private float networkAngle;

    void Start()
    {
        // Baþlangýç açýsýný kaydet
        startAngle = transform.eulerAngles.z;
        currentAngle = startAngle;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            Swing();
        }
        else
        {
            // Diðer oyuncular için senkronize bir þekilde açý deðiþimini uygula
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(currentAngle, networkAngle, Time.deltaTime * swingSpeed));
            currentAngle = transform.eulerAngles.z;
        }
    }

    private void Swing()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0, 0, startAngle + angle);
        currentAngle = startAngle + angle;
    }

    // Photon'un senkronizasyon metodunu kullanarak açýyý güncelleriz
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Eðer bu objenin sahibi ise, açýyý diðer oyunculara gönder
            stream.SendNext(currentAngle);
        }
        else
        {
            // Eðer bu objenin sahibi deðilse, açýyý al ve güncelle
            networkAngle = (float)stream.ReceiveNext();
        }
    }
}
