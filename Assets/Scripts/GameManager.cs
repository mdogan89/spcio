using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas; // Reference to the game over canvas
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameOver(int score) {
    
    gameOverCanvas.gameObject.SetActive(true); // Show the game over canvas
    gameOverCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Game Over!\nYour Score: " + score; // Update the game over text with the player's score
    GameObject.Find("Joysticks").SetActive(false); // Disable the player object

}




    public void OnPlayAgainButtonClicked()
    {
        // Logic to reset the game state and restart the game
        // This could involve resetting player scores, positions, and any other game state variables
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
