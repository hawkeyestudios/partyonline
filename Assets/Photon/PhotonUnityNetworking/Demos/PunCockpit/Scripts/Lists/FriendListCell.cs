using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

namespace Photon.Pun.Demo.Cockpit
{
    /// <summary>
    /// Friend list cell
    /// </summary>
    public class FriendListCell : MonoBehaviour
    {
        public Text NameText;
        public GameObject OnlineFlag;
        public GameObject inRoomText;
        public GameObject JoinButton;

        private FriendInfo _info;
        private FriendListView listManager;

        private void Awake()
        {
            // Parent objesinden FriendListView bileşenini bulma
            listManager = GetComponentInParent<FriendListView>();
        }

        public void RefreshInfo(FriendListView.FriendDetail details)
        {
            NameText.text = details.NickName;

            OnlineFlag.SetActive(false);
            inRoomText.SetActive(false);
            JoinButton.SetActive(false);
        }

        public void RefreshInfo(FriendInfo info)
        {
            _info = info;

            OnlineFlag.SetActive(_info.IsOnline);
            inRoomText.SetActive(_info.IsInRoom);
            JoinButton.SetActive(_info.IsInRoom);
        }

        public void JoinFriendRoom()
        {
            if (listManager != null && _info != null)
            {
                listManager.JoinFriendRoom(_info.Room);
            }
        }

        public void RemoveFromList()
        {
            Destroy(this.gameObject);
        }
    }
}
