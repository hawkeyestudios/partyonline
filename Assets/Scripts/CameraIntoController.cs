using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraIntroController : MonoBehaviourPunCallbacks
{
    public Animator cameraAnimator;
    public GameObject gameUI;
    public float animationDuration = 5f;
    public CameraFollow cameraFollow;

    private float timer;
    private bool hasAnimationCompleted = false;

    void Start()
    {
        timer = 0f;

        if (PhotonNetwork.IsMasterClient)
        {
            if (cameraAnimator != null)
            {
                cameraAnimator.SetTrigger("StartGame");
            }
            photonView.RPC("StartAnimation_RPC", RpcTarget.OthersBuffered);
        }
    }

    [PunRPC]
    void StartAnimation_RPC()
    {
        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("StartGame");
        }
    }

    void Update()
    {
        if (!hasAnimationCompleted)
        {
            if (cameraAnimator.GetCurrentAnimatorStateInfo(0).IsName("TrapPGStartAnim"))
            {
                timer += Time.deltaTime;
                if (timer >= animationDuration)
                {
                    OnAnimationComplete();
                    hasAnimationCompleted = true;
                }
            }
        }
    }

    public void OnAnimationComplete()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("OnAnimationComplete_RPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void OnAnimationComplete_RPC()
    {
        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }
        else
        {
            Debug.LogError("gameUI is not assigned.");
        }

        if (cameraFollow != null)
        {
            cameraFollow.StartCameraFollow();
        }
        else
        {
            Debug.LogError("cameraFollow is not assigned.");
        }


        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGame_RPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void StartGame_RPC()
    {
        Debug.Log("Game Started");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("New MasterClient assigned: " + newMasterClient.NickName);

        if (!hasAnimationCompleted && PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Debug.Log("I am the new MasterClient, taking over responsibilities.");
            photonView.RPC("StartAnimation_RPC", RpcTarget.AllBuffered);
        }
    }
}
