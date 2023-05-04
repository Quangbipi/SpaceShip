using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeaderPanel : MonoBehaviour
{
    private readonly string connectionStatusMessage = "\t\t\t\t\t\t\t\t\t\t\tConnection Status: ";

    [Header("UI References")]
    public Text ConnectionStatusText;

    #region UNITY

    public void Update()
    {
        ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
    }

    #endregion
}
