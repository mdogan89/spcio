using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bot : MonoBehaviour
{
    Rigidbody rb;
    Vector3 target = Vector3.zero;
    public bool hasTarget = false;
    Player botPlayer;
    float moveTimer = 0f;
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
    
    void MoveToTarget()
    {
        Vector3 direction = (target - rb.position);
        float distance = direction.magnitude;
        if (distance<0.1f)
        {
            hasTarget = false;
            return;
        }
        float speed = botPlayer.speedMult * Mathf.Clamp01(distance / 5f); // Yakla�t�k�a yava�la
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

    // Check is bot not moving longer than 2 seconds
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bot"))
        {
            moveTimer += Time.deltaTime;
            if (moveTimer > 2f)
            {
                FindTarget();
                moveTimer = 0f; // Reset the timer after checking
            }
        }
    }

    void isMoving()
    {
        if (rb.linearVelocity.magnitude < 0.1f)
        {
            hasTarget = false;
            target = Vector3.zero;
        }
    }

}





