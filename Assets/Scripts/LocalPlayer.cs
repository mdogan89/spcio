using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayer : MonoBehaviour
{
    private const float RotationDivisor = 50f;
    private const float DefaultSpeedMultAngle = 0.01f;

    public float speedMult;
    public float speedMultAngle = DefaultSpeedMultAngle;

    ParticleSystem _particleSystem;
    Rigidbody rb;

    private Vector2 moveInput;
    private Vector2 lookInput;

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
        speedMult = GetComponent<Player>().speedMult;
        moveInput = InputSystem.actions["Move"].ReadValue<Vector2>().normalized;
        lookInput = InputSystem.actions["Look"].ReadValue<Vector2>().normalized;

        if ((moveInput != Vector2.zero) && PlayerManager.Instance.trailerEnabled)
            _particleSystem.Play();
        else
            _particleSystem.Stop();

        _particleSystem.startSize = transform.localScale.x;
    }

    private void FixedUpdate()
    {
        if (PlayerManager.Instance.easyControls)
        {
            if (InputSystem.actions["Jump"].triggered)
                TeleportToRandomPosition();
            else
            {
                RotatePlayer();
                MovePlayer();
            }
        }
        else
        {
            if (InputSystem.actions["Jump"].triggered)
                TeleportToRandomPosition();
            else
            {
                ApplyMovementForces();
                ApplyRotationForces();
            }
        }
    }

    private void TeleportToRandomPosition()
    {
        rb.MovePosition(Spawner.GetRandomPosition());
    }

    private void RotatePlayer()
    {
        Vector3 rotation = new Vector3(-lookInput.y, lookInput.x, 0f) * lookSensitivity * speedMultAngle * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
    }

    private void MovePlayer()
    {
        Vector3 move = transform.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y)) * speedMult * moveSensitivity * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    private void ApplyMovementForces()
    {
        Vector3 desiredVelocity = rb.transform.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y)) * speedMult * moveSensitivity;
        Vector3 velocityChange = desiredVelocity - rb.linearVelocity;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void ApplyRotationForces()
    {
        rb.AddTorque(rb.transform.right * speedMultAngle * -lookInput.y * lookSensitivity / RotationDivisor, ForceMode.Acceleration);
        rb.AddTorque(rb.transform.up * speedMultAngle * lookInput.x * lookSensitivity / RotationDivisor, ForceMode.Acceleration);
    }
}
