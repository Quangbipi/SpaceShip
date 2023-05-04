using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void BackToLobby()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            PhotonNetwork.LoadLevel("SampleScene");
        }
    }

    // X? lý s? ki?n r?i phòng thành công
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("SampleScene");
    }
}
