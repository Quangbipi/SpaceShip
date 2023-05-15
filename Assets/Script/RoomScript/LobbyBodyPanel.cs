using Assets.Script;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using System.ComponentModel;

public class LobbyBodyPanel : MonoBehaviourPunCallbacks
{
    [Header("Login Panel")]
    public GameObject LoginPanel;
    public InputField PlayerNameInput;
    public GameObject LoaderUI;
    public Slider progressSlider;
    public GameObject Fill;
    public Gradient gradientColor;

    [Header("Selection Panel")]
    public GameObject SelectionPanel;

    [Header("Create Room Panel")]
    public GameObject CreateRoomPanel;

    public InputField RoomNameInputField;
    public InputField MaxPlayersInputField;

    [Header("Join Random Room Panel")]
    public GameObject JoinRandomRoomPanel;

    [Header("Room List Panel")]
    public GameObject RoomListPanel;

    public GameObject RoomListContent;
    public GameObject RoomListEntryPrefab;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomPanel;

    [Header("Map List Panel")]
    public GameObject MapListPanel;

    [Header("Mode Panel")]
    public GameObject ModePanel;

    [Header("Single Panel")]
    public GameObject SinglePanel;

    [Header("SettingPanel")]
    public GameObject MusicSettingPanel;
    public Slider musicSlider;
    public GameObject AboutPanel;
    

    public Button StartGameButton;
    public GameObject PlayerListEntryPrefab;

    // Luu tru phong 
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;
    private bool hasLeftRoom = false;
    private bool single = false;
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

        PlayerNameInput.text = "Player " + Random.Range(1000, 10000);
    }
    void Start()
    {
        LoadValues();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Photon Callback

    public override void OnConnectedToMaster()
    {
        if (hasLeftRoom)
        {
            return;
        }
        Debug.Log("g?i");
        this.SetActivePanel(ModePanel.name);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }
    public override void OnJoinedLobby()
    {
        
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    
    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(SelectionPanel.name);
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetActivePanel(SelectionPanel.name);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = "Room " + Random.Range(1000, 10000);

        RoomOptions options = new RoomOptions { MaxPlayers = 8 };

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        
        cachedRoomList.Clear();


        SetActivePanel(InsideRoomPanel.name);

        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            // khoi tao item list
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(InsideRoomPanel.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerListItem>().Initialize(p.ActorNumber, p.NickName);
            //cap nhat trang thai
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(SpaceShip.PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<PlayerListItem>().SetPlayerReady((bool)isPlayerReady);
            }

            playerListEntries.Add(p.ActorNumber, entry);
        }

        StartGameButton.gameObject.SetActive(CheckPlayersReady());

        Hashtable props = new Hashtable
            {
                {SpaceShip.PLAYER_LOADED_LEVEL, false}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

    }
    // roi room
    public override void OnLeftRoom()
    {
        hasLeftRoom = true;

        SetActivePanel(single==true ? SinglePanel.name: SelectionPanel.name);

        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
        
    }
    // nguoi khac tham gia room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(PlayerListEntryPrefab);
        entry.transform.SetParent(InsideRoomPanel.transform);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<PlayerListItem>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);

        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }
    // xoa data khi roi room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);

        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }
    // check chu phong ms
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            object isPlayerReady;
            if (changedProps.TryGetValue(SpaceShip.PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<PlayerListItem>().SetPlayerReady((bool)isPlayerReady);
            }
        }

        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SetActivePanel(LoginPanel.name);
    }



    #endregion

    #region UI Callback


    // click button login
    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            //PhotonNetwork.ConnectUsingSettings();
            StartCoroutine(ConnectPhoton());
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    public void OnBackLoginScene()
    {
        PhotonNetwork.Disconnect();
    }
    IEnumerator ConnectPhoton()
    {
        // Hi?n th? LoaderUI
        SetActivePanel(LoaderUI.name);

        // ??t gi� tr? ban ??u cho progressSlider
        progressSlider.value = 0f;

        // K?t n?i v?i Photon
        PhotonNetwork.ConnectUsingSettings();

        float progress = 0f;
        // Ch? k?t n?i th�nh c�ng ho?c th?t b?i
#pragma warning disable 0618
        while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterserver && PhotonNetwork.NetworkClientState != ClientState.Disconnected)
        {
            // C?p nh?t gi� tr? c?a progressSlider
            progress = Mathf.MoveTowards(progress, (float)PhotonNetwork.NetworkingClient.LoadBalancingPeer.BytesOut / (float)PhotonNetwork.NetworkingClient.LoadBalancingPeer.BytesIn, Time.deltaTime); 
            progressSlider.value = progress;
            Debug.Log(progressSlider.value);
            
            yield return null;
        }

        // N?u k?t n?i th�nh c�ng, ?n LoaderUI
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterserver)
        {
            SetActivePanel(ModePanel.name);
        }
        else
        {
            Debug.LogError("Failed to connect to Photon: " + PhotonNetwork.NetworkClientState.ToString());
        }
#pragma warning restore 0618
    }
    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        
        
    }

    // click button join random room
    public void OnJoinRandomRoomButtonClicked()
    {
        SetActivePanel(JoinRandomRoomPanel.name);
        // goi method cua photon
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        SetActivePanel(RoomListPanel.name);
    }

    public void OnMapListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        SetActivePanel(MapListPanel.name);
    }

    public void OnMultiButtonClicked()
    {
        single = false;
        PlayerPrefs.SetInt("SingleScene", 0);
        SetActivePanel(SelectionPanel.name);
    }

    public void OnSingleButtonClicked()
    {
        single=true;
        PlayerPrefs.SetInt("SingleScene", 1);
        SetActivePanel(SinglePanel.name);
    }
    // leave room
    public void OnBackButtonClicked()
    {
        // nguoi choi trong room
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        // nguoi choi khong o trong room
        SetActivePanel(single == true ? SinglePanel.name : SelectionPanel.name);
    }

    public void OnSaveButtonClicked()
    {
        float volumeValue = musicSlider.value;
        PlayerPrefs.SetFloat("VolumeValue", volumeValue);
        LoadValues();
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        // nguoi choi khong o trong room
        SetActivePanel(single == true ? SinglePanel.name : SelectionPanel.name);

    }

    public void OnAboutButtonClicked()
    {
        SetActivePanel(AboutPanel.name);
    }

    void LoadValues()
    {
        float volumeValue = PlayerPrefs.GetFloat("VolumeValue"); 
        musicSlider.value = volumeValue; 
        
    }
    public void OnCreateRoomButtonClicked()
    {
        string roomName = RoomNameInputField.text;
        roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

        byte maxPlayers;
        byte.TryParse(MaxPlayersInputField.text, out maxPlayers);
        maxPlayers = (byte)Mathf.Clamp(maxPlayers, 2, 8);

        // thiet lap phong: so luong nguoi, thoi gian gioi h?n cho nguoi choi 
        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000 };

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public void OnButtonSinglePlayerClicked()
    {
        string roomName = "Room " + Random.Range(1000, 10000);

        byte maxPlayers = 1;
        
        // thiet lap phong: so luong nguoi, thoi gian gioi h?n cho nguoi choi 
        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000 };

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public void OnButtonSettingClicked()
    {
        SetActivePanel(MusicSettingPanel.name);
    }
    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("Game Scene");
    }

    // tho�t game
    public void OnExitGameButtonClicked()
    {
        Application.Quit();
        Debug.Log("tho�t game");
    }

    public void OnBackMode()
    {
        SetActivePanel(ModePanel.name);
    }
    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(SpaceShip.PLAYER_READY, out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    #endregion
    // set cac thanh phan, tat mo cac panel
    public void SetActivePanel(string activePanel)
    {
        LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
        SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));
        CreateRoomPanel.SetActive(activePanel.Equals(CreateRoomPanel.name)); 
        JoinRandomRoomPanel.SetActive(activePanel.Equals(JoinRandomRoomPanel.name));
        RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name));    
        InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
        MapListPanel.SetActive(activePanel.Equals(MapListPanel.name));
        ModePanel.SetActive(activePanel.Equals(ModePanel.name));
        SinglePanel.SetActive(activePanel.Equals(SinglePanel.name));
        LoaderUI.SetActive(activePanel.Equals(LoaderUI.name));
        MusicSettingPanel.SetActive(activePanel.Equals(MusicSettingPanel.name));
        AboutPanel.SetActive(activePanel.Equals(AboutPanel.name));
    }

    public void LocalPlayerPropertiesUpdated()
    {
        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // xoa phong khoi bo nho neu phong bi dong hoac da xoa
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Cap nhap phong
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // them phong moi vao bo nho
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    
    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab);
            entry.transform.SetParent(RoomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListItem>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }
}
