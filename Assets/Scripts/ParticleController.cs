using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem particleSystem;

    void Awake()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop(); // Baþlangýçta partikül sistemini durdurur
        }
    }
}
