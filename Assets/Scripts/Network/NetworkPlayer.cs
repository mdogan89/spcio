using Fusion;
using Fusion.Addons.Physics;
using Fusion.Addons.SimpleKCC;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 20f;

    [Networked]
    public float speed { get => default; set { } }

    public static NetworkPlayer Local { get; set; }
    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI playerNickNameTM;
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [Header("Sprite")]
    public MeshRenderer playerSpriteRenderer;
    [Networked ]
    public NetworkString<_16> nickName { get => default; set { } }

    InGameUIHandler inGameUIHandler;

    [SerializeField] NetworkRigidbody3D _rigidbody;

    public enum PlayerState
    {
        pendingConnect,
        connected,
        playing,
        dead
    }
    [Networked]
    public PlayerState playerState { get => default; set { } }

    [Networked]
    public Color spriteColor { get; set; }

    [Networked]
    public ushort size { get => default; set { } }

    private NetworkInputManager inputManager;
    private Vector2 baseLookRotation;
    private ChangeDetector _changeDetector;
    public Vector2 inputDirection = Vector2.zero;
    public Vector2 inputLook = Vector2.zero;

    [Networked]
    public bool isBot { get => default; set { } }
    public Vector3 aiTarget = Vector3.zero;
    NetworkSpawner networkSpawner;
    bool enableSpriteRenderer = true;
    TickTimer delayedEnabledSpriteRenderer = new TickTimer();
    public bool hasTarget = false;

    void Awake()
    {
        inGameUIHandler = FindObjectOfType<InGameUIHandler>();
        networkSpawner = FindObjectOfType<NetworkSpawner>();

        // Nickname için
        if (playerNickNameTM == null)
        {
            foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.gameObject.name.ToLower().Contains("nick"))
                {
                    playerNickNameTM = tmp;
                    break;
                }
            }
        }

        // Skor için
        if (scoreText == null)
        {
            foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.gameObject.name.ToLower().Contains("score"))
                {
                    scoreText = tmp;
                    break;
                }
            }
        }
        Debug.Log($"Awake: {gameObject.name}, playerNickNameTM null? {playerNickNameTM == null}, scoreText null? {scoreText == null}");
    }
    void Start()
    {
        inputDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        Reset();
        UpdateSize();
    }

    public override void Spawned()
    {
        //kcc.SetGravity(Physics.gravity.y * 0f);
        //kcc.SetShape(EKCCShape.None, 0, 0);
        Runner.SetPlayerObject(Object.InputAuthority, Object);
        if (HasInputAuthority)
        {
            inputManager = Runner.GetComponent<NetworkInputManager>();
            inputManager.LocalPlayer = this;
            Local = this;
            CameraFollow.Singleton.SetTarget(camTarget);
            //kcc.Settings.ForcePredictedLookRotation = true;
        }
        if (Object.HasStateAuthority)
            spriteColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.5f, 1f);
        playerSpriteRenderer.material.color = spriteColor;
        ////ResetCells();

        transform.name = $"P_{Object.Id}";
        // ChangeDetector'ı doğru şekilde başlat

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        
        
        if (isBot && HasStateAuthority)
        {
            nickName = Utils.GetRandomName();
        //    playerNickNameTM.text = nickName.ToString();
        }
        else if (Object.HasInputAuthority)
        {
            nickName = PlayerManager.Instance.nick;
        }
    }

    public override void FixedUpdateNetwork()
    {


        if (HasStateAuthority) {
            Vector3 scale = Vector3.one + Vector3.one * 1000 * (size / 65535f);
            transform.localScale = scale;

            if (playerState == PlayerState.playing)
            {
                _rigidbody.enabled = true;
            }
            else
            {
                _rigidbody.enabled = false;
            }
        }



        //if (delayedEnabledSpriteRenderer.Expired(Runner))
        //{
        //    playerSpriteRenderer.gameObject.SetActive(enableSpriteRenderer);

        //    if (Object.HasInputAuthority && enabled) { 
        //        Camera.main.transform.position = transform.position;
            
        //}
        //    delayedEnabledSpriteRenderer = TickTimer.None;

        //}

        //Movement **check if player inside the game area**
        if (GetInput(out NetInput netInput))
        {
            inputDirection = netInput.Direction;
            inputLook = netInput.LookDelta;
        }
        if (Object.HasStateAuthority)
        {
            //bot movement
            if (isBot)
            {
                //if ai target is null find a target
                if (hasTarget == false || aiTarget == Vector3.zero)
                {
                    Debug.Log("Has no target, finding food..." + hasTarget);
                    FindFood();
                }
                //if ai target is not null move towards it
                else
                {
                    speed = (size / Mathf.Pow(size, 1.1f)) * 5;
                    //transform.localScale = Vector3.one + Vector3.one * 1000 * (size / 65535);
                    Vector3 vector3 = aiTarget - transform.position;
                    float distance = vector3.magnitude;
                   // float _speed = speed * Mathf.Clamp01(distance / 5f); // Yakla�t�k�a yava�la
                    _rigidbody.Rigidbody.MovePosition(_rigidbody.Rigidbody.position + vector3.normalized * speed * Runner.DeltaTime);
                    // _rigidbody.Rigidbody.MovePosition(_rigidbody.Rigidbody.position + vector3.normalized * speed * Runner.DeltaTime);
                    //baseLookRotation = kcc.GetLookRotation();
                    if(distance<0.1f)
                        hasTarget = false; // Reset target if reached


                }
            }
            //Player movement
            else
            {
                speed = (size / Mathf.Pow(size, 1.1f)) * 5;
                //transform.localScale = Vector3.one + Vector3.one * 1000 * (size / 65535);
                Vector3 rotation = new Vector3(-netInput.LookDelta.y, netInput.LookDelta.x, 0f) * lookSensitivity * Runner.DeltaTime;
                _rigidbody.Rigidbody.MoveRotation(_rigidbody.Rigidbody.rotation * Quaternion.Euler(rotation));
                UpdateCamTarget();
                //Vector3 worldDirection = kcc.LookRotation * new Vector3(input.Direction.x, 0, input.Direction.y);
                Vector3 move = transform.TransformDirection(new Vector3(netInput.Direction.x, 0, netInput.Direction.y)) * speed * Runner.DeltaTime ;
                _rigidbody.Rigidbody.MovePosition(_rigidbody.Rigidbody.position + move);

                //kcc.Move(move);
                //baseLookRotation = kcc.GetLookRotation();

            }
        }

        if (Object.HasInputAuthority && inGameUIHandler != null)
        {
            inGameUIHandler.SetConnectionType(Runner.CurrentConnectionType.ToString());
            inGameUIHandler.SetRtt($"RTT {Mathf.RoundToInt((float)Runner.GetPlayerRtt(Object.InputAuthority) * 100)} ms");
        }
        // Change detection kontrolü için yeni yaklaşım

        if (_changeDetector != null)
            {

                CheckStateChanges();

            }




    }
    void FindFood()
    {

        var foodList = GetFoodList();
        Debug.Log($"Food list count: {foodList.Count}");
        if (foodList.Count > 0)
        {
            aiTarget = foodList[0];
            hasTarget = true;
            Debug.Log($"AI target set to: {aiTarget}");
            Debug.Log($"hasTarget set to: {hasTarget}");
        }
    }
    List<Vector3> GetFoodList()
    {
        var foods = GameObject.FindGameObjectsWithTag("Cell");
        if (foods == null || foods.Length == 0) return new List<Vector3>();
        return foods.Select(f => f.transform.position)
                    .OrderBy(pos => Vector3.Distance(transform.position, pos))
                    .ToList();
    }

    public override void Render()
    {
        //if (kcc.Settings.ForcePredictedLookRotation)
        //{
        //    //Vector2 predictedLookRotation = baseLookRotation + inputManager.AccumulatedMouseDelta * lookSensitivity;
        //    //kcc.SetLookRotation(predictedLookRotation);
        //}
        UpdateCamTarget();
        if (playerNickNameTM != null && scoreText != null)
        {
            playerNickNameTM.text = nickName.ToString();
            scoreText.text = size.ToString();
        }
    }


    private void UpdateCamTarget()
    {
        camTarget.transform.rotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cell") && HasStateAuthority)
        {
            OnCollectFood(12);
            other.gameObject.GetComponent<NetworkTransform>().transform.position = Utils.GetRandomPosition();
        }

        if (other.GetComponent<NetworkPlayer>() && HasStateAuthority)
        {
            NetworkPlayer player = other.GetComponent<NetworkPlayer>();
            if (player.size == size)
                return;
            if (player.size > size)
            {
                Debug.Log(nickName + "collided with bigger player" + player.nickName);
                float foodFromOtherPlayer = size * 0.1f;

                if (foodFromOtherPlayer < 20)
                {
                    foodFromOtherPlayer = 20;
                }
                player.OnCollectFood((ushort)foodFromOtherPlayer);

                OnPlayerDead();
                //  Runner.Despawn(player.Object);
            }
            //else
            //{
            //    Player oPlayer = other.GetComponentInParent<Player>();
            //    oPlayer.OnPlayerDead();
            //    // Runner.Despawn(oPlayer.Object);
            //}
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
            Runner.Despawn(Object);
    }

    public void JoinGame(string nickname)
    {
        Debug.Log($"JoinGame called with nickname: {nickname}");
       // if(HasInputAuthority)
            RPC_JoinGame(nickname);
    }

    public void BotJoinGame()
    {
        Debug.Log("Bot joined game");
        ResetPlayer();
    }


    void OnNickNameChanged()
    {

        Debug.Log($"Nickname changed for player to {nickName} for player {gameObject.name}");
        Debug.Log($"OnNickNameChanged: {gameObject.name}, playerNickNameTM null? {playerNickNameTM == null}");
        playerNickNameTM.text = nickName.ToString();

    }

    void OnColorChanged()
    {
        playerSpriteRenderer.material.color = spriteColor;
    }

    void OnSizeChanged(ushort s)
    {
        UpdateSize();
        speed = (size / Mathf.Pow(size, 1.1f)) * 5;
        scoreText.text = s.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_JoinGame(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] RPC_JoinGame {nickName}");
        this.nickName = nickName;

        ResetPlayer();
    }

    public void OnPlayerDead()
    {
        playerState = PlayerState.dead;

        if (!isBot)
            inGameUIHandler.OnPlayerDied();

        if (isBot)
        {
            networkSpawner.OnBotDied(this);
        }
    }




    void OnPlayerStateChanged()
    {
        if (playerState == PlayerState.playing)
        {
            enableSpriteRenderer = true;
            if (!delayedEnabledSpriteRenderer.IsRunning)
                delayedEnabledSpriteRenderer = TickTimer.CreateFromSeconds(Runner, 0.1f);
        }
        else
        {
            enableSpriteRenderer = false;
            if (!delayedEnabledSpriteRenderer.IsRunning)
                delayedEnabledSpriteRenderer = TickTimer.CreateFromSeconds(Runner, 0.1f);

            if (playerState == PlayerState.dead && Object.HasInputAuthority)
            {
                inGameUIHandler.OnPlayerDied();
            }
        }
    }

    public void Reset()
    {
        size = 1;
        speed = 5;
        transform.localScale = Vector3.one;
        playerState = PlayerState.playing;
        hasTarget = false;
    }
    void UpdateSize()
    {
        //meshrenderer
        transform.localScale = Vector3.one + Vector3.one * 1000 * (size / 65535f);
    }
   public void OnCollectFood(ushort growSize)
    {
        size += growSize;
        //speed -= speed / 100;
        speed = (size / Mathf.Pow(size, 1.1f)) * 5;
        UpdateSize();
        hasTarget = false;
        //scoreText.text = size.ToString();
    }

    public void ResetPlayer()
    {
        //if (HasStateAuthority)
        //{
            Debug.Log("Reset Player called");
            playerState = PlayerState.playing;
            Vector3 newPosition = Utils.GetRandomPosition();
            //enableSpriteRenderer = true;

            _rigidbody.Rigidbody.MovePosition(newPosition);

            //kcc.enabled = true;

            size = 1;
            speed = 5;
            transform.localScale = Vector3.one;
            hasTarget = false;
        //}
    }
    private void CheckStateChanges()
    {
        var changes = _changeDetector.DetectChanges(this);

            foreach (var change in changes)
            {
                switch (change)
                {
                case nameof(nickName):
                    OnNickNameChanged();
                    break;
                case nameof(playerState):
                    OnPlayerStateChanged();
                    break;
                case nameof(spriteColor):
                        OnColorChanged();
                        break;
                    case nameof(size):
                        OnSizeChanged(size);
                        break;
                }
            }
        
    }
}
