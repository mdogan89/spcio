using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] Bot botPrefab;
    [SerializeField] int numberOfBots = 20;
    [SerializeField] GameObject foodPrefab;
    [SerializeField] int numberOfFood = 50;
    public static float spawnRadius = 50f;
    public static List<Bot> botList = new List<Bot>();
    Player player;
    public static List<Player> playerList = new List<Player>();
    void Start()
    {
        SpawnBots();
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
            bot.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.5f, 1f);
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



}


