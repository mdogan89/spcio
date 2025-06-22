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
    Rigidbody rb;

    public static float lookSensitivity = 1f; // Sensitivity for camera rotation
    public static float moveSensitivity = 1f; // Sensitivity for movement

    public static float exposure = 3.0f; // Default exposure value for the skybox

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();

        stars.SetActive(PlayerManager.Instance.starsEnabled); // Enable or disable stars based on stars setting

        rb = GetComponent<Rigidbody>();

        RenderSettings.skybox.SetFloat("_Exposure", exposure); // Set the skybox exposure for the scene

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
    private void FixedUpdate()
    {
        if (PlayerManager.Instance.easyControls)
        {
            //    Move Player based on input with rigidbody move position
            Vector3 v = new Vector3(-lookY, lookX, 0f) * lookSensitivity * speedMultAngle * Time.fixedDeltaTime; // Create a vector for rotation based on look input
            rb.MoveRotation(rb.rotation * Quaternion.Euler(v)); // Rotate the player based on look input **rb needed?
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
}
