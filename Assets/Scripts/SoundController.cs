using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioClip soundClip; // Unity'de atayacaðýnýz ses dosyasý
    public float maxVolume = 1.0f; // Maksimum ses þiddeti
    public float fadeInDuration = 5.0f; // Sesin maxVolume kadar yükselmesi için geçen süre

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = soundClip;
        audioSource.volume = 0; // Baþlangýçta sesi kapalý olarak baþlatýyoruz
        audioSource.Play(); // Sesi çalmaya baþlatýyoruz
    }

    void Update()
    {
        if (audioSource.volume < maxVolume)
        {
            // Ses þiddetini zamanla artýrma
            audioSource.volume += Time.deltaTime / fadeInDuration * maxVolume;
        }
        else
        {
            audioSource.volume = maxVolume; // Maksimum ses þiddetini sabitler
        }
    }
}
