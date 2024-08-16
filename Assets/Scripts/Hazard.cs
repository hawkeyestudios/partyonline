using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Hazard : MonoBehaviourPunCallbacks
{
    public Transform deadPoint; // Dead point transform
    public GameObject gameUI; // Bu UI, oyuncunun kendi UI'sý olmalý
    public Joystick joystick;
    public float deathAnimationCooldown = 3f; // Animasyonun tekrar çalýþabileceði süre

    private bool isPlayerDead = false; // Bu, her oyuncu için ayrý olarak yönetilecek

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerDead) // Oyuncu ile temas ve animasyon tekrar çalýþabilir durumda
        {
            // Bu RPC'yi çaðýrarak diðer oyunculara oyuncunun öldüðünü bildiriyoruz
            photonView.RPC("HandlePlayerDeathRPC", RpcTarget.All, other.gameObject.GetPhotonView().ViewID);
        }
    }

    [PunRPC]
    private void HandlePlayerDeathRPC(int playerViewID)
    {
        GameObject player = PhotonView.Find(playerViewID).gameObject;
        if (player != null && !isPlayerDead)
        {
            StartCoroutine(HandlePlayerDeath(player));
        }
    }

    private IEnumerator HandlePlayerDeath(GameObject player)
    {
        isPlayerDead = true;
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            gameUI.SetActive(false);
            animator.SetTrigger("Die");
            joystick.ResetHandlePosition();
        }

        // Oyuncunun altýnda BoomEffect adýnda bir ParticleSystem arayýn
        ParticleSystem boomEffect = player.GetComponentInChildren<ParticleSystem>();

        if (boomEffect != null)
        {
            boomEffect.Play(); // Partikülleri baþlat
        }
        else
        {
            Debug.LogError("BoomEffect ParticleSystem not found in the player!");
        }

        yield return new WaitForSeconds(2.16f); // Die animasyonunun süresi
        player.SetActive(false);

        player.transform.position = deadPoint.position;
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        gameUI.SetActive(true);
        player.SetActive(true);
        boomEffect.Stop();

        yield return new WaitForSeconds(deathAnimationCooldown - 2.16f); // Kalan süre için bekle

        isPlayerDead = false;
    }
}
