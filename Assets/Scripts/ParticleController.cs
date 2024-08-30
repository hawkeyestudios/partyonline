using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem particle;

    void Awake()
    {
        if (particle != null)
        {
            particle.Stop(); 
        }
    }
}
