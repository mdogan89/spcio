using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] Bot botPrefab;
    [SerializeField] int numberOfBots = 20;
    [SerializeField] GameObject foodPrefab;
    [SerializeField] int numberOfFood = 50;
    public static float spawnRadius = 40f;
    public static List<Bot> botList = new List<Bot>();
    Player player;
    public static List<Player> playerList = new List<Player>();

    [SerializeField] TextMeshProUGUI[] playerNamesText;

    string[] playerNames = new string[20] { "Rick", "Morty", "Beth", "Jerry", "Summer", "Birdperson", "Mr. Poopybutthole", "Evil Morty", "Squanchy", "Tammy", "Unity", "Mr. Meeseeks", "Scary Terry", "Krombopulos Michael", "Gearhead", "Abradolf Lincler", "Noob-Noob", "Jessica", "Poopy Diaper", "Mr. Goldenfold" };

    void Start()
    {
        SpawnBots();
        UpdateBotNames();
        SpawnFood();
        player = GameObject.Find("Player").GetComponentInChildren<Player>();
        playerList.Add(player);
    }

    void SpawnBots()
    {
        for (int i = 0; i < numberOfBots; i++)
        {
            Vector3 position = GetRandomPosition();
            Bot bot = Instantiate(botPrefab, position, Quaternion.identity).GetComponent<Bot>();
            bot.GetComponent<Player>()._color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.5f, 1f);
            bot.GetComponent<MeshRenderer>().material.color = bot.GetComponent<Player>()._color;
            botList.Add(bot);
            playerList.Add(bot.GetComponent<Player>());
        }
    }

    void SpawnFood()
    {
        for (int i = 0; i < numberOfFood; i++)
        {
            Vector3 position = GetRandomPosition();
            Instantiate(foodPrefab, position, Quaternion.identity);
        }
    }

    public static Vector3 GetRandomPosition()
    {
        return Random.insideUnitSphere * spawnRadius;
    }

    private void Update()
    {
        UpdatePlayerNames();
    }

    void UpdatePlayerNames()
    {
        //var sortedList = new SortedList<int, Player>(Comparer<int>.Create((a, b) => b.CompareTo(a))); // Sort by score descending


        // sort the playerList by score in descending order
        var sortedList = new List<Player>(playerList);
        sortedList.Sort((a, b) => b.score.CompareTo(a.score));


        for (int i = 0; i < playerNamesText.Length; i++)
        {
            if (i < sortedList.Count)
            {
                playerNamesText[i].text = sortedList[i].nick + " - " + sortedList[i].score;
                playerNamesText[i].color = sortedList[i]._color;
            }
            else
            {
                playerNamesText[i].text = "";
            }
        }
    }

    void UpdateBotNames()
    {
        for (int i = 0; i < botList.Count; i++)
        {
            Bot bot = botList[i];
            bot.GetComponent<Player>().nick = playerNames[i]; // Assign a name from the array
        }
    }

    }


