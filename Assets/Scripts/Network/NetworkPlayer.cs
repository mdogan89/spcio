using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using UnityEngine;
public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float speed;

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
    [Networked]
    public bool isBot { get => default; set { } }
    GameObject aiTarget = null;
    NetworkSpawner networkSpawner;
    bool enableSpriteRenderer = true;
    TickTimer delayedEnabledSpriteRenderer = new TickTimer();

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
    void ResetPlayer()
    {
        Vector3 newPosition = Utils.GetRandomPosition();

        kcc.SetPosition(newPosition);

        kcc.enabled = true;

        playerState = PlayerState.playing;

        size = 1;
        speed = 5;
        transform.localScale = Vector3.one;
    }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 0f);
        kcc.SetShape(EKCCShape.None, 0, 0);
        Runner.SetPlayerObject(Object.InputAuthority, Object);
        if (HasInputAuthority)
        {
            inputManager = Runner.GetComponent<NetworkInputManager>();
            inputManager.LocalPlayer = this;
            Local = this;
            CameraFollow.Singleton.SetTarget(camTarget);
            kcc.Settings.ForcePredictedLookRotation = true;
        }
        if (Object.HasStateAuthority)
            spriteColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.5f, 1f);
        playerSpriteRenderer.material.color = spriteColor;
        ////ResetCells();

        transform.name = $"P_{Object.Id}";
        // ChangeDetector'ý doðru þekilde baþlat

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
        
        if (playerState != PlayerState.playing)
        {
            kcc.SetPosition(transform.position);
            // kcc.enabled = false;

 
        }

        if (delayedEnabledSpriteRenderer.Expired(Runner))
        {
            playerSpriteRenderer.gameObject.SetActive(enableSpriteRenderer);

            if (Object.HasInputAuthority && enabled)
                Camera.main.transform.position = transform.position;
            
            delayedEnabledSpriteRenderer = TickTimer.None;
        }

        //Movement **check if player inside the game area**
        if (GetInput(out NetInput input))
        {
            inputDirection = input.Direction;
        }
        if (Object.HasStateAuthority)
        {
            //bot movement
            if (isBot)
            {
                if (aiTarget == null)
                {
                    GameObject[] foodTargets = GameObject.FindGameObjectsWithTag("Cell");
                    aiTarget = foodTargets[Random.Range(0, foodTargets.Length)];
                }
                else
                {
                    Vector3 vector3 = aiTarget.transform.position - transform.position;
                    kcc.Move(vector3.normalized * speed);
                    baseLookRotation = kcc.GetLookRotation();
                }
            }
            //Player movement
            else
            {
                kcc.AddLookRotation(input.LookDelta * lookSensitivity);
                UpdateCamTarget();
                Vector3 worldDirection = kcc.LookRotation * new Vector3(input.Direction.x, 0, input.Direction.y);

                kcc.Move(worldDirection.normalized * speed);
                baseLookRotation = kcc.GetLookRotation();

            }
        }

        if (Object.HasInputAuthority && inGameUIHandler != null)
        {
            inGameUIHandler.SetConnectionType(Runner.CurrentConnectionType.ToString());
            inGameUIHandler.SetRtt($"RTT {Mathf.RoundToInt((float)Runner.GetPlayerRtt(Object.InputAuthority) * 100)} ms");
        }
        // Change detection kontrolü için yeni yaklaþým

        if (_changeDetector != null)
            {

                CheckStateChanges();

            }
        
    }

    public override void Render()
    {
        if (kcc.Settings.ForcePredictedLookRotation)
        {
            Vector2 predictedLookRotation = baseLookRotation + inputManager.AccumulatedMouseDelta * lookSensitivity;
            kcc.SetLookRotation(predictedLookRotation);
        }
        UpdateCamTarget();
        if (playerNickNameTM != null && scoreText != null)
        {
            playerNickNameTM.text = nickName.ToString();
            scoreText.text = size.ToString();
        }
        playerSpriteRenderer.transform.localScale = Vector3.one + Vector3.one * 100 * (size / 65535f);
    }


    private void UpdateCamTarget()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cell") && HasStateAuthority)
        {
            OnCollectFood(12);
            other.gameObject.GetComponent<NetworkTransform>().transform.position = Utils.GetRandomPosition();
        }

        if (other.GetComponentInParent<NetworkPlayer>() && HasStateAuthority)
        {
            NetworkPlayer player = other.GetComponentInParent<NetworkPlayer>();
            if (player.size == size)
                return;
            if (player.size > size)
            {
                Debug.Log(nickName + "collided with bigger player" + player.nickName);
                float foodFromOtherPlayer = player.size * 0.1f;

                if (foodFromOtherPlayer < 20)
                {
                    foodFromOtherPlayer = 20;
                }
                OnCollectFood((ushort)foodFromOtherPlayer);

                player.OnPlayerDead();
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
    }
    void UpdateSize()
    {
        //meshrenderer
        playerSpriteRenderer.transform.localScale = Vector3.one + Vector3.one * 100 * (size / 65535f);
    }
    void OnCollectFood(ushort growSize)
    {
        size += growSize;
        //speed -= speed / 100;
        speed = (size / Mathf.Pow(size, 1.1f)) * 50;
        UpdateSize();
        //scoreText.text = size.ToString();
    }
    //public void ResetCells()
    //{
    //    eatenCells = 0;
    //}

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
