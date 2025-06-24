using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas; // Reference to the game over canvas
    [SerializeField] private GameObject cube;
    public Material[] playerMaterials;
    public Material[] mapMaterials;
    GameObject fog; // Reference to the fog GameObject for enabling/disabling fog effects
    float gameTimer = 60f; 
    [SerializeField]Canvas TimerCanvas; // Reference to the canvas displaying the timer
    [SerializeField] TextMeshProUGUI timerText; // Reference to the text component displaying the timer
    void Start()
    {


        switch (PlayerManager.Instance.mapId)
        {
            case 0: // Space
                RenderSettings.skybox = mapMaterials[0]; // Set the skybox material for the space map
                PlayerManager.Instance.isCube = false; // Set the isCube flag to true for the cube map

                break;
            case 1: // Cube
                RenderSettings.skybox = mapMaterials[0]; // Set the skybox material for the cube map
                PlayerManager.Instance.isCube = true; // Set the isCube flag to true for the cube map
                break;
            case 2: // Ocean
                RenderSettings.skybox = mapMaterials[1]; // Set the skybox material for the ocean map
                PlayerManager.Instance.isCube = false; // Set the isCube flag to true for the cube map

                break;
            case 3: // Mars
                RenderSettings.skybox = mapMaterials[2]; // Set the skybox material for the Mars map
                PlayerManager.Instance.isCube = false; // Set the isCube flag to true for the cube map

                break;
            default:
                Debug.LogError("Invalid map ID selected: " + PlayerManager.Instance.mapId);
                break;
        }
        cube.SetActive(PlayerManager.Instance.isCube); // Ensure the player object is active at the start of the game
        if (PlayerManager.Instance.isCube)
        {
            float mult = Spawner.spawnRadius / 40f;

            cube.transform.localScale = Vector3.one* mult; // Scale the cube based on the spawn radius
        }
        List<Material> playerMaterial = new List<Material>() { playerMaterials[PlayerManager.Instance.skinId] }; // Create a list to hold the map materials
        GameObject.Find("Player").GetComponent<MeshRenderer>().SetMaterials(playerMaterial); // Set the player's material based on the selected skin ID

        fog = GameObject.Find("Fog"); // Find the fog GameObject in the scene
        // Initialize the fog setting
            ParticleSystem.ShapeModule shape = fog.GetComponent<ParticleSystem>().shape;
            shape.scale = new Vector3(Spawner.spawnRadius, Spawner.spawnRadius, Spawner.spawnRadius); // Set the particle system shape scale based on spawn radius
            fog.SetActive(PlayerManager.Instance.fogEnabled); // Set the fog active state based on the fogEnabled variable

        }

    // Update is called once per frame
    void Update()
    {
        if (PlayerManager.Instance.gameMode == 2)
        {
            TimerCanvas.gameObject.SetActive(true); // Show the timer canvas if the game mode is 2 (time-based mode)
            gameTimer -= Time.deltaTime; // Decrease the game timer
            timerText.text = Mathf.CeilToInt(gameTimer).ToString(); // Update the timer text with the remaining time
            if (gameTimer <= 0)
            {
                int score = GameObject.Find("Player").GetComponent<Player>().score; // Get the player's score
                // Find all bots in the scene and sort them by score
                List<Bot> bots = Spawner.botList.FindAll(bot => bot != null); // Find all active bots in the scene
                bots.Sort((a, b) => b.GetComponent<Player>().score.CompareTo(a.GetComponent<Player>().score)); // Sort bots by score in descending order
                
                GameOver(score); // Trigger game over when the timer reaches zero
                if (score > bots[0].GetComponent<Player>().score)
                {
                    LocalPlayer.winner = true; // Set the winner flag for the local player if their score is higher than the highest bot score
                }    }
        }
    }

    public void GameOver(int score) {

        if (score > PlayerManager.Instance.highScore) // Check if the current score is greater than the high score

        {
            PlayerManager.Instance.highScore = score; // Update the high score in PlayerManager
                                                      // PlayerManager.Instance.highScoreText.text = PlayerManager.Instance.nick + " " + score; // Update the high score text in the UI
            PlayerPrefs.SetString("hsNick", PlayerManager.Instance.nick); // Save the player's nickname to PlayerPrefs
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
}




    public void OnPlayAgainButtonClicked()
    {
        // Logic to reset the game state and restart the game
        // This could involve resetting player scores, positions, and any other game state variables
        gameOverCanvas.SetActive(false); // Hide the game over canvas
        Spawner.botList.Clear(); // Clear the list of bots
        Spawner.playerList.Clear(); // Clear the list of players

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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
