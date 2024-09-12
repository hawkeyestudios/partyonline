using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using Photon.Pun;

public class LogoutManager : MonoBehaviour
{
    public void Logout()
    {
        PlayFabClientAPI.ForgetAllCredentials();

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Login"); 
    }
}
