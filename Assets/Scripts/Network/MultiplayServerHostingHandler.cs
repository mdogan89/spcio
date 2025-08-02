using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

#if UNITY_SERVER
using Unity.Services.Multiplay;
#endif


public class MultiplayServerHostingHandler : MonoBehaviour
{
    public static MultiplayServerHostingHandler instance = null;
#if UNITY_SERVER
    IServerQueryHandler serverQueryHandler;
#endif
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
#if UNITY_SERVER
    async void Start()
    {
        await InitUnityServices();

        serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(Utils.GetMaxPlayersFromStartupArgs(), "Test", "Regular", "Test", "Default");
    }
#endif
#if UNITY_SERVER
    void Update()
    {
        if (serverQueryHandler != null)
            serverQueryHandler.UpdateServerCheck();
    }

    public void SetCurrentNumberOfPlayers(ushort numberOfPlayers)
    {
        serverQueryHandler.CurrentPlayers = numberOfPlayers;
    }
#endif



    async Task InitUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.Log($"UnityServices init failed with error {e.InnerException}");
            return;
        }


        Debug.Log("UnityServices init OK");

    }

}
