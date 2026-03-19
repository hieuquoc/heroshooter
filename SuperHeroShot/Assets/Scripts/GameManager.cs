using UnityEngine;

namespace rescueforce
{
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
        Instantiate(Resources.Load<GameObject>("City"), Vector3.zero, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        cameraFollow.gameObject.SetActive(true);
        player.gameObject.SetActive(true);
        UIManager.Instance.Close("HomePopup");
        UIManager.Instance.Open("InGameHUDPopup");
        _gameState = GameState.Playing;
        LevelManager.Instance.StartLevel(PlayerData.Level);
        player.SetAll(PlayerSpawnPoint);
    }

    public void BackToHome()
    {
        cameraFollow.gameObject.SetActive(false);
        player.gameObject.SetActive(false);
        UIManager.Instance.Open("HomePopup");
        _gameState = GameState.Home;
    }

    public void WinGame()
    {
        if(_gameState != GameState.Playing) return;
        _gameState = GameState.GameOver;
        PlayerData.Level += 1; // level up on win
        PlayerPrefs.SetInt("Level", PlayerData.Level);
        UIManager.Instance.Open("WinPopup");
        UIManager.Instance.Close("InGameHUDPopup");
        PlayerData.IsFreeFire = false;
        PlayerData.IsInvincible = false;
    }
}

public enum GameState
{
    Home,
    Playing,
    GameOver
}
}


