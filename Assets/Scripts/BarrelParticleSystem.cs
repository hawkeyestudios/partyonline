using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelParticleSystem : MonoBehaviour
{
    public ParticleSystem particle;

    void Awake()
    {
        if (particle != null)
        {
            particle.Stop();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!particle.isPlaying)
            {
                particle.Play();
                Destroy(gameObject, 1f);
            }

        }
    }
}
