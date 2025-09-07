using Fusion;
using Fusion.Addons.Physics;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    [Networked]
    public PlayerState playerState { get => default; set { } }
    public enum PlayerState
    {
        pendingConnect,
        connected,
        playing,
        dead
    }
    public static NetworkPlayer Local { get; set; }

    [Networked]
    public NetworkString<_16> nickName { get; set; }
    [Networked]
    public ushort size { get; set; }
    [Networked]
    public float speed { get; set; }
    [Networked]
    public Color spriteColor { get; set; }
    [Networked]
    public bool isBot { get => default; set { } }
    [Networked]
    public bool isMoving { get; set; }
    [SerializeField] Transform camTarget;
    [SerializeField] float lookSensitivity = 20f;
    [SerializeField] TextMeshProUGUI playerNickNameTM;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] SphereCollider _collider;
    [SerializeField] AudioSource moveAudioSource;
    [SerializeField] Canvas joysticks;
    [SerializeField] Canvas nickScoreCanvas;
    [SerializeField] AudioSource foodAudioSource;
    [SerializeField] AudioSource absorbAudioSource;
    [SerializeField] AudioSource deathAudioSource;
    [SerializeField] Animator _animator;
    [SerializeField] NetworkRigidbody3D _rigidbody;
    [SerializeField] ParticleSystem _particleSystem;

    public MeshRenderer _meshRenderer;
    public Vector2 inputDirection = Vector2.zero;
    public Vector2 inputLook = Vector2.zero;
    public Vector3 aiTarget = Vector3.zero;
    public bool hasTarget = false;

    InGameUIHandler inGameUIHandler;
    InterstitialAdSample adSample;
    NetworkInputManager inputManager;
    ChangeDetector _changeDetector;
    NetworkSpawner networkSpawner;
    [SerializeField] bool enableSpriteRenderer = true;
    TickTimer delayedEnabledSpriteRenderer = new TickTimer();
    private MaterialPropertyBlock _propBlock;
    private Material _instanceMaterial; // Her bot için ayrı materyal örneği

    void Awake()
    {
       
        networkSpawner = FindObjectOfType<NetworkSpawner>();
        if(_particleSystem==null)
            _particleSystem = GetComponentInChildren<ParticleSystem>();
        adSample = FindObjectOfType<InterstitialAdSample>();
        if (_meshRenderer==null)
            _meshRenderer = GetComponent<MeshRenderer>();
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
        _propBlock = new MaterialPropertyBlock();
        if (_meshRenderer != null)
        {
            // Her bot için ayrı materyal örneği oluştur
            _instanceMaterial = new Material(_meshRenderer.sharedMaterial);
            _meshRenderer.material = _instanceMaterial;
        }
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
        transform.name = $"P_{Object.Id}";

        if (Object.HasStateAuthority)
        {
            networkSpawner.UpdatePlayerNetworkArray();
            ResetPlayer();
        }
        Runner.SetPlayerObject(Object.InputAuthority, Object);
        Debug.Log("NetworkPlayer Spawned with InputAuthority: " + Object.InputAuthority + " " + Object.Id);
        if (HasInputAuthority && !isBot)
        {
            inputManager = Runner.GetComponent<NetworkInputManager>();
            inputManager.LocalPlayer = this;
            Local = this;
            CameraFollow.Singleton.SetTarget(camTarget);
            ChangeNickname(PlayerManager.Instance.nick);
            moveAudioSource.volume = PlayerManager.Instance.volume;
            ChangeColor(PlayerManager.Instance.skinColor);
        }

        if (Object.HasStateAuthority && isBot)
        {
            ChangeColor(Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.5f, 1f));
        }

        UpdateColor(spriteColor);

        if ((HasInputAuthority && !isBot)||HasStateAuthority )
            inGameUIHandler = FindObjectOfType<InGameUIHandler>();
        if (inGameUIHandler == null)
        {
            Debug.Log("InGameUIHandler not found!");
        }
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void FixedUpdateNetwork()
    {
        _meshRenderer.material.color = spriteColor;


        //Movement **check if player inside the game area**
        if (GetInput(out NetInput netInput))
        {
            inputDirection = netInput.Direction;
            inputLook = netInput.LookDelta;
        }
        if (Object.HasStateAuthority)
        {
            Vector3 scale = Vector3.one + Vector3.one * 100 * (size / 65535f);
            transform.localScale = scale;

            //bot movement
            if (isBot)
            { 
                //if ai target is null find a target
                if (hasTarget == false || aiTarget == Vector3.zero)
                {
                    FindTarget();
                }
                //if ai target is not null move towards it
                else
                {
                    MoveToTarget();
                }
            }
            //Player movement
            else
            {
                speed = (size / Mathf.Pow(size, 1.1f)) * 5;
                //Delta time??
                Vector3 rotation = new Vector3(-netInput.LookDelta.y, netInput.LookDelta.x, 0f) * lookSensitivity * Runner.DeltaTime;
                _rigidbody.Rigidbody.MoveRotation(_rigidbody.Rigidbody.rotation * Quaternion.Euler(rotation));
                UpdateCamTarget();
                Vector3 move = transform.TransformDirection(new Vector3(netInput.Direction.x, 0, netInput.Direction.y)) * speed * Runner.DeltaTime ;
                // Ensure player does not move faster than the speed limit diagonally **necessary?**
                if (move.magnitude > speed * Runner.DeltaTime)
                {
                    move = move.normalized * (speed * Runner.DeltaTime);
                }
                _rigidbody.Rigidbody.MovePosition(_rigidbody.Rigidbody.position + move);
            }
        }

        if (Object.HasStateAuthority)
        {
            // Hareket kontrolü
            bool currentlyMoving = inputDirection.magnitude > 0.1f;
            isMoving = currentlyMoving;
        }
        if (Object.HasInputAuthority && inGameUIHandler != null)
        {
            inGameUIHandler.SetConnectionType(Runner.CurrentConnectionType.ToString());
            inGameUIHandler.SetRtt($"RTT {Mathf.RoundToInt((float)Runner.GetPlayerRtt(Object.InputAuthority) * 100)} ms");
        }

        if (_changeDetector != null)
                CheckStateChanges();

        //if (delayedEnabledSpriteRenderer.Expired(Runner))
        //{
        //    playerSpriteRenderer.gameObject.SetActive(enableSpriteRenderer);
        //    if (Object.HasInputAuthority && enabled) { 
        //        Camera.main.transform.position = transform.position; 
        //}
        //    delayedEnabledSpriteRenderer = TickTimer.None;
        //}
        if(isBot)
            joysticks.gameObject.SetActive(false);
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
            _particleSystem.startSize = transform.localScale.x;
            if (isMoving & playerState == PlayerState.playing)
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

        UpdateColor(spriteColor); // Render'da rengi güncelle
        if (isBot)
            joysticks.gameObject.SetActive(false);
    
        if (playerState != PlayerState.playing)
        {
            _meshRenderer.enabled = false;
            nickScoreCanvas.gameObject.SetActive(false);
            _rigidbody.enabled = false;
            _collider.enabled = false;
            if (playerState == PlayerState.dead && !isBot)
            {
                //Debug.Log($"Player {nickName} is dead, showing join game canvas. isBot: {isBot}");
                //Debug.Log($"inGameUIHandler null? {inGameUIHandler == null}");
                //Debug.Log($"joinGameCanvas null? {inGameUIHandler?.joinGameCanvas == null}");
                //inGameUIHandler.joinGameCanvas.gameObject.SetActive(true);
            }
        }
        else
        {
            _meshRenderer.enabled = true;
            nickScoreCanvas.gameObject.SetActive(true);
            _rigidbody.enabled = true;
            _collider.enabled = true;
            //inGameUIHandler.joinGameCanvas.gameObject.SetActive(false);

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cell") && HasStateAuthority)
        {
            OnCollectFood(12);
            other.gameObject.GetComponent<NetworkTransform>().transform.position = Utils.GetRandomPosition();
        }

        if (other.CompareTag("Cell") && HasInputAuthority)
            foodAudioSource.Play();
        if (other.CompareTag("Cell"))
            _animator.SetTrigger("Food");

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
            }
#if UNITY_ANDROID_API || UNITY_IOS
            if (PlayerManager.Instance.vibration)
                Handheld.Vibrate();
#endif
        }

        if (other.GetComponent<NetworkPlayer>() && HasInputAuthority && other.GetComponent<NetworkPlayer>().size < size && !isBot)
            absorbAudioSource.Play();
        else if (other.GetComponent<NetworkPlayer>() && HasInputAuthority && other.GetComponent<NetworkPlayer>().size > size && !isBot)
        {
            deathAudioSource.Play();
            //adSample.ShowInterstitialAd();
            //inGameUIHandler.joinGameCanvas.gameObject.SetActive(true);
        }
        else if (other.GetComponent<NetworkPlayer>() && other.GetComponent<NetworkPlayer>().size < size)
            _animator.SetTrigger("Absorb");
    }
    private void UpdateCamTarget()
    {
        camTarget.transform.rotation = Quaternion.Lerp(camTarget.transform.rotation, transform.rotation * Quaternion.Euler(10, 0, 0), Time.deltaTime * 5f); // Smoothly transition camera rotation
    }

    void FindTarget()
    {
        var playerPositions = new List<(Vector3 pos, ushort size)>();
        Vector3 roundedPosition = new Vector3(
                   Mathf.Round(transform.position.x),
                   Mathf.Round(transform.position.y),
                   Mathf.Round(transform.position.z));
        foreach (var p in NetworkPlayerList.Instance.PlayerPositions)
        {
                playerPositions.Add((p.Key, p.Value));
        }
        Debug.Log("Find target called. player positions list count: " + playerPositions.Count);
        Debug.Log("NetworkPlayerList.Instance.PlayerPositions count: " + NetworkPlayerList.Instance.PlayerPositions.Count);
        var closest = playerPositions.OrderBy(p => Vector3.Distance(transform.position, p.pos)).ToList()[1];
       
        Debug.Log("Closest player found at " + closest.pos + " with size " + closest.size + "size was" + size + "pos: " + roundedPosition);

        if (closest.size<size){
            Debug.Log("Closest player is smaller, targeting it");
            aiTarget = closest.pos;
            hasTarget = true;
            return;
        }
        else 
        {
            FindFood();
            Debug.Log("No valid target found, looking for food");
        }
    }

    void MoveToTarget()
    {
        this.speed = (size / Mathf.Pow(size, 1.1f)) * 5;
        Vector3 vector3 = aiTarget - transform.position;
        float distance = vector3.magnitude;
        float _speed = this.speed * Mathf.Clamp01(distance / 5f); // Yaklaştıkça yavaşla
        _rigidbody.Rigidbody.MovePosition(_rigidbody.Rigidbody.position + vector3.normalized * _speed * Runner.DeltaTime);
        _particleSystem.Play();
        if (distance < 0.1f)
            hasTarget = false; // Reset target if reached

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




    

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority) { 
            Debug.Log("Despawning player: " + player);
            Runner.Despawn(Object);
        }

        }


    public void ChangeColor(Color newColor)
    {
        Debug.Log($"ChangeColor called with newColor: {newColor}");
        RPC_ChangeColor(newColor);
    }

    private void UpdateColor(Color color)
    {
        if (_meshRenderer != null && _instanceMaterial != null)
        {
            _instanceMaterial.color = color;
            _propBlock.SetColor("_Color", color);
            _meshRenderer.SetPropertyBlock(_propBlock);
        }
    }

    void OnColorChanged(Color color)
    {
        Debug.Log($"OnColorChanged: Player {nickName}, New Color: {color}");
        UpdateColor(color);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_ChangeColor(Color newColor, RpcInfo info = default)
    {
        if (!Object.HasStateAuthority) return;
        
        if (newColor == Color.black || newColor == Color.red)
        {
            spriteColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.5f, 1f);
        }
        else 
        {
            spriteColor = newColor;
        }
    }






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

    void OnSizeChanged(ushort s)
    {
        UpdateSize();
        speed = (size / Mathf.Pow(size, 1.1f)) * 5;
        scoreText.text = s.ToString();
    }


    public void OnPlayerDead()
    {
        Debug.Log($"OnPlayerDead called for player {nickName} with state {playerState}");
        _meshRenderer.enabled = false;
        playerState = PlayerState.dead;
        Debug.Log("Player state set to dead" + playerState);
        if (!isBot)
        {
            //adSample.ShowInterstitialAd();
            Debug.Log("Player is not bot, showing join game canvas");
            inGameUIHandler.OnPlayerDied();
        }
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

            if (playerState == PlayerState.dead && Object.HasInputAuthority &&!isBot)
            {
                //adSample.ShowInterstitialAd();
                Debug.Log("Not bot player is dead, showing join game canvas" + playerState);
                inGameUIHandler.OnPlayerDied();
            }
        }
    }

    public void Reset()
    {
        Debug.Log("Reset called"+nickName);
        _meshRenderer.enabled = true;
        //working?
        size = 1;
        speed = 5;
        transform.localScale = Vector3.one;
        playerState = PlayerState.playing;
        hasTarget = false;
    }
    void UpdateSize()
    {
        transform.localScale = Vector3.one + Vector3.one * 100 * (size / 65535f);
    }
   public void OnCollectFood(ushort growSize)
    {
#if UNITY_ANDROID_API || UNITY_IOS
            if (PlayerManager.Instance.vibration)
                Handheld.Vibrate();
#endif


        if (size >= 65535)
            return;

        size += growSize;
        speed = (size / Mathf.Pow(size, 1.1f)) * 5;
        UpdateSize();
        hasTarget = false;
    }

    public void ResetPlayer()
    {
            playerState = PlayerState.playing;
            Vector3 newPosition = Utils.GetRandomPosition();
            _rigidbody.Rigidbody.MovePosition(newPosition);
             _meshRenderer.enabled = true;
            size = 1;
            speed = 5;
            transform.localScale = Vector3.one;
            hasTarget = false;
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
                case nameof(spriteColor):
                        OnColorChanged(spriteColor);
                        break;
                    case nameof(size):
                        OnSizeChanged(size);
                        break;
                case nameof(playerState):
                    OnPlayerStateChanged();
                    break;  
                }
            }
        
    }
}
