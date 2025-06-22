using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayer : MonoBehaviour
{
    public float speedMult = 5f;
    public float speedMultAngle = 0.01f;
    float verticalMove;
    float horizontalMove;
    float lookX;
    float lookY;
    ParticleSystem _particleSystem;
    [SerializeField] GameObject stars;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();

        stars.SetActive(PlayerManager.Instance.starsEnabled); // Enable or disable stars based on trailer setting
    }

    // Update is called once per frame
    void Update()
    {
        verticalMove = InputSystem.actions["Move"].ReadValue<Vector2>().normalized.y;
        horizontalMove = InputSystem.actions["Move"].ReadValue<Vector2>().normalized.x;
        lookX = InputSystem.actions["Look"].ReadValue<Vector2>().normalized.x;
        lookY = InputSystem.actions["Look"].ReadValue<Vector2>().normalized.y;


        if ((verticalMove != 0 || horizontalMove != 0) && PlayerManager.Instance.trailerEnabled)
        {
            _particleSystem.Play(); // Play particle system when moving
        }
        else
        {
            _particleSystem.Stop(); // Stop particle system when not moving
        }
    }
}
