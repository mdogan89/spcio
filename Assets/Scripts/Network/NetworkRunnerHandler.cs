using Fusion;
using Fusion.Sockets;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.Photon.Realtime;

public class NetworkRunnerHandler : MonoBehaviour
{
    [SerializeField] NetworkRunner networkRunnerPrefab;

    NetworkRunner networkRunner;

    private void Awake()
    {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();

        if (networkRunnerInScene != null)
        {
            networkRunner = networkRunnerInScene;
        }

        //NetworkObject networkSpawnerInScene = FindObjectOfType<NetworkSpawner>().gameObject.GetComponent<NetworkObject>();
        //if (networkSpawnerInScene != null)
        //{
        //    networkSpawner = networkSpawnerInScene;
        //}
    }

    async private void Start()
    {
        //   QualitySettings.vSyncCount = 0; // Set vSyncCount to 0 so that using .targetFrameRate is enabled.
        //  Application.targetFrameRate = 24;
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network runner";

            GameMode gameMode = GameMode.Client;
#if UNITY_EDITOR
            gameMode = GameMode.Client;
#endif

#if UNITY_SERVER
            gameMode = GameMode.Server;

            Debug.Log("ServerNetworkRunner done");
#endif
            int multiplayerSceneIndex2 = SceneManager.GetSceneByName("Multiplayer").buildIndex;

            await InitializeNetworkRunner(networkRunner, gameMode, "TestSession", SceneManager.GetActiveScene().buildIndex, null);
            Debug.Log( gameMode + "runner initialized at scene:" + SceneManager.GetActiveScene().buildIndex);
        }

       
    }


    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, string sessionName, int scene, Action<NetworkRunner> onGameStarted)
    {
        var sceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;

        FusionAppSettings appSettings = PhotonAppSettings.Global.AppSettings;

        string fixedRegion = Utils.GetRegionFromStartupArgs();
        if (fixedRegion != "")
        {
            appSettings.FixedRegion = fixedRegion;
        }

        int port = Utils.GetServerPortFromStartupArgs();

        if (port != 0)
        {
            appSettings.Port = port;
        }
        //string serverID = Utils.GetServerIDFromStartupArgs();

        //if (serverID == "")
        //{
        //    serverID = sessionName;
        //}


          Debug.Log($"InitializeNetworkRunner with port{appSettings.Port} using region {appSettings.FixedRegion}");

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Scene = SceneRef.FromIndex(scene),
            SessionName = sessionName,
            //OnGameStarted = onGameStarted,
            SceneManager = sceneManager,
            CustomPhotonAppSettings = appSettings,
            //CustomLobbyName = "Default",
        });
    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        return sceneManager;

    }


}
