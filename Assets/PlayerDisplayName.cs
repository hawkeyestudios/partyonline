using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDisplayName : MonoBehaviour
{
    public TMP_Text playerDisplayName;
    // Start is called before the first frame update
    void Start()
    {
        string displayName = PlayerPrefs.GetString("DISPLAYNAME", "Guest");
        playerDisplayName.text = displayName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
