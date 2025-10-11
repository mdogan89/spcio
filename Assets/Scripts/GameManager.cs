using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas; // Reference to the game over canvas
    [SerializeField] private GameObject cube;
    public Material[] playerMaterials;
    public Material[] mapMaterials;
    GameObject fog; // Reference to the fog GameObject for enabling/disabling fog effects
    float gameTimer = 120f; 
    [SerializeField] Canvas TimerCanvas; // Reference to the canvas displaying the timer
    [SerializeField] TextMeshProUGUI timerText; // Reference to the text component displaying the timer
    [SerializeField] Camera secondCam; // Reference to the second camera
    [SerializeField] AudioSource gameOverSound; // Reference to the sound played when the game is over
    [SerializeField] Canvas pauseMenuCanvas;
    Player player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>(); // Find the player object in the scene and get its Player component
        if (player == null)
        {
            Debug.LogError("Player object not found in the scene. Make sure a player is present before GameManager starts.");
            return;
        }

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager instance is not found. Make sure it is initialized before GameManager.");
            return;
        }
        else {
            //Set the map ID  and game mode for how to play scene
            if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                PlayerManager.Instance.mapId = 0; // Set the map ID to 0 if the game mode is 3
                PlayerManager.Instance.gameMode = 3;
            }
            //set the map ID for game scene
            switch (PlayerManager.Instance.mapId)
        {
            case 0: // Space
                RenderSettings.skybox = mapMaterials[0]; // Set the skybox material for the space map
                PlayerManager.Instance.isCube = false; // Set the isCube flag to false for the cube map
                break;
            case 1: // Cube
                RenderSettings.skybox = mapMaterials[0]; // Set the skybox material for the cube map
                PlayerManager.Instance.isCube = true; // Set the isCube flag to true for the cube map
                break;
            case 2: // Ocean
                RenderSettings.skybox = mapMaterials[1]; // Set the skybox material for the ocean map
                PlayerManager.Instance.isCube = false; // Set the isCube flag to false for the cube map
                break;
            case 3: // Mars
                RenderSettings.skybox = mapMaterials[2]; // Set the skybox material for the Mars map
                PlayerManager.Instance.isCube = false; // Set the isCube flag to false for the cube map
                break;
            default:
                Debug.LogError("Invalid map ID selected: " + PlayerManager.Instance.mapId);
                break;
        }
            // set the cube active state based on the isCube flag
            cube.SetActive(PlayerManager.Instance.isCube);
            if (PlayerManager.Instance.isCube)
            {
            float mult = Spawner.spawnRadius / 40f;
            cube.transform.localScale = Vector3.one* mult; // Scale the cube based on the spawn radius
            }
            // Set the fog
            fog = GameObject.Find("Fog"); // Find the fog GameObject in the scene
            ParticleSystem.ShapeModule shape = fog.GetComponent<ParticleSystem>().shape;
            shape.scale = new Vector3(Spawner.spawnRadius, Spawner.spawnRadius, Spawner.spawnRadius); // Set the particle system shape scale based on spawn radius
            fog.SetActive(PlayerManager.Instance.fogEnabled); // Set the fog active state based on the fogEnabled variable
            // set the player material based on the selected skin ID
            List<Material> playerMaterial = new List<Material>() { playerMaterials[PlayerManager.Instance.skinId] }; // Create a list to hold the map materials
            player.GetComponent<MeshRenderer>().SetMaterials(playerMaterial); // Set the player's material based on the selected skin ID

            gameTimer = PlayerManager.Instance.timer; // Set the game timer based on the selected timer value from PlayerManager
            gameOverSound.volume = PlayerManager.Instance.volume; // Set the volume for the game over sound effect
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerManager.Instance.gameMode == 2 && gameTimer>0)
        {
            TimerCanvas.gameObject.SetActive(true); // Show the timer canvas if the game mode is 2 (time-based mode)
            gameTimer -= Time.deltaTime; // Decrease the game timer
            timerText.text = Mathf.CeilToInt(gameTimer).ToString(); // Update the timer text with the remaining time
            if (gameTimer <= 0 && player != null)
            {
                int score = player.Score; // Get the player's score
                // Find all bots in the scene and sort them by score
                List<Bot> bots = Spawner.botList.FindAll(bot => bot != null); // Find all active bots in the scene
                bots.Sort((a, b) => b.GetComponent<Player>().Score.CompareTo(a.GetComponent<Player>().Score)); // Sort bots by score in descending order
                
                if (score > bots[0].GetComponent<Player>().Score)
                {
                    LocalPlayer.winner = true; // Set the winner flag for the local player if their score is higher than the highest bot score
                }
                gameTimer = 0; // Reset the game timer to prevent further updates
                GameOver(score); // Trigger game over when the timer reaches zero
            }
        }
    }
    
    public void GameOver(int score) {
        gameOverSound.Play(); // Play the game over sound effect
#if !UNITY_ANDROID_API && !UNITY_IOS

        Cursor.lockState = CursorLockMode.None; // Lock the cursor to the center of the screen
#endif
        //Camera.main.GetComponent<AudioSource>().PlayDelayed(10f); // Play the main camera's audio source after a delay of 1 second
        if (PlayerManager.Instance.showAds && !PlayerManager.Instance.adsRemoved) { 
        InterstitialAdSample adSample = GameObject.Find("InterstitialAdSample").GetComponent<InterstitialAdSample>(); // Find the InterstitialAdSample component in the scene
        adSample.ShowInterstitialAd(); // Show an interstitial ad when the game is over
        }
        if (score > PlayerManager.Instance.highScore) // Check if the current score is greater than the high score
        {
            PlayerManager.Instance.highScore = score; // Update the high score in PlayerManager                                         
            PlayerPrefs.SetInt("HighScore", PlayerManager.Instance.highScore); // Save the high score to PlayerPrefs
            PlayerPrefs.SetInt(PlayerManager.Instance.nick, score); // Save the score with the player's nickname as the key
        }
        
        gameOverCanvas.gameObject.SetActive(true); // Show the game over canvas
        
        if(LocalPlayer.winner) // Check if the local player is the winner
        {
            gameOverCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "You Win!\nYour Score: " + score; // Update the game over text with the player's score
        }
        else
            gameOverCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Game Over!\nYour Score: " + score; // Update the game over text with the player's score
    
        GameObject joysticks = GameObject.Find("Joysticks"); // Find the Joysticks GameObject in the scene
        if(joysticks != null && joysticks.active) // Check if the Joysticks GameObject exists and disable it
            GameObject.Find("Joysticks").SetActive(false); // Disable the player object

        var playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null) // Check if the player object exists
        {
                Destroy(playerObject); // Destroy the player object     
        }

        if (secondCam != null) // Check if the second camera exists
        {
            secondCam.gameObject.SetActive(true);
        }
    }

    public void OnRestartButtonClicked()
    {
        //var manager = GameObject.Find("PlayerManager");
        //if (manager != null)
        //    Destroy(manager); // Destroy the PlayerManager instance if it exists
        if(pauseMenuCanvas.isActiveAndEnabled)
            pauseMenuCanvas.gameObject.SetActive(false); // Hide the pause menu canvas if it is active
        Time.timeScale = 1f; // Resume the game by setting time scale back to normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene to restart the game
    }
    public void OnResumeButtonClicked()
    {
        // This could involve unpausing the game, hiding the pause menu, and resuming gameplay
        if(pauseMenuCanvas.isActiveAndEnabled)
            pauseMenuCanvas.gameObject.SetActive(false); // Hide the pause menu canvas if it is active
        Time.timeScale = 1f; // Resume the game by setting time scale back to normal
    }

    public void OnPauseButtonClicked()
    {
        // This could involve showing a pause menu, stopping gameplay, and freezing the game state
        if(!pauseMenuCanvas.isActiveAndEnabled)
            pauseMenuCanvas.gameObject.SetActive(true); // Show the pause menu canvas if it is not active
        Time.timeScale = 0f; // Pause the game by setting time scale to zero
    }

    public void OnMenuButtonClicked()
    {
        var manager = GameObject.Find("PlayerManager");
        if (manager != null)
            Destroy(manager); // Destroy the PlayerManager instance if it exists
        SceneManager.LoadScene(0);
    }
    public void OnPlayAgainButtonClicked()
    {
        // Logic to reset the game state and restart the game
        // This could involve resetting player scores, positions, and any other game state variables
        if(gameOverCanvas.active)
            gameOverCanvas.SetActive(false); // Hide the game over canvas
       
        if(pauseMenuCanvas.isActiveAndEnabled)
            pauseMenuCanvas.gameObject.SetActive(false); // Hide the pause menu canvas if it is active

        Spawner.botList.Clear(); // Clear the list of bots
        Spawner.playerList.Clear(); // Clear the list of players
        gameTimer = 180f; // Reset the game timer to its initial value

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnExitButtonClicked()
    {         // Logic to exit the game
        // This could involve saving game state, showing a confirmation dialog, or simply quitting the application
        Application.Quit();
        // If running in the editor, stop playing the scene
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
