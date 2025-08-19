using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class InGameUIHandler : SimulationBehaviour
{
    [SerializeField]
    TextMeshProUGUI statusText;

    [SerializeField]
    TextMeshProUGUI connectionTypeText;

    [SerializeField]
    TextMeshProUGUI rttText;

    [Header("Canvas")]
    [SerializeField]
    Canvas joinGameCanvas;

    public TextMeshProUGUI[] textMeshProUGUIs;

    private void Start()
    {
        //#if UNITY_SERVER
        //gameObject.SetActive(false);
        //return;
        //#endif

       // statusText.gameObject.SetActive(false);
    }

    void Update()
    {
        SetStatusText();
    }



    public void SetConnectionType(string type)
    {
        connectionTypeText.text = $"Connection type: {type} ";
    }

    public void SetRtt(string rtt)
    {
        rttText.text = rtt;
    }

    public void OnJoinGame()
    {
        Debug.Log("OnJoinGame clicked");
        if (NetworkPlayer.Local == null)
            Debug.Log("NetworkPlayer.Local is null");
        if(NetworkPlayer.Local.nickName.ToString() == "")
            Debug.Log("NetworkPlayer.Local.nickName is empty");
        
        NetworkPlayer.Local.JoinGame(NetworkPlayer.Local.nickName.ToString());
        Debug.Log("NetworkPlayer.Local.JoinGame called with nickName: " + NetworkPlayer.Local.nickName.ToString());
        joinGameCanvas.gameObject.SetActive(false);

        //statusText.gameObject.SetActive(true);
    }
    public void OnPlayerDied()
    {
        joinGameCanvas.gameObject.SetActive(true);
    }
    public void Highscores(NetworkArray<NetworkString<_32>> playerList)
    {
        if (playerList.Length > 0 && playerList.Length < 11)
        {
            for (int i = 0; i < playerList.Length; i++)
            {
              // NetworkPlayer player = playerList[i];
              //  textMeshProUGUIs[i].text = player.nickName + " : " + player.size;
              //  textMeshProUGUIs[i].color = player.spriteColor;

                textMeshProUGUIs[i].text = playerList[i].ToString();

            }
        }
    }

    public void SetStatusText() {
        if (NetworkPlayer.Local != null) {
            statusText.text = NetworkPlayer.Local.playerState.ToString();
            statusText.gameObject.SetActive(true);
        }
        else
        {
            statusText.text = "Waiting for connection...";
            statusText.gameObject.SetActive(true);
        }
    }

    // Menü butonuna atanacak fonksiyon
    public void OnMenuButtonClicked()
    {
        // NetworkRunner'ý bul
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            runner.Shutdown();
        }

        // Gerekirse PlayerManager gibi singletonlarý temizle
        var manager = GameObject.Find("PlayerManager");
        if (manager != null)
            Destroy(manager);

        // Ana menü sahnesini yükle (index 0)
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
