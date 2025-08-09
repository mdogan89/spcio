using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        
        NetworkPlayer.Local.JoinGame("TESTPLAYER");
        Debug.Log("NetworkPlayer.Local.JoinGame(\"TESTPLAYER\")" + "called");
        joinGameCanvas.gameObject.SetActive(false);

        //statusText.gameObject.SetActive(true);
    }
    public void OnPlayerDied()
    {
        joinGameCanvas.gameObject.SetActive(true);
    }

    //public void Highscores(string[] scores)
    //{
    //    for (int i = 0; i < scores.Length; i++)
    //    {
    //        textMeshProUGUIs[i].text = scores[i];

    //    }

    //}
    public void Highscores(List<NetworkPlayer> playerList)
    {
        if (playerList.Count > 0 && playerList.Count < 11)
        {
            for (int i = 0; i < 10; i++)
            {
                NetworkPlayer player = playerList[i];
                textMeshProUGUIs[i].text = player.nickName + " : " + player.size;
                textMeshProUGUIs[i].color = player.spriteColor;

            }
        }
    }

    public void SetStatusText() {
        if (NetworkPlayer.Local != null){
            statusText.text = NetworkPlayer.Local.playerState.ToString();
            statusText.gameObject.SetActive(true);
        }
        else
        {
            statusText.text = "Waiting for spawning player...";
            statusText.gameObject.SetActive(true);
        }




    }


}


