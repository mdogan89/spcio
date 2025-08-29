using Fusion;
using Fusion.Addons.Physics;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.Unicode;
public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
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
    public NetworkString<_16> nickName { get ; set; }

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

    [SerializeField] ParticleSystem _particleSystem;
    [Networked]
    public bool isMoving { get; set; }

    [SerializeField] AudioSource moveAudioSource;
    [SerializeField] MeshRenderer _meshRenderer;
    [SerializeField] Canvas joysticks;
    [SerializeField] Canvas nickScoreCanvas;


    void Awake()
    {
        inGameUIHandler = FindObjectOfType<InGameUIHandler>();
        networkSpawner = FindObjectOfType<NetworkSpawner>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        if(_meshRenderer==null)
            _meshRenderer = GetComponent<MeshRenderer>();
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
        base.Spawned();
        if (Object.HasStateAuthority)
        {
            Debug.Log($"Player {nickName} spawned, updating network array");
            var spawner = FindObjectOfType<NetworkSpawner>();
            spawner?.UpdatePlayerNetworkArray();
        }
        //kcc.SetGravity(Physics.gravity.y * 0f);
        //kcc.SetShape(EKCCShape.None, 0, 0);
        Runner.SetPlayerObject(Object.InputAuthority, Object);
        Debug.Log("NetworkPlayer Spawned: " + Object.InputAuthority + " " + Object.Id);
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


        //if (isBot && HasStateAuthority)
        //{
        //    nickName = Utils.GetRandomName();
        ////    playerNickNameTM.text = nickName.ToString();
        //}
        //else if (HasStateAuthority)
        //{
        //    Debug.Log("NetworkPlayer Spawned with input authority" + PlayerManager.Instance.nick) ;
        //    nickName = PlayerManager.Instance.nick;
        //}

        if (HasInputAuthority && !isBot)
        {
            ChangeNickname(PlayerManager.Instance.nick);
            moveAudioSource.volume = PlayerManager.Instance.volume;
        }



    }

    public override void FixedUpdateNetwork()
    {


        if (HasStateAuthority) {
            Vector3 scale = Vector3.one + Vector3.one * 100 * (size / 65535f);
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
                    FindFood();
                }
                //if ai target is not null move towards it
                else
                {
                    speed = (size / Mathf.Pow(size, 1.1f)) * 5;
                    //transform.localScale = Vector3.one + Vector3.one * 1000 * (size / 65535);
                    Vector3 vector3 = aiTarget - transform.position;
                    float distance = vector3.magnitude;
                   // float _speed = speed * Mathf.Clamp01(distance / 5f); // Yaklaştıkça yavaşla
                    _rigidbody.Rigidbody.MovePosition(_rigidbody.Rigidbody.position + vector3.normalized * speed * Runner.DeltaTime);
                    // _rigidbody.Rigidbody.MovePosition(_rigidbody.Rigidbody.position + vector3.normalized * speed * Runner.DeltaTime);
                    //baseLookRotation = kcc.GetLookRotation();
                    _particleSystem.Play();
                    if (distance<0.1f)
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


        if (Object.HasStateAuthority)
        {
            // Hareket kontrolü
            bool currentlyMoving = inputDirection.magnitude > 0.1f;
            isMoving = currentlyMoving;
        }
    }
    void FindFood()
    {

        var foodList = GetFoodList();
        if (foodList.Count > 0)
        {
            aiTarget = foodList[0];
            hasTarget = true;
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
        UpdateCamTarget();
        if (playerNickNameTM != null && scoreText != null)
        {
            playerNickNameTM.text = nickName.ToString();
            scoreText.text = size.ToString();
        }
        if (_particleSystem != null)
        {
            if (isMoving)
            {
                if (!_particleSystem.isPlaying)
                    _particleSystem.Play();
                if (moveAudioSource != null && !moveAudioSource.isPlaying && !isBot)
                    moveAudioSource.Play();
            }
            else
            {
                if (_particleSystem.isPlaying)
                    _particleSystem.Stop();
                if (moveAudioSource != null && moveAudioSource.isPlaying && !isBot)
                    moveAudioSource.Stop();
            }
        }
        _meshRenderer.enabled = enableSpriteRenderer;
        nickScoreCanvas.gameObject.SetActive(enableSpriteRenderer);
        joysticks.gameObject.SetActive(enableSpriteRenderer && !isBot && Object.HasInputAuthority);
    }


    private void UpdateCamTarget()
    {
        camTarget.transform.rotation = Quaternion.Lerp(camTarget.transform.rotation, transform.rotation * Quaternion.Euler(15, 0, 0), Time.deltaTime * 5f); // Smoothly transition camera rotation
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
        if (player == Object.InputAuthority) { 
            Debug.Log("Despawning player: " + player);
            Runner.Despawn(Object);
        }

        }

    

    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    //public void RPC_OnMenuButtonClicked()
    //{
    //    if (Object == null)
    //    {
    //        Debug.Log("RPC_OnMenuButtonClicked: Object is null");
    //        return;
    //    }
    //    Debug.Log("RPC_OnMenuButtonClicked called" + Object.name);

    //    if (Runner != null && Runner.IsServer)
    //    {
    //        Debug.Log("RPC_OnMenuButtonClicked: Despawning player " + Object.name);
    //        Runner.Despawn(Object);
    //        Debug.Log("Player despawned: " + Object.name == null);
    //    }
    //    if (Runner.IsClient)
    //    {
    //        Debug.Log("RPC_OnMenuButtonClicked: Client is trying to leave the game");
    //        Runner.Shutdown();
    //    }
    //    SceneManager.LoadScene(0, LoadSceneMode.Single);
    //    var manager = GameObject.Find("PlayerManager");
    //    if (manager != null)
    //        Destroy(manager); // Destroy the PlayerManager instance if it exists
    //}

    //public void OnMenuButtonClicked()
    //{
    //    Debug.Log("OnMenuButtonClicked");

    //    // (NetworkPlayer localPlayer) = Runner.GetPlayer(Object.InputAuthority);

    //    if (Object.HasInputAuthority)
    //    {
    //        Debug.Log("OnMenuButtonClicked: Local player is despawning");
    //        NetworkRunner _runner = FindAnyObjectByType<NetworkRunner>();
    //        if (_runner == null)
    //        {
    //            Debug.LogError("OnMenuButtonClicked: NetworkRunner is null");
    //            return;
    //        }

    //        else if (_runner.IsClient)
    //        {
    //            Debug.Log("OnMenuButtonClicked: Client is trying to leave the game");
    //            Runner.Shutdown();
    //        }
    //        else
    //        {
    //            Debug.Log("OnMenuButtonClicked: Not local player, cannot despawn");

    //        }
    //    }











            //if (HasStateAuthority)
            //{
            //    Debug.Log("OnMenuButtonClicked: Server is despawning player " + NetworkPlayer.Local.Object.Name);
            //    NetworkRunner _runner = FindAnyObjectByType<NetworkRunner>();
            //    if (_runner == null)
            //    {
            //        Debug.LogError("OnMenuButtonClicked: NetworkRunner is null");
            //        return;
            //    }
            //    else if (_runner.IsServer) {
            //        Debug.Log("OnMenuButtonClicked: ServerRunner is despawning player " + NetworkPlayer.Local.Object.Name);
            //        _runner.Despawn(NetworkPlayer.Local.Object);
            //    }

            //    else if (_runner.IsClient)
            //    {
            //        Debug.Log("OnMenuButtonClicked: Client is trying to leave the game");
            //        Runner.Shutdown();
            //    }


            //public void OnMenuButtonClicked(PlayerRef player)
            //{
            //    Debug.Log($"OnMenuButtonClicked called for player: {player}");
            //    var manager = GameObject.Find("PlayerManager");
            //    if (manager != null)
            //        Destroy(manager); // Destroy the PlayerManager instance if it exists
            //    SceneManager.LoadScene(0, LoadSceneMode.Single);
            //    PlayerLeft(player);
            //}


        //}
    public void ChangeNickname(string newNick)
    {
        if (string.IsNullOrWhiteSpace(newNick))
        {
            Debug.Log("ChangeNickname: New nickname is null or whitespace");
            return;
        }
        if (newNick.Length > 16)
        {
            newNick = newNick.Substring(0, 16); // Trim to max length
        }
        Debug.Log($"ChangeNickname called with newNick: {newNick}");
        RPC_ChangeNickname(newNick);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_ChangeNickname(string newNick, RpcInfo info = default)
    {
        if (string.IsNullOrWhiteSpace(newNick))
        {
            Debug.Log("RPC_ChangeNickname: New nickname is null or whitespace");
            return;
        }
        if (newNick.Length > 16)
        {
            newNick = newNick.Substring(0, 16); // Trim to max length
        }
        Debug.Log($"[RPC] RPC_ChangeNickname {newNick}");
        this.nickName = newNick;
    }




    public void JoinGame(string nickname)
    {
            Debug.Log($"JoinGame called with nickname: {nickname}");
            RPC_JoinGame(nickname);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_JoinGame(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] RPC_JoinGame {nickName}");
        this.nickName = nickName;

        ResetPlayer();
    }
    public void BotJoinGame()
    {
        Debug.Log("Bot joined game");
        ResetPlayer();
    }


    void OnNickNameChanged(NetworkString<_16> nick)
    {

        Debug.Log($"Nickname changed for player to {nick} for player {gameObject.name}");
        Debug.Log($"OnNickNameChanged: {gameObject.name}, playerNickNameTM null? {playerNickNameTM == null}");
        playerNickNameTM.text = nick.ToString();

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
        transform.localScale = Vector3.one + Vector3.one * 100 * (size / 65535f);
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
            _meshRenderer.enabled = true;
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
                    OnNickNameChanged(nickName);
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
