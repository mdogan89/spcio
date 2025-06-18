using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{
    public Rigidbody rb;
    public float speedMult = 5f;
    [SerializeField] float speedMultAngle = 0.01f;
    float verticalMove;
    float horizontalMove;
    float lookX;
    float lookY;

    Camera cam;

   public Color _color = Color.white; // Default color

    public int score = 0;
    public int _Size = 1;

    public string nick = "nick";

    [SerializeField] TextMeshProUGUI nickText;
    [SerializeField] TextMeshProUGUI scoreText;
    Bot bot;
    StarterAssetsInputs starterAssetsInputs;

    public Camera _cam;


    void Start()
    {



        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        //if(PlayerManager.Instance.nick == null || PlayerManager.Instance.nick == "")
        //{
        //    PlayerManager.Instance.nick = "Player" + Random.Range(1000, 9999); // Assign a default nickname if none is set
        //}
        bot = GetComponent<Bot>();
        if (bot == null)
        {
          //  starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            nick = PlayerManager.Instance.nick; // Get the player's nickname from PlayerManager
            GetComponentInChildren<MeshRenderer>().material = PlayerManager.Instance.playerMaterial; // Set the player's material from PlayerManager
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





    }

    private void Update()
    {
        verticalMove = InputSystem.actions["Move"].ReadValue<Vector2>().normalized.y;
        horizontalMove = InputSystem.actions["Move"].ReadValue<Vector2>().normalized.x;
        lookX = InputSystem.actions["Look"].ReadValue<Vector2>().normalized.x;
        lookY = InputSystem.actions["Look"].ReadValue<Vector2>().normalized.y;

        //if (bot == null && starterAssetsInputs != null)
        //{


        //    verticalMove = starterAssetsInputs.move.normalized.y;
        //    horizontalMove = starterAssetsInputs.move.normalized.x;
        //    lookX = starterAssetsInputs.look.normalized.x;
        //    lookY = starterAssetsInputs.look.normalized.y;
        //}
        //else
        //{
        //    //Debug.LogError("StarterAssetsInputs component not found on Player object.");
        //}

        // Update the score text UI element
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        else
        {
            Debug.LogError("Score Text UI element is not assigned.");
        }

        if(bot == null)
        {
           // transform.rotation = Quaternion.Euler(lookY * 10, lookX * 10, 0); // Adjust camera rotation based on look input
            //_cam.transform.rotation = Quaternion.Euler(lookY * 10, lookX * 10, 0); // Keep the camera rotation aligned with the player
        }

        if (_Size >= 2000)
        {
            _Size = 2000; // Cap the size to prevent excessive scaling
        }


    }


    private void FixedUpdate()
    {   
       
        rb.AddForce(rb.transform.TransformDirection(Vector3.forward) * verticalMove * speedMult, ForceMode.Acceleration);
        rb.AddForce(rb.transform.TransformDirection(Vector3.right) * horizontalMove * speedMult, ForceMode.Acceleration);

        rb.AddTorque(rb.transform.right * speedMultAngle * lookY * -1, ForceMode.Acceleration);
        rb.AddTorque(rb.transform.up * speedMultAngle * lookX, ForceMode.Acceleration);
    }



    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            var food = other.GetComponent<Food>();
            if (food != null)
            {
                eatFood(food);
                UpdateSize(); // Update the size of the player
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
            if(collidedBot == null)
            {
                if(score < collidedPlayer.score)
                {
                    AbsorbPlayer(collidedPlayer);
                }
                else if (score > collidedPlayer.score)
                {
                    GameManager gameManager = FindObjectOfType<GameManager>();
                    gameManager.GameOver(collidedPlayer.score); // Trigger game over if the player is absorbed
                }
            }

            if (collidedPlayer != null && score > collidedPlayer.score)
            {
                AbsorbPlayer(collidedPlayer);
            }
        }
    }
    void eatFood(Food food)
    {
        score += 100; // Increase score
        _Size += 12; // Increase size
        UpdateSize(); // Update the size of the player
        UpdateSpeed(); // Update speed based on new size

        UpdateCam(); // Update camera position based on new size

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
        score += other.score / 10; // Example of increasing score --if other.score == 0 score += 10 
        _Size += other._Size / 10; // Example of increasing size
        other.score = 0; // Reset collided player's score
        other._Size = 1; // Reset collided player's size
        UpdateSize(); // Example of increasing size
        UpdateSpeed(); // Update speed based on new size
        UpdateCam(); // Update camera position based on new size
        other.speedMult = 5f;
        other.transform.localScale = new Vector3(1, 1, 1); // Reset collided player's size
        other.GetComponent<Rigidbody>().MovePosition(Spawner.GetRandomPosition()); // Reset collided player's position
        Debug.Log("Collided with Bot! Score: " + score);

        if (other.GetComponent<Bot>() != null)
            other.GetComponent<Bot>().hasTarget = false; // Reset target for the collided bot
        if (bot != null)
            bot.hasTarget = false; // Reset target for the player bot
    }
    /// <summary>
    /// Oyuncunun hýzýný günceller.
    /// </summary>
    private void UpdateSpeed()
    {
        speedMult = (_Size / Mathf.Pow(_Size, 1.1f)) * 5;
    }

    private void UpdateCam()
    {
        //if(bot ==null)
           // cam.transform.localPosition = new Vector3(0, 0.5f + (_Size / 1000f), -(_Size / 1000f) * 2.5f);
          // cam.fieldOfView = 60 + (_Size / 1000f) * 10; // Adjust the field of view based on size

    }

   public void OnJumpButtonClicked()
    {
        rb.MovePosition(Vector3.zero); // Reset position to the center of the map
    }
}
