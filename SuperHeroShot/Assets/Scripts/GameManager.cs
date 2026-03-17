using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerController player;
    public CameraFollow cameraFollow;
    public static PlayerController Player;
    public static CameraFollow Camera;
    public GameState GameState => _gameState;
    public static GameManager Instance { get; private set; }
    [SerializeField] private Transform PlayerSpawnPoint;
    [SerializeField] private GameState _gameState = GameState.Home;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Player = player;
        Camera = cameraFollow;

        // Load persisted player data (level, kills) and reset runtime state (hp, flags)
        PlayerData.Load();
        player.gameObject.SetActive(false);
        cameraFollow.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        // Example: reset player HP and flags at the start of each game
        PlayerData.HP = PlayerData.MaxHp;
        cameraFollow.gameObject.SetActive(true);
        player.gameObject.SetActive(true);
        player.transform.position = PlayerSpawnPoint.position;
        player.transform.rotation = PlayerSpawnPoint.rotation;
        UIManager.Instance.Close("HomePopup");
        UIManager.Instance.Open("InGameHUDPopup");
        _gameState = GameState.Playing;
    }
}

public enum GameState
{
    Home,
    Playing,
    GameOver
}
