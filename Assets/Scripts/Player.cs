using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Player : MonoBehaviour
{
    public float speedMult = 5f;

    public Color _color = Color.white; // Default color

    public int score = 0;
    public int _Size = 1;

    public string nick = "nick";

    [SerializeField] TextMeshProUGUI nickText;
    [SerializeField] TextMeshProUGUI scoreText;
    Bot bot;
    Animator _animator;
    GameManager gameManager;

    void Start()
    {
      //  rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();

        bot = GetComponent<Bot>();
        if (bot == null)
        {
            _animator = GetComponent<Animator>();


            if (PlayerManager.Instance.nick == null || PlayerManager.Instance.nick == "")
            {
                PlayerManager.Instance.nick = "Player" + Random.Range(1000, 9999); // Assign a default nickname if none is set
                nick = PlayerManager.Instance.nick; // Set the player's nickname to the default
            }
            else { 
            nick = PlayerManager.Instance.nick; // Get the player's nickname from PlayerManager
            }
        }

        // Update the UI text elements with the player's nickname and score
        if (nickText != null)
        {
            nickText.text = nick;
        }
        else
        {
            Debug.LogError("Nick Text UI element is not assigned.");
        }


        if (!PlayerManager.Instance.thirdPersonView&& SceneManager.GetActiveScene().name != "HowToPlay")
        {
            

            if(bot == null)
            {
                Camera.main.transform.position = transform.position; // Adjust camera position for first-person view
                GetComponentInChildren<Canvas>().GetComponent<Transform>().position = GetComponentInChildren<Canvas>().GetComponent<Transform>().position + new Vector3(0, 0, 5); // Adjust canvas position for first-person view
                scoreText.color = Color.white; // Set score text color to white for first-person view
                nickText.color = Color.white; // Set nickname text color to white for first-person view
                PlayerManager.Instance.trailerEnabled = false; // Disable trailer effect for first-person view
            }
        }

    }

    private void Update()
    {

        // Update the score text UI element
        if (scoreText != null&&SceneManager.GetActiveScene().name != "HowToPlay")
        {
            scoreText.text = score.ToString();
        }
        //else
        //{
        //    Debug.LogError("Score Text UI element is not assigned.");
        //}

        if (_Size >= 2000 && PlayerManager.Instance.gameMode != 1) // game mode?
        {
            _Size = 2000; // Cap the size to prevent excessive scaling
            if (bot == null)
            {
                LocalPlayer.winner = true; // Set the winner flag for the local player
                gameManager.GameOver(score); // Trigger game over if the player exceeds the size limit
            }

        }
        if(PlayerManager.Instance.gameMode == 1)
        {
            GameObject[] survivedBots = GameObject.FindGameObjectsWithTag("Bot");
            if (survivedBots.Length == 0)
            {
                LocalPlayer.winner = true; // Set the winner flag for the local player
                gameManager.GameOver(score); // Trigger game over if all bots are destroyed
                
            }
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            var food = other.GetComponent<Food>();
            if (food != null)
            {
                eatFood(food);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bot"))
        {
            Player collidedPlayer = collision.gameObject.GetComponent<Player>();
            //if(player.score == score)
            var collidedBot = collidedPlayer.GetComponent<Bot>();

            // Bot collided with player
            if (collidedBot == null)
            {
                if(score < collidedPlayer.score)
                {
                    collidedPlayer.AbsorbPlayer(this);
                }
                else if (score > collidedPlayer.score)
                {
                    gameManager.GameOver(collidedPlayer.score); // Trigger game over if the player is absorbed
                }
            }
            // Bot collided with another bot
            else if (collidedBot != null && bot != null)
            {
                if (score < collidedPlayer.score)
                {
                    collidedPlayer.AbsorbPlayer(this); // Absorb the player if the collided bot has a higher score
                }
                else if (score > collidedPlayer.score)
                {
                AbsorbPlayer(collidedPlayer); // Absorb the collided player if the player has a higher score
                }
            }
        }
    }
    void eatFood(Food food)
    {
        score += 100; // Increase score
        _Size += 12; // Increase size
        if (bot == null) { 
            _animator.SetTrigger("Eat"); // Trigger eating animation if not a bot
            if(PlayerManager.Instance.vibration)
                Handheld.Vibrate(); // Vibrate the device when eating food
        }
        UpdateSize(); // Update the size of the player
        UpdateSpeed(); // Update speed based on new size

        Vector3 position = Spawner.GetRandomPosition(); // Get a random position for the food
        food.transform.position = position; // Move food to a new random position

        if (bot != null)
            bot.hasTarget = false; // Reset target for the bot if it exists

    }
    void UpdateSize()
    {
        transform.localScale = Vector3.one + Vector3.one * 1000 * (_Size / 65535f);
    }

    void AbsorbPlayer(Player other)
    {
        if (other.score == 0)
        {
            score += 50;
        }
        else
        {
            score += other.score; // Example of increasing score
        }
        _Size += other._Size; // Example of increasing size
        UpdateSize(); // Example of increasing size
        UpdateSpeed(); // Update speed based on new size
        if (PlayerManager.Instance.gameMode == 1)
        {
            Destroy(other.gameObject); // Destroy the other player if in survival mode
            Spawner.botList.Remove(other.GetComponent<Bot>()); // Remove the absorbed bot from the bot list
            Spawner.playerList.Remove(other); // Remove the absorbed player from the player list
        }
        else
        {
            other.score = 0; // Reset collided player's score
            other._Size = 1; // Reset collided player's size
            other.speedMult = 5f;
            other.transform.localScale = new Vector3(1, 1, 1); // Reset collided player's size
            other.GetComponent<Rigidbody>().MovePosition(Spawner.GetRandomPosition()); // Reset collided player's position
            if (other.GetComponent<Bot>() != null)
                other.GetComponent<Bot>().hasTarget = false; // Reset target for the collided bot
        }
        if (bot != null)
            bot.hasTarget = false; // Reset target for the player bot

        if (bot == null) { 
            _animator.SetTrigger("Absorb"); // Trigger absorb animation if the player is absorbed
            if(PlayerManager.Instance.vibration)
                Handheld.Vibrate(); // Vibrate the device when absorbing another player
           
        }
    }

    private void UpdateSpeed()
    {
        speedMult = (_Size / Mathf.Pow(_Size, 1.1f)) * 5;
    }

    public void OnMenuButtonClicked()
    {
        Destroy(GameObject.Find("PlayerManager")); // Destroy PlayerManager to reset player data
        SceneManager.LoadScene("Title"); // Load the title scene when the menu button is clicked
    }
}
