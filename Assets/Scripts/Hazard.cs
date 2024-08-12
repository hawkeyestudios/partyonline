using UnityEngine;
using System.Collections;

public class Hazard : MonoBehaviour
{
    private static bool isPlayerDead = false;
    public Transform deadPoint; // Dead point transform
    public GameObject gameUI;
    public Joystick joystick;
    public float deathAnimationCooldown = 3f; // Animasyonun tekrar çalýþabileceði süre

    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerDead) // Oyuncu ile temas ve animasyon tekrar çalýþabilir durumda
        {
            StartCoroutine(HandlePlayerDeath(other.gameObject));
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

    // Bu methodu static olarak tanýmlýyoruz
    public static bool IsPlayerDead()
    {
        return isPlayerDead;
    }
}
