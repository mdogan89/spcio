using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bot : MonoBehaviour
{
    Rigidbody rb;
    Vector3 target = Vector3.zero;
    public bool hasTarget = false;
    Player botPlayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        botPlayer = GetComponent<Player>();
    }

    void FixedUpdate()
    {
        if (!hasTarget)
        {
            FindTarget();

        }
        else
        {
            MoveToTarget();
        }
    }
    void FindTarget()
    {
        var candidates = new List<(GameObject obj, float distance, int score)>();

        List<GameObject> targets = new List<GameObject>();
        if (PlayerManager.Instance.botLevel == 1)
            targets = Spawner.playerList.Where(p => p != null && p != botPlayer).Select(p => p.gameObject).ToList();
        else if (PlayerManager.Instance.botLevel == 0)
            targets = Spawner.botList.Where(b => b != null && b != this).Select(b => b.gameObject).ToList();
        else if (PlayerManager.Instance.botLevel == 2)
            if (GameObject.FindAnyObjectByType<LocalPlayer>() != null)
                targets.Add(GameObject.FindAnyObjectByType<LocalPlayer>().GetComponent<Player>().gameObject);
            else
                targets = Spawner.botList.Where(b => b != null && b != this).Select(b => b.gameObject).ToList();


        foreach (var t in targets)
                {
                    if (t == null) continue;
                    float dist = Vector3.Distance(transform.position, t.transform.position);
                    int score = t.GetComponent<Player>().score;
                    candidates.Add((t, dist, score));
                }

        if (candidates.Count == 0)
        {
            FindFood();
            return;
        }

        var closest = candidates.OrderBy(c => c.distance).First();

        if (closest.score == botPlayer.score || closest.score > botPlayer.score)
            FindFood();
        else
        {
            target = closest.obj.transform.position;
            hasTarget = true;
        }
    }
    //void FindTarget()
    //{
    //    SortedList<Vector3, int> sortedTargets = new SortedList<Vector3, int>(Comparer<Vector3>.Create((a, b) => a.magnitude.CompareTo(b.magnitude)));

    //    List<GameObject> targets = new List<GameObject>();

    //    if (PlayerManager.Instance.botLevel == 1) {

    //        targets = Spawner.playerList
    //            .Where(player => player != null && player != GetComponent<Player>())
    //            .Select(player => player.gameObject)
    //            .ToList();
    //    }
    //    else if (PlayerManager.Instance.botLevel == 0)
    //    {
    //        targets = Spawner.botList
    //            .Where(bot => bot != null && bot != GetComponent<Bot>())
    //            .Select(bot => bot.gameObject)
    //            .ToList();
    //    }
    //    else if (PlayerManager.Instance.botLevel == 2)
    //    {
    //        targets.Add(GameObject.Find("Player").GetComponentInChildren<Player>().gameObject);
    //    } 
    //    foreach (var bot in targets)
    //    {
    //        if (bot == null || bot == GetComponent<Bot>()) continue; // Skip if player is null

    //        Vector3 botPosition = bot.transform.position;
    //        Vector3 directionToBot = (botPosition - transform.position).normalized;
    //        Vector3 distance = botPosition - transform.position;

    //        if (sortedTargets.ContainsKey(distance))
    //        {
    //            sortedTargets[distance] += bot.GetComponent<Player>().score; // If the distance already exists, add the score
    //        }
    //        else
    //        {
    //            sortedTargets.Add(distance, bot.GetComponent<Player>().score); // Add the distance to the list with score
    //        }
    //    }

    //    if(sortedTargets.Count == 0)
    //    {
    //        FindFood(); // If no targets found, find food
    //        return;
    //    }

    //    if (sortedTargets.ElementAt(0).Value == botPlayer.score)
    //    {
    //        FindFood();
    //    }

    //    else if (sortedTargets.ElementAt(0).Value < botPlayer.score)
    //    {
    //        target = sortedTargets.ElementAt(0).Key; // Set the target to the closest distance
    //        hasTarget = true;
    //    }
    //    else if (sortedTargets.ElementAt(0).Value > botPlayer.score)
    //    {
    //        FindFood(); // Find food if the closest enemy's score is more than the bot's score
    //    }
    //}
    //void MoveToTarget()
    //{
    //    //bot speed?
    //        rb.MovePosition(rb.position + (target - rb.position).normalized * botPlayer.speedMult * Time.fixedDeltaTime * PlayerManager.Instance.moveSensitivity);
    //    //  rb.AddForce((target - transform.position).normalized * GetComponent<Player>().speedMult * Time.fixedDeltaTime, ForceMode.Impulse);
    //    if ((target - transform.position).magnitude < 1f)
    //        hasTarget = false;
    //}
    void MoveToTarget()
    {
        Vector3 direction = (target - rb.position);
        float distance = direction.magnitude;
        if (distance<0.25)
        {
            hasTarget = false;
            return;
        }
        float speed = botPlayer.speedMult * Mathf.Clamp01(distance / 5f); // Yaklaþtýkça yavaþla
        rb.MovePosition(rb.position + direction.normalized * speed * Time.fixedDeltaTime * PlayerManager.Instance.moveSensitivity);
    }
    void FindFood()
    {
        var foodList = GetFoodList();
        if (foodList.Count == 0)
        {
            GameObject.Find("Spawner").GetComponent<Spawner>().SpawnFood();
            foodList = GetFoodList();
        }
        if (foodList.Count > 0)
        {
            target = foodList[Random.Range(0, foodList.Count)];
            hasTarget = true;
        }
    }
    List<Vector3> GetFoodList()
    {
        var foods = GameObject.FindGameObjectsWithTag("Food");
        if (foods == null || foods.Length == 0) return new List<Vector3>();
        return foods.Select(f => f.transform.position)
                    .OrderBy(pos => Vector3.Distance(transform.position, pos))
                    .ToList();
    }
    //void FindFood()
    //{
    //    List<Vector3> foodList = GetFoodList();
    //    if (foodList.Count > 0 && foodList[0] != Vector3.zero)
    //    {
    //        target = foodList[0]; // Set the target to the closest food position
    //        hasTarget = true;
    //    }
    //    else
    //    {
    //        foodList = GetFoodList(); // Get the list of food positions again if the closest one is not valid
    //        if (foodList.Count > 0 && foodList[0] != Vector3.zero)
    //        {
    //            target = foodList[0]; // Set the target to the closest food position
    //            hasTarget = true;
    //        }
    //        else
    //        {
    //            GameObject.Find("Spawner").GetComponent<Spawner>().SpawnFood(); // Spawn food if no valid food positions are found
    //            foodList = GetFoodList(); // Get the list of food positions again after spawning
    //            if (foodList.Count == 0) return; // If still no food, exit
    //            else if (foodList.Count > 0 && foodList[0] != Vector3.zero)
    //            {
    //                target = foodList[0]; // If only one food position, set it as the target
    //                hasTarget = true;
    //            }
    //            else
    //            {
    //                int i = Random.Range(0, foodList.Count); // Get a random food position
    //                target = foodList[i]; // Set the target to the closest food position
    //                hasTarget = true;
    //            }
    //        }
    //    }
    //}
    //List<Vector3> GetFoodList()
    //{
    //    List<Vector3> foodList = GameObject.FindGameObjectsWithTag("Food")
    //              .Select(food => food.transform.position).OrderBy(foodPosition => (foodPosition - transform.position).magnitude)
    //              .ToList();
    //    return foodList;
    //}
}





