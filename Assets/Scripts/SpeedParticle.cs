using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedParticle : MonoBehaviour
{
    public GameObject speedParticle;
    public ParticleSystem boomEffect;
    void Start()
    {
        boomEffect.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            boomEffect.Play();
            Destroy(speedParticle, 0.3f);
        }
    }
}
