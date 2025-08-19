using Fusion;
using Fusion.Sockets;
using System;
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

    List<NetworkPlayer> botList = new List<NetworkPlayer>();
    TickTimer botRespawnTimer = new TickTimer();

    public List<NetworkPlayer> Players = new List<NetworkPlayer>();

    NetworkPlayerList NetworkPlayerList;

    InGameUIHandler gameUIHandler;
    void Start()
    {
        gameUIHandler = FindObjectOfType<InGameUIHandler>();
        NetworkPlayerList = FindObjectOfType<NetworkPlayerList>();
    }


    public override void FixedUpdateNetwork()
    {
        if (botRespawnTimer.Expired(Runner))
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

            Debug.Log($"Bot respawn, number of bots to spawn {numberOfBotsToSpawn} any not dead bot");

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
        if (Runner.IsServer && NetworkPlayerList.isActiveAndEnabled)
        {
            IEnumerable<NetworkPlayer> orderedPlayers = Players.OrderByDescending(player => player.size);
            List<NetworkPlayer> topTenPlayers = new List<NetworkPlayer>();

            foreach (NetworkPlayer player in orderedPlayers)
            {
                topTenPlayers.Add(player);
            }    
            for (int i = 0; i < topTenPlayers.Count(); i++)
            {
                NetworkString<_32> playerName = new NetworkString<_32>(topTenPlayers[i].nickName.ToString() + " : " + topTenPlayers[i].size);
                NetworkPlayerList.PlayerNetworkArray.Set(i, playerName);
            }  
        }
    }

    public override void Render()
    {
        if (Runner.IsPlayer && NetworkPlayerList.isActiveAndEnabled)
        {
            if (gameUIHandler == null)
                Debug.Log("gameUIHandler is null");

            gameUIHandler.Highscores(NetworkPlayerList.PlayerNetworkArray);
        }
    }





    void SpawnBots()
    {
        if (Runner.SessionInfo.PlayerCount < desiredNumberOfPlayers + botList.Count && Runner.IsServer)
        {
            int numberOfBotsToSpawn = desiredNumberOfPlayers - Runner.SessionInfo.PlayerCount - botList.Count;

            Debug.Log($"Number of bots to spawn {numberOfBotsToSpawn}. Bot spawned count {botList.Count}. Player count {Runner.SessionInfo.PlayerCount}");

            for (int i = 0; i < numberOfBotsToSpawn; i++)
            {
                NetworkPlayer spawnedAIPlayer = Runner.Spawn(playerPrefab, Utils.GetRandomPosition(), Quaternion.identity, null,InitializeBotBeforeSpawn);

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
            Debug.Log("Bot died,respawn required");

            if (!botRespawnTimer.IsRunning)
            {
                botRespawnTimer = TickTimer.CreateFromSeconds(Runner, 2);
            }
        }
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerJoined");
        if (runner.IsServer)
        {
            NetworkPlayer spawnedNetworkPlayer = runner.Spawn(playerPrefab, Utils.GetRandomPosition(), Quaternion.identity, player);
            spawnedNetworkPlayer.playerState = NetworkPlayer.PlayerState.playing;
            spawnedNetworkPlayer.nickName = PlayerManager.Instance.nick;
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
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");

        if (runner.IsServer)
            NetworkPlayer.Local.playerState = NetworkPlayer.PlayerState.connected;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

        if (runner.IsServer)
        {
#if UNITY_SERVER
      MultiplayServerHostingHandler.instance.SetCurrentNumberOfPlayers((ushort)runner.ActivePlayers.Count());
#endif
            if (runner.ActivePlayers.Count() < Utils.GetMaxPlayersFromStartupArgs())
            {
                runner.SessionInfo.IsOpen = true;
            }
        }
        Debug.Log("OnPlayerLeft");
 
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

        Debug.Log("OnShutdown" + shutdownReason);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { Debug.Log("OnDisconnectedFromServer" + reason); }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { Debug.Log("OnConnectRequest"); }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log("OnConnectFailed"); }

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





