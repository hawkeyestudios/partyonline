using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using Photon.Pun;

public class LogoutManager : MonoBehaviour
{
    public void Logout()
    {
        // PlayFab'den çýkýþ yapma
        PlayFabClientAPI.ForgetAllCredentials();

        // Photon'dan çýkýþ yapma
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        // Kullanýcý verilerini temizleme
        PlayerPrefs.DeleteAll();

        // Giriþ ekranýna yönlendirme
        SceneManager.LoadScene("Login"); // LoginScene adýndaki sahneye yönlendiriyor
    }
}
