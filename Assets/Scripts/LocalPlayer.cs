using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayer : MonoBehaviour
{
    public float speedMult;
    public float speedMultAngle = 0.01f;
    public float verticalMove;
    public float horizontalMove;
    float lookX;
    float lookY;
    ParticleSystem _particleSystem;
    Rigidbody rb;

    public static float lookSensitivity; // Sensitivity for camera rotation
    public static float moveSensitivity = 1f; // Sensitivity for movement
    public float exposure; // Default exposure value for the skybox
    public static bool winner = false; // Flag to indicate if the player is the winner

    void Start()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        GameObject.Find("StarParticles").SetActive(PlayerManager.Instance.starsEnabled); // Enable or disable stars based on stars setting
        lookSensitivity = PlayerManager.Instance.lookSensitivity; // Get the look sensitivity from PlayerManager
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on LocalPlayer. Please ensure it is attached.");
        }
        if(PlayerManager.Instance.mapId == 2 || PlayerManager.Instance.mapId == 3)
        {
            exposure = PlayerManager.Instance.exposure / 2f; // Get the exposure value from PlayerManager
        }
        else
        {
            exposure = PlayerManager.Instance.exposure; // Get the exposure value from PlayerManager
        }
        RenderSettings.skybox.SetFloat("_Exposure", exposure); // Set the skybox exposure for the scene
    }

    void Update()
    {
        speedMult = GetComponent<Player>().speedMult; // Get the speed multiplier from the Player component
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
        _particleSystem.startSize = transform.localScale.x/3;
    }

    private void FixedUpdate()
    {
        if (PlayerManager.Instance.easyControls)
        {
            if(InputSystem.actions["Jump"].triggered) // Check if the jump button is pressed or if jumped flag is set
            {
                rb.MovePosition(Spawner.GetRandomPosition()); // Reset position to a random position in the spawn area
            }
            else {
                //    Move Player based on input with rigidbody move position
                Vector3 v = new Vector3(-lookY, lookX, 0f) * lookSensitivity * speedMultAngle * Time.fixedDeltaTime; // Create a vector for rotation based on look input
                rb.MoveRotation(rb.rotation * Quaternion.Euler(v)); // Rotate the player based on look input **rb needed?
                rb.MovePosition(rb.position + (transform.TransformDirection(new Vector3(horizontalMove, 0f, verticalMove)) * speedMult * moveSensitivity * Time.fixedDeltaTime)); // Move the player based on input
            }
        }

        else
        {
            if (InputSystem.actions["Jump"].triggered) // Check if the jump button is pressed or if jumped flag is set
            {
                rb.MovePosition(Spawner.GetRandomPosition()); // Reset position to a random position in the spawn area
            }
            else { 
                rb.AddForce(rb.transform.TransformDirection(Vector3.forward) * verticalMove * speedMult * moveSensitivity, ForceMode.Acceleration);
                rb.AddForce(rb.transform.TransformDirection(Vector3.right) * horizontalMove * speedMult * moveSensitivity, ForceMode.Acceleration);
                rb.AddTorque(rb.transform.right * speedMultAngle * lookY * -1 * lookSensitivity /50, ForceMode.Acceleration);
                rb.AddTorque(rb.transform.up * speedMultAngle * lookX * lookSensitivity /50, ForceMode.Acceleration);
            }
        }
    }
}
