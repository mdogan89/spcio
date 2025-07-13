using System.Collections.Generic;
using System.Linq;
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
    public static bool winner = false; // Flag to indicate if the player is the winner
    public AudioSource audioSource; // Audio source for the player
    public AudioSource teleportAudio; // Audio source for teleportation sound
    //public Transform zeroPoint; // Reference point for teleportation
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
    }

    void Update()
    {
        speedMult = GetComponent<Player>().SpeedMult;
        moveInput = InputSystem.actions["Move"].ReadValue<Vector2>().normalized;
        lookInput = InputSystem.actions["Look"].ReadValue<Vector2>().normalized;

#if !UNITY_ANDROID_API && !UNITY_IOS
        
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None; // Unlock the cursor when Escape is pressed
        }

        Vector2 vector2 = Vector2.zero;
        vector2.x = Input.mousePositionDelta.x;
        vector2.y = Input.mousePositionDelta.y;
        lookInput += vector2;
#endif

        if ((moveInput != Vector2.zero) && PlayerManager.Instance.trailerEnabled)
            _particleSystem.Play();
        else
            _particleSystem.Stop();

        _particleSystem.startSize = transform.localScale.x;
        teleportAudio.volume = PlayerManager.Instance.volume; // Set the teleport audio volume based on the saved volume level
        audioSource.volume = PlayerManager.Instance.volume; // Set the audio source volume based on the saved volume level
        if(moveInput == Vector2.zero)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }




    }

    private void FixedUpdate()
    {
        if (PlayerManager.Instance.easyControls)
        {
            if (InputSystem.actions["Jump"].triggered)
                TeleportToRandomPosition();
            else if (InputSystem.actions["Sprint"].IsPressed())
            {
                MoveToClosestFood();
            }
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

    private void MoveToClosestFood()
    {
        Vector3 closestFood = GetFoodList()[0];
        if (closestFood != null)
        {
            //Vector3 move = closestFood.normalized * speedMult * moveSensitivity * Time.fixedDeltaTime;
            //rb.MovePosition(rb.position + move);
            //rb.MoveRotation(Quaternion.LookRotation(closestFood - rb.position));
            //rb.Move(closestFood.normalized * speedMult * moveSensitivity * Time.fixedDeltaTime,Quaternion.identity);
            Vector3 direction = (closestFood - rb.position).normalized;
            float speed = speedMult * moveSensitivity;

            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * Quaternion.Euler(direction * lookSensitivity * speedMultAngle * Time.fixedDeltaTime));
            
        }
    }
    List<Vector3> GetFoodList()
    {
        var foods = GameObject.FindGameObjectsWithTag("Food");
        if (foods == null || foods.Length == 0) return new List<Vector3>();
        return foods.Select(f => f.transform.position)
                    .OrderBy(pos => Vector3.Distance(transform.position, pos))
                    .ToList();
    }


    private void TeleportToRandomPosition()
    {
        teleportAudio.Play(); // Play the teleport sound effect
        rb.MovePosition(Spawner.GetRandomPosition());
        // creat zero point of map for rotation


        //Spawner spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
        //Quaternion vector4 = spawner.transform.rotation; // Get the rotation of the zero poinT
        //Vector3 vector3 = (Vector3.zero - transform.position).normalized; // Get the position of the zero point relative to the player
        // Set the rotation of the player to match the zero point
        //rb.MoveRotation(Quaternion.FromToRotation(transform.position,spawner.transform.position));
        //rb.MoveRotation(rb.rotation * Quaternion.Euler(vector3));
    }

    private void RotatePlayer()
    {
        Vector3 rotation = new Vector3(-lookInput.y, lookInput.x, 0f) * lookSensitivity * speedMultAngle * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
    }

    private void MovePlayer()
    {
        Vector3 move = transform.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y)) * speedMult * moveSensitivity * Time.fixedDeltaTime;
        // Ensure player does not move faster than the speed limit diagonally **necessary?**
        if (move.magnitude > speedMult * moveSensitivity * Time.fixedDeltaTime)
        {
            move = move.normalized * (speedMult * moveSensitivity * Time.fixedDeltaTime);
        }

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
