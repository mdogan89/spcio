using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour
{
    [SerializeField] Bot botPrefab;
    public int numberOfBots; // Default number of bots
    [SerializeField] GameObject foodPrefab;
    public int numberOfFood;
    public static float spawnRadius;
    public static List<Bot> botList = new List<Bot>();
    Player player;
    public static List<Player> playerList = new List<Player>();
    public GameObject playerPrefab; // Reference to the player prefab

    [SerializeField] TextMeshProUGUI[] playerNamesText;

    string[] playerNames = new string[0];
    string[] playerNamesRickandMorty = new string[20] { "Rick", "Morty", "Beth", "Jerry", "Summer", "Birdperson", "Mr. Poopybutthole", "Evil Morty", "Squanchy", "Tammy", "Unity", "Mr. Meeseeks", "Scary Terry", "Krombopulos Michael", "Gearhead", "Abradolf Lincler", "Noob-Noob", "Jessica", "Poopy Diaper", "Mr. Goldenfold" };
    string[] playerNamesBojack = new string[20] { "Bojack", "Diane", "Todd", "Princess Carolyn", "Mr. Peanutbutter", "Wanda", "Ruthie", "Sarah Lynn", "Vincent Adultman", "Emily", "Judah Mannowdog", "Lenny Turteltaub", "Cuddlywhiskers", "Charley Witherspoon", "Yoda", "Groot", "Darth Vader", "Obi-Wan Kenobi", "Leia Organa", "Han Solo" };
    string[] playerNamesSimpsons = new string[20] { "Homer", "Marge", "Bart", "Lisa", "Maggie", "Mr. Burns", "Smithers", "Ned Flanders", "Apu", "Krusty", "Sideshow Bob", "Milhouse", "Ralph Wiggum", "Chief Wiggum", "Comic Book Guy", "Edna Krabappel", "Patty Bouvier", "Selma Bouvier", "Moe Szyslak", "Barney Gumble" };
    string[] playerNamesFamilyGuy = new string[20] { "Peter", "Lois", "Stewie", "Brian", "Meg", "Chris", "Glenn Quagmire", "Cleveland Brown", "Joe Swanson", "Tom Tucker", "Angela", "Carter Pewterschmidt", "Mort Goldman", "Seamus", "Consuela", "Herbert", "Dr. Hartman", "Dr. Elmer Hartman", "Mayor Adam West", "Tricia Takanawa" };
    string[] playerNamesSouthPark = new string[20] { "Stan", "Kyle", "Cartman", "Kenny", "Butters", "Randy", "Sheila", "Mr. Garrison", "Mr. Mackey", "Chef", "Timmy", "Towelie", "Wendy", "Bebe", "Token", "Craig", "Tweek", "PC Principal", "Mr. Hankey", "Satan" };
    private void Awake()
    {
        // Ensure the static lists are cleared when the game starts
        botList.Clear();
        playerList.Clear();

        playerNames = playerNames.Concat(playerNamesRickandMorty)
            .Concat(playerNamesBojack)
            .Concat(playerNamesSimpsons)
            .Concat(playerNamesFamilyGuy)
            .Concat(playerNamesSouthPark)
            .ToArray();
        //Shuffle the player names array
        for (int i = 0; i < playerNames.Length; i++)
        {
            int randomIndex = Random.Range(i, playerNames.Length);
            string temp = playerNames[i];
            playerNames[i] = playerNames[randomIndex];
            playerNames[randomIndex] = temp;
        }

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager instance is not found. Make sure it is initialized before Spawner.");
            return;
        }
        // Load the number of bots and food from PlayerManager
        numberOfBots = PlayerManager.Instance.numberOfBots;
        numberOfFood = PlayerManager.Instance.numberOfFood;
        spawnRadius = PlayerManager.Instance.spawnRadius;
        Player player;

        if (SceneManager.GetActiveScene().name == "HowToPlay")
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }
        else
        {
            //Rotate the player prefab to the center of the scene ??
            Quaternion rotation = Quaternion.Euler(0, 180, 0); 
            player = Instantiate(playerPrefab, GetRandomPosition(), rotation).GetComponent<Player>();
            playerList.Add(player);
        }
    }
    void Start()
    {

        if(PlayerManager.Instance.gameMode != 3 && SceneManager.GetActiveScene().name != "HowToPlay") { 
        SpawnBots();
        UpdateBotNames();
        }
        SpawnFood();
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

    public void SpawnFood()
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
            bot.name = playerNames[i]; // Set the name of the GameObject
        }
    }

    }


