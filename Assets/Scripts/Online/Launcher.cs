using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Photon Settings
    private const string playerNamePrefKey = "PlayerName";
    private byte maxPlayersPerRoom = 2;
    private string gameVersion = "1";
    private bool isConnecting;
    public InputField playerNameInputField;
    // Scene
    [SerializeField] private GameObject controlPanel = null;
    [SerializeField] private TextMeshProUGUI progressLabel = null;
    [SerializeField] private Button connectButton = null;
    [SerializeField] private InputField inputField;

    public static ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        ExitGames.Client.Photon.PhotonPeer.RegisterType(typeof(Card), (byte)'C', Card.Serialize, Card.Deserialize);

        connectButton.onClick.AddListener(Connect);
        PhotonNetwork.AutomaticallySyncScene = true;

        string defaultName = string.Empty;

        if (inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;

    }

    private void Connect()
    {
        progressLabel.gameObject.SetActive(true);
        controlPanel.SetActive(false);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public void SetPlayerName()
    {
        string value = playerNameInputField.text;
        value = value.Trim();

        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player name is null or empty.");
            return;
        }

        PhotonNetwork.NickName = value;
        PlayerPrefs.SetString(playerNamePrefKey, value);

        Debug.Log("Player name set to: " + value);
    }



    public void LoadLevel()
    {
        PhotonNetwork.LoadLevel("Main");
    }


    #region MonoBehaviourPunCallBacks Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master called by Pun");

        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
            isConnecting = false;

        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.gameObject.SetActive(false);
        controlPanel.SetActive(true);
        isConnecting = false;

        Debug.LogWarningFormat("OnDisconnected called with reason: {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No random room available.. creating");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom, CustomRoomProperties = customProperties });
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            LoadLevel();
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Client is now in room..");

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            progressLabel.text = "Waiting for another player..";
        }
    }


    #endregion

}
