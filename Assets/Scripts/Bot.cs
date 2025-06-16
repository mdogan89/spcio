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

    void Update()
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
        SortedList<Vector3, int> sortedTargets = new SortedList<Vector3, int>(Comparer<Vector3>.Create((a, b) => a.magnitude.CompareTo(b.magnitude)));

        foreach (Bot bot in Spawner.botList)
        {
            if (bot == null || bot == GetComponent<Player>()) continue; // Skip if player is null

            Vector3 botPosition = bot.transform.position;
            Vector3 directionToBot = (botPosition - transform.position).normalized;
            Vector3 distance = botPosition - transform.position;

            if (sortedTargets.ContainsKey(distance))
            {
                sortedTargets[distance] += bot.GetComponent<Player>().score; // If the distance already exists, add the score
            }
            else
            {
                sortedTargets.Add(distance, bot.GetComponent<Player>().score); // Add the distance to the list with score
            }
        }

        if (sortedTargets.ElementAt(0).Value == botPlayer.score)
        {
            FindFood();
        }

        else if (sortedTargets.ElementAt(0).Value < botPlayer.score)
        {
            target = sortedTargets.ElementAt(0).Key; // Set the target to the closest distance
            hasTarget = true;
        }
        else if (sortedTargets.ElementAt(0).Value > botPlayer.score)
        {
            FindFood(); // Find food if the closest enemy's score is more than the bot's score
        }
    }
    void MoveToTarget()
    {
        rb.AddForce((target - transform.position).normalized * GetComponent<Player>().speedMult, ForceMode.Impulse);
        if ((target - transform.position).magnitude < 1f)
            hasTarget = false;
    }


    void FindFood()
    {
        List<Vector3> foodList = GetFoodList();
        if (foodList.Count > 0 && foodList[0] != Vector3.zero)
        {
            target = foodList[0]; // Set the target to the closest food position
            hasTarget = true;
        }
        else
        {
            foodList = GetFoodList(); // Get the list of food positions again if the closest one is not valid
            if (foodList.Count > 0 && foodList[0] != Vector3.zero)
            {
                target = foodList[0]; // Set the target to the closest food position
                hasTarget = true;
            }
            else
            {
                GameObject.Find("Spawner").GetComponent<Spawner>().SpawnFood(); // Spawn food if no valid food positions are found
                foodList = GetFoodList(); // Get the list of food positions again after spawning
                if (foodList.Count == 0) return; // If still no food, exit
                else if (foodList.Count > 0 && foodList[0] != Vector3.zero)
                {
                    target = foodList[0]; // If only one food position, set it as the target
                    hasTarget = true;
                }
                else
                {
                    int i = Random.Range(0, foodList.Count); // Get a random food position
                    target = foodList[i]; // Set the target to the closest food position
                    hasTarget = true;
                }
            }
        }
    }
    List<Vector3> GetFoodList()
    {
        List<Vector3> foodList = GameObject.FindGameObjectsWithTag("Food")
                  .Select(food => food.transform.position).OrderBy(foodPosition => (foodPosition - transform.position).magnitude)
                  .ToList();
        return foodList;
    }
}





