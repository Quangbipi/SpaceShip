using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Assets.Script;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerOverViewPanell : MonoBehaviourPunCallbacks
{
    public GameObject PlayerOverviewEntryPrefab;

    private Dictionary<int, GameObject> playerListEntries;

    

    public void Awake()
    {
        playerListEntries = new Dictionary<int, GameObject>();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(PlayerOverviewEntryPrefab);
            entry.transform.SetParent(gameObject.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<Text>().color = SpaceShip.GetColor(p.GetPlayerNumber());
            entry.GetComponent<Text>().text = string.Format("{0}\nDiem: {1}\nMang: {2}", p.NickName, p.GetScore(), SpaceShip.PLAYER_MAX_LIVES);

            playerListEntries.Add(p.ActorNumber, entry);
        }
    }

   

    
    // xoa khi roi phong
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameObject go = null;
        if (this.playerListEntries.TryGetValue(otherPlayer.ActorNumber, out go))
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber]);
            playerListEntries.Remove(otherPlayer.ActorNumber);
        }
    }

    // cap nhat
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            entry.GetComponent<Text>().text = string.Format("{0}\nDiem: {1}\nMang: {2}", targetPlayer.NickName, targetPlayer.GetScore(), targetPlayer.CustomProperties[SpaceShip.PLAYER_LIVES]);
        }
    }

    
}
