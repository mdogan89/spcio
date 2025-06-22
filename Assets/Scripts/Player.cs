using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;
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
    GameObject fog; // Reference to the fog GameObject for enabling/disabling fog effects
    Bot bot;

    public static float lookSensitivity = 1f; // Sensitivity for camera rotation
    public static float moveSensitivity = 1f; // Sensitivity for movement

    public static float exposure = 3.0f; // Default exposure value for the skybox

    void Start()
    {
        fog = GameObject.Find("Fog"); // Find the fog GameObject in the scene
         // Find the stars GameObject in the scene
        if (bot == null) {

            // Initialize the fog setting

            ParticleSystem.ShapeModule shape = fog.GetComponent<ParticleSystem>().shape;
        shape.scale = new Vector3(Spawner.spawnRadius, Spawner.spawnRadius, Spawner.spawnRadius); // Set the particle system shape scale based on spawn radius
        fog.SetActive(PlayerManager.Instance.fogEnabled); // Set the fog active state based on the fogEnabled variable


        }






        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        bot = GetComponent<Bot>();
        if (bot == null)
        {
          //  starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            if(PlayerManager.Instance.nick == null || PlayerManager.Instance.nick == "")
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

        RenderSettings.skybox.SetFloat("_Exposure", exposure); // Set the skybox exposure for the scene

        if (!PlayerManager.Instance.thirdPersonView)
        {
            

            if(bot == null)
            {
                Camera.main.transform.position = Camera.main.transform.position + new Vector3(0, 0, 3f); // Adjust camera position for first-person view
                GetComponentInChildren<Canvas>().GetComponent<Transform>().position = GetComponentInChildren<Canvas>().GetComponent<Transform>().position + new Vector3(0, 0, 5); // Adjust canvas position for first-person view
                scoreText.color = Color.white; // Set score text color to white for first-person view
                nickText.color = Color.white; // Set nickname text color to white for first-person view
            }
        }

    }

    private void Update()
    {



        // Update the score text UI element
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        else
        {
            Debug.LogError("Score Text UI element is not assigned.");
        }

        if (_Size >= 2000)
        {
            _Size = 2000; // Cap the size to prevent excessive scaling
        }

        verticalMove = InputSystem.actions["Move"].ReadValue<Vector2>().normalized.y;
        horizontalMove = InputSystem.actions["Move"].ReadValue<Vector2>().normalized.x;
        lookX = InputSystem.actions["Look"].ReadValue<Vector2>().normalized.x;
        lookY = InputSystem.actions["Look"].ReadValue<Vector2>().normalized.y;
    }


    private void FixedUpdate()
    {
        if (PlayerManager.Instance.easyControls)
        {
            //    Move Player based on input with rigidbody move position
            Vector3 v = new Vector3(-lookY,lookX, 0f) * lookSensitivity * speedMultAngle *Time.fixedDeltaTime; // Create a vector for rotation based on look input
            rb.MoveRotation(rb.rotation * Quaternion.Euler(v)); // Rotate the player based on look input
            rb.MovePosition(rb.position + (transform.TransformDirection(new Vector3(horizontalMove, 0f, verticalMove)) * speedMult * moveSensitivity * Time.fixedDeltaTime)); // Move the player based on input
            GameManager gameManager = FindObjectOfType<GameManager>();
            //// Update the map rotation based on player rotation

            //float rotationY = rb.rotation.eulerAngles.y; // Get the player's rotation around the Y-axis
            
            //RenderSettings.skybox.SetFloat("_Rotation", rotationY); // Set the skybox rotation to match the player's rotation
        }
        else
        {
            rb.AddForce(rb.transform.TransformDirection(Vector3.forward) * verticalMove * speedMult * moveSensitivity, ForceMode.Acceleration);
            rb.AddForce(rb.transform.TransformDirection(Vector3.right) * horizontalMove * speedMult * moveSensitivity, ForceMode.Acceleration);

            rb.AddTorque(rb.transform.right * speedMultAngle * lookY * -1 * lookSensitivity, ForceMode.Acceleration);
            rb.AddTorque(rb.transform.up * speedMultAngle * lookX * lookSensitivity, ForceMode.Acceleration);
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
                    GameManager gameManager = FindObjectOfType<GameManager>();
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

    public void OnMenuButtonClicked()
    {
        Destroy(GameObject.Find("PlayerManager")); // Destroy PlayerManager to reset player data
        SceneManager.LoadScene("Title"); // Load the title scene when the menu button is clicked
    }



}
