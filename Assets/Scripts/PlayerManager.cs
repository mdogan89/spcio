using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public string nick = "nick";
    [SerializeField] TMP_InputField nickInputField;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    void Update()
    {
        nick = nickInputField.text;
    }

    public void OnStartButtonClicked()
    {
        nick = nickInputField.text;
        SceneManager.LoadScene("GameScene");
    }
}

