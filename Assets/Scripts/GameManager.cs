using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas; // Reference to the game over canvas
    [SerializeField] private GameObject cube;
    public Material[] playerMaterials;
    public Material[] mapMaterials;


    void Start()
    {
        cube.SetActive(PlayerManager.Instance.isCube); // Ensure the player object is active at the start of the game
        if (PlayerManager.Instance.isCube)
        {
            float mult = Spawner.spawnRadius / 40f;

            cube.transform.localScale = Vector3.one* mult; // Scale the cube based on the spawn radius
        }
        List<Material> playerMaterial = new List<Material>() { playerMaterials[PlayerManager.Instance.skinId] }; // Create a list to hold the map materials
        GameObject.Find("Player").GetComponent<MeshRenderer>().SetMaterials(playerMaterial); // Set the player's material based on the selected skin ID

        switch (PlayerManager.Instance.mapId)
        {
            case 0: // Space
                RenderSettings.skybox = mapMaterials[0]; // Set the skybox material for the space map
                break;
            case 1: // Cube
                // RenderSettings.skybox = mapMaterials[0]; // Set the skybox material for the cube map
                break;
            case 2: // Ocean
                RenderSettings.skybox = mapMaterials[1]; // Set the skybox material for the ocean map
                break;
            case 3: // Mars
                RenderSettings.skybox = mapMaterials[2]; // Set the skybox material for the Mars map
                break;
            default:
                Debug.LogError("Invalid map ID selected: " + PlayerManager.Instance.mapId);
                break;
        }



    }

    // Update is called once per frame
    void Update()
    {
        
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
    gameOverCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Game Over!\nYour Score: " + score; // Update the game over text with the player's score
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
