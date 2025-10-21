using Fusion;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class InGameUIHandler : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI statusText;

    [SerializeField]
    TextMeshProUGUI connectionTypeText;

    [SerializeField]
    TextMeshProUGUI rttText;

    [Header("Canvas")]
    [SerializeField] Canvas joinGameCanvas;

    public TextMeshProUGUI[] textMeshProUGUIs;

    private float lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 0.5f; // Her 0.5 saniyede bir güncelle

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
        Debug.Log("InGameUIHandler.OnPlayerDied() called");
        if (joinGameCanvas == null)
        {
            Debug.Log("joinGameCanvas is null!");
            return;
        }
        joinGameCanvas.gameObject.SetActive(true);
    }
    public void Highscores(NetworkDictionary<NetworkString<_32>, Color> playerList)
    {
        if (Time.time - lastUpdateTime < UPDATE_INTERVAL) return;
        lastUpdateTime = Time.time;

        if (playerList.Count == 0) return;

        var sortedPlayers = playerList.ToDictionary(x => x.Key, x => x.Value)
            .OrderByDescending(x => 
            {
                string scoreStr = x.Key.ToString().Split(':').Last().Trim();
                int.TryParse(scoreStr, out int score);
                return score;
            })
            .Take(10)
            .ToList();

        for (int i = 0; i < textMeshProUGUIs.Length; i++)
        {
            if (i < sortedPlayers.Count)
            {
                textMeshProUGUIs[i].text = sortedPlayers[i].Key.ToString();
                textMeshProUGUIs[i].color = sortedPlayers[i].Value;
            }
            else
            {
                textMeshProUGUIs[i].text = "";
            }
        }
    }
    public void SetStatusText() {
        if (NetworkPlayer.Local != null)
        {
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
