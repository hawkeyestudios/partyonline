using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    public Animator mainmenucameraAnim;

    // Start is called before the first frame update
    void Awake()
    {
        mainmenucameraAnim.SetTrigger("MainMenuCamera");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
