using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas; // Reference to the game over canvas
    [SerializeField] private GameObject cube;

    void Start()
    {
        cube.SetActive(PlayerManager.Instance.isCube); // Ensure the player object is active at the start of the game
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
            PlayerPrefs.SetString("PlayerNick", PlayerManager.Instance.nick); // Save the player's nickname to PlayerPrefs
            PlayerPrefs.SetInt("HighScore", PlayerManager.Instance.highScore); // Save the high score to PlayerPrefs
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
