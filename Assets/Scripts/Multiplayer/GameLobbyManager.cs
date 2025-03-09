using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class GameLobbyManager : MonoBehaviourPunCallbacks
{
    // UI References
    [SerializeField] private GameObject loobyscreen;
    [SerializeField] private GameObject gamescreen;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button contestButton;
    [SerializeField] private Button foldButton;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private TextMeshProUGUI player1Text;
    [SerializeField] private TextMeshProUGUI player2Text;
    [SerializeField] private TextMeshProUGUI player3Text;
    [SerializeField] private TextMeshProUGUI player4Text;

    // Game Variables
    private float lobbyCountdownTimer = 5f;
    private float gameTimer = 10f;
    private bool isCountingDown = false;
    private bool isGameRunning = false;

    private Dictionary<Player, int> contestedResults = new Dictionary<Player, int>();

    void Start()
    {
        // Initialize Photon
        PhotonNetwork.ConnectUsingSettings();

        // Set initial button states
       
        contestButton.interactable = false;
        foldButton.interactable = false;

        // Add button listeners
      
        contestButton.onClick.AddListener(OnContestClicked);
        foldButton.onClick.AddListener(OnFoldClicked);

        // Initialize UI
        loobyscreen.SetActive(true);
        gamescreen.SetActive(false);
        UpdatePlayerListUI();
    }

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        PhotonNetwork.LocalPlayer.NickName = "Player" + Random.Range(1, 1000);

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom("GameRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        AssignRandomNumber();
        UpdateStatusText();
        UpdatePlayerListUI();
        CheckPlayerCount();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateStatusText();
        UpdatePlayerListUI();
        CheckPlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateStatusText();
        UpdatePlayerListUI();
        CheckPlayerCount();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join Room Failed: {message}");
        statusText.text = "Failed to join room. Retrying...";
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion

    void Update()
    {
        if (isCountingDown && !isGameRunning)
        {
            lobbyCountdownTimer -= Time.deltaTime;
            UpdateStatusText();
            if (lobbyCountdownTimer <= 0)
            {
                StartGame();
            }
        }

        if (isGameRunning)
        {
            gameTimer -= Time.deltaTime;
            countdownText.text = $"Time Left: {Mathf.Ceil(gameTimer)}s";

            if (gameTimer <= 0)
            {
                EndGame();
            }
        }
    }

    #region Game Logic
    void CheckPlayerCount()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCount >= 2 && !isCountingDown && !isGameRunning)
        {
            StartCountdown();
        }
        else if (playerCount < 2)
        {
            isCountingDown = false;
            lobbyCountdownTimer = 15f;
        }

      
        contestButton.interactable = isGameRunning;
        foldButton.interactable = isGameRunning;
    }

    void StartCountdown()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isCountingDown = true;
            lobbyCountdownTimer = 15f;
        }
    }

    void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            photonView.RPC("RPC_StartGame", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_StartGame()
    {
        isCountingDown = false;
        isGameRunning = true;
        loobyscreen.SetActive(false);
        gamescreen.SetActive(true);
        gameTimer = 10f;
        contestButton.interactable = true;
        foldButton.interactable = true;
        statusText.text = "Game in Progress!";
        countdownText.text = $"Time Left: {Mathf.Ceil(gameTimer)}s";

        // Reset contest status
        Hashtable props = new Hashtable { { "HasContested", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_EndGame", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_EndGame()
    {
        isGameRunning = false;
        contestButton.interactable = false;
        foldButton.interactable = false;

        // Determine winner and collect contested players
        Player winner = null;
        int highestNumber = -1;
        contestedResults.Clear();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("HasContested", out object hasContested) &&
                (bool)hasContested)
            {
                if (player.CustomProperties.TryGetValue("RandomNumber", out object number))
                {
                    int playerNumber = (int)number;
                    contestedResults[player] = playerNumber;
                    if (playerNumber > highestNumber)
                    {
                        highestNumber = playerNumber;
                        winner = player;
                    }
                }
            }
        }

        // Update UI with results
        if (winner != null)
        {
            countdownText.text = $"Winner: {winner.NickName} with {highestNumber}!";
        }
        else
        {
            countdownText.text = "No winner - all players folded!";
        }

        UpdatePlayerListUI();
    }
    #endregion

    #region Player Actions
    void OnContestClicked()
    {
        if (isGameRunning)
        {
            Hashtable props = new Hashtable { { "HasContested", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            contestButton.interactable = false;
            statusText.text = "You have contested!";
        }
    }

    void OnFoldClicked()
    {
        if (isGameRunning)
        {
            countdownText.text = "You have folded!";
            contestButton.interactable = false;
            foldButton.interactable = false;
        }
    }

    void OnStartButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient && !isGameRunning)
        {
            StartGame();
        }
    }

    void AssignRandomNumber()
    {
        int randomNumber = Random.Range(1, 100);
        Hashtable props = new Hashtable
        {
            { "RandomNumber", randomNumber },
            { "HasContested", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
    #endregion

    #region UI Updates
    void UpdateStatusText()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        string countdownText = isCountingDown && !isGameRunning ?
            $"Starting in {Mathf.Ceil(lobbyCountdownTimer)}s" : "";
        if (!isGameRunning)
        {
            statusText.text = $"Players: {playerCount}/4 {countdownText}";
        }
    }

    void UpdatePlayerListUI()
    {
        TextMeshProUGUI[] playerTexts = { player1Text, player2Text, player3Text, player4Text };

        for (int i = 0; i < playerTexts.Length; i++)
        {
            playerTexts[i].text = "Waiting...";
        }

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            string playerText = players[i].NickName;
            if (contestedResults.ContainsKey(players[i]))
            {
                playerText += $" Number: {contestedResults[players[i]]}";
            }
            playerTexts[i].text = playerText;
        }
    }
    #endregion
}
