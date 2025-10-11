using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
public class NetworkSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    [Header("Player")]
    [SerializeField]
    NetworkPlayer playerPrefab;
    //InputHandler inputHandler;

    [Header("Cell")]
    [SerializeField]
    NetworkObject cellPrefab;
    bool isBotsSpawned = false;

    const int desiredNumberOfPlayers = 10;

    public List<NetworkPlayer> botList = new List<NetworkPlayer>();
    TickTimer botRespawnTimer = new TickTimer();

    public List<NetworkPlayer> Players = new List<NetworkPlayer>();

    NetworkPlayerList NetworkPlayerList;

    InGameUIHandler gameUIHandler;


    void Start()
    {
        gameUIHandler = FindObjectOfType<InGameUIHandler>();
        NetworkPlayerList = GameObject.Find("PlayerListObject").GetComponent<NetworkPlayerList>();
    }


    public override void FixedUpdateNetwork()
    {
        if (botRespawnTimer.Expired(Runner) && Runner.ActivePlayers.Count()>0)
        {
            int aliveBots = 0;

            NetworkPlayer anyDeadBot = null;

            foreach (NetworkPlayer botNetworkPlayer in botList)
            {
                if (botNetworkPlayer.playerState == NetworkPlayer.PlayerState.playing)
                {
                    aliveBots++;
                }
                else
                {
                    anyDeadBot = botNetworkPlayer;
                }
            }
            int numberOfBotsToSpawn = desiredNumberOfPlayers - Runner.SessionInfo.PlayerCount - aliveBots;

            if (numberOfBotsToSpawn > 0 && anyDeadBot != null)
            {
                anyDeadBot.BotJoinGame();
            }
            if (numberOfBotsToSpawn > 1)
            {
                botRespawnTimer = TickTimer.CreateFromSeconds(Runner, 2);
            }
            else
            {
                botRespawnTimer = TickTimer.None;
            }
        }

        if (Runner.IsServer) { 
            UpdatePlayerNetworkArray();
            UpdatePlayerPositionDict();
           // Debug.Log("UpdatePlayerNetworkArray called from FixedUpdateNetwork" + NetworkPlayerList.PlayerNetworkArray.Length);
        }
    }

    public override void Render()
    {
        if(Runner == null)
            Debug.Log("Runner is null in NetworkSpawner Render");
        if(NetworkPlayerList.Instance == null)
            Debug.Log("NetworkPlayerList is null in NetworkSpawner Render");

        else if (Runner.IsClient && NetworkPlayerList.Instance.isActiveAndEnabled)
        {
            if (gameUIHandler == null)
                Debug.Log("gameUIHandler is null");

            gameUIHandler.Highscores(NetworkPlayerList.Instance.PlayerNetworkDict);
        }
    }





    void SpawnBots()
    {
        if (Runner.SessionInfo.PlayerCount < desiredNumberOfPlayers + botList.Count && Runner.IsServer && Runner.ActivePlayers.Count()>0)
        {
            int numberOfBotsToSpawn = desiredNumberOfPlayers - Runner.SessionInfo.PlayerCount - botList.Count;

            //Debug.Log($"Number of bots to spawn {numberOfBotsToSpawn}. Bot spawned count {botList.Count}. Player count {Runner.SessionInfo.PlayerCount}");

            for (int i = 0; i < numberOfBotsToSpawn; i++)
            {
                NetworkPlayer spawnedAIPlayer = Runner.Spawn(playerPrefab, Utils.GetRandomPosition(), Quaternion.identity, null,InitializeBotBeforeSpawn);
                spawnedAIPlayer.nickName = Utils.GetRandomName();
                spawnedAIPlayer.BotJoinGame();
                botList.Add(spawnedAIPlayer);
                Players.Add(spawnedAIPlayer);
            }
        }
        isBotsSpawned = true;
    }

    private void InitializeBotBeforeSpawn(NetworkRunner runner, NetworkObject networkObject)
    {
        if (runner.IsServer)
            networkObject.GetComponent<NetworkPlayer>().isBot = true;
    }

    public void OnBotDied(NetworkPlayer networkPlayer)
    {
        if (Runner.IsServer && Runner.SessionInfo.PlayerCount < desiredNumberOfPlayers)
        {
            //Debug.Log("Bot died,respawn required");

            if (!botRespawnTimer.IsRunning)
            {
                botRespawnTimer = TickTimer.CreateFromSeconds(Runner, 2);
            }
        }
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //Debug.Log("OnPlayerJoined");
        if (runner.IsServer)
        {
            NetworkPlayer spawnedNetworkPlayer = runner.Spawn(playerPrefab, Utils.GetRandomPosition(), Quaternion.identity, player);
            spawnedNetworkPlayer.playerState = NetworkPlayer.PlayerState.playing;
            
                Players.Add(spawnedNetworkPlayer);
            for (int i = 0; i < desiredNumberOfPlayers; i++)
            {
                NetworkObject networkCell = runner.Spawn(cellPrefab, Utils.GetRandomPosition(), Quaternion.identity);
                networkCell.transform.position = Utils.GetRandomPosition();
            }
            if (!isBotsSpawned)
            {
                SpawnBots();
            }
#if UNITY_SERVER
            MultiplayServerHostingHandler.instance.SetCurrentNumberOfPlayers((ushort)runner.ActivePlayers.Count());
#endif
            if (runner.ActivePlayers.Count() >= Utils.GetMaxPlayersFromStartupArgs())
            {
                Debug.Log("runner.ActivePlayers.Count() >= Utils.GetMaxPlayersFromStartupArgs()" + runner.ActivePlayers.Count() + " - " + Utils.GetMaxPlayersFromStartupArgs());
                runner.SessionInfo.IsOpen = false;
                Debug.Log("Session is closed, no more players can join");
            }
            // Array'i temizle ve yeniden doldur
            ClearNetworkArray();
            UpdatePlayerNetworkArray();
            Debug.Log("OnPlayerJoined called from NetworkSpawner for: " + player + " " + spawnedNetworkPlayer.nickName + DateTime.Now.ToString());
        }
    }

    private void ClearNetworkArray()
    {
        if (Runner.IsServer && NetworkPlayerList.Instance != null)
        {
            NetworkPlayerList.Instance.PlayerNetworkDict.Clear();
        }
    }

    public void UpdatePlayerNetworkArray()
    {
        if (!Runner.IsServer || NetworkPlayerList.Instance == null) return;

        ClearNetworkArray();

        var orderedPlayers = Players.Where(p => p != null && p.Object != null)
                                   .OrderByDescending(p => p.size)
                                   .ToList();

        for (int i = 0; i < orderedPlayers.Count && i < NetworkPlayerList.Instance.PlayerNetworkDict.Capacity/2; i++)
        {
            var player = orderedPlayers[i];
            var playerInfo = new NetworkString<_32>($"{player.nickName} : {player.size}");
            NetworkPlayerList.Instance.PlayerNetworkDict.Set(playerInfo, player.spriteColor);
        }
    }

    public void UpdatePlayerPositionDict()
    {
        if (!Runner.IsServer || NetworkPlayerList.Instance == null) return;
        NetworkPlayerList.Instance.PlayerPositions.Clear();
        foreach (var player in Players)
        {
            if (player != null && player.Object != null && player.playerState == NetworkPlayer.PlayerState.playing)
            {
                Vector3 roundedPosition = new Vector3(
                    Mathf.Round(player.transform.position.x),
                    Mathf.Round(player.transform.position.y),
                    Mathf.Round(player.transform.position.z)
                );
                if (NetworkPlayerList.Instance.PlayerPositions.ContainsKey(roundedPosition)) { 
                    NetworkPlayerList.Instance.PlayerPositions.Remove(roundedPosition);
                }
                NetworkPlayerList.Instance.PlayerPositions.Add(roundedPosition, player.size);
            }
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        //Debug.Log("OnConnectedToServer");

        if (runner.IsServer)
            NetworkPlayer.Local.playerState = NetworkPlayer.PlayerState.connected;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            NetworkPlayer leavingPlayer = Players.Find(p => p != null && p.Object != null && p.Object.InputAuthority == player);
            Debug.Log($"Player {player} left. Updating arrays..." + leavingPlayer.nickName + DateTime.Now);


            Players.RemoveAll(p => p == null || p.Object == null || p.Object.InputAuthority == player);
            
            // Array'i temizle ve güncelle
            ClearNetworkArray();
            UpdatePlayerNetworkArray();
            Debug.Log("Active players after despawn: " + runner.ActivePlayers.Count());
        }

        if (runner.IsServer && runner.ActivePlayers.Count() <= 0)
        {
            Debug.Log("no active players" + runner.ActivePlayers.Count() + "Despawning bots" + botList.Count);
            for(int i = 0; i < botList.Count; i++)
            {
                NetworkPlayer bot = botList[i];
                if (bot != null && bot.Object != null)
                {
                    Players.Remove(bot);
                    runner.Despawn(bot.Object);
                }
            }
            if(botList.Count > 0)
                botList.Clear();
            ClearNetworkArray();
            //Debug.Log("Active players after despawning bots: " + runner.ActivePlayers.Count());
            //Debug.Log("Bots despawned" + botList.Count);
            //Debug.Log("Players despawned" + Players.Count);
            isBotsSpawned = false;

            NetworkObject[] cells = GameObject.FindGameObjectsWithTag("Cell").Select(go => go.GetComponent<NetworkObject>()).ToArray();
            foreach (var cell in cells)
            {
                if (cell != null)
                {
                    runner.Despawn(cell);
                }
            }
        }
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

        Debug.Log("OnShutdown" + shutdownReason);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    { 
        Debug.Log("OnDisconnectedFromServer " + reason);

        if (gameUIHandler == null)
        {
            Debug.LogError("gameUIHandler is null in OnDisconnectedFromServer");
            return;
        }
        if (NetworkPlayer.Local == null)
            Debug.Log("NetworkPlayer.Local is null in OnDisconnectedFromServer");
        else
            NetworkPlayer.Local.OnPlayerDead();
        // Her türlü baðlantý kopmasý durumunda ana menüye dön ***
           gameUIHandler.OnMenuButtonClicked();

        if (reason == NetDisconnectReason.Timeout)
        {
            Debug.Log("You have been disconnected due to a timeout. Please check your internet connection and try again.");
            if (NetworkPlayer.Local == null)
                Debug.Log("NetworkPlayer.Local is null in OnDisconnectedFromServer");
            else
                NetworkPlayer.Local.OnPlayerDead();
        }
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log("OnConnectFailed" + reason); }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        Debug.Log("OnReliableDataProgress");
    }
}