using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerController player;
    public CameraFollow cameraFollow;
    public static PlayerController Player;
    public static CameraFollow Camera;

    void Awake()
    {
        Player = player;
        Camera = cameraFollow;

        // Load persisted player data (level, kills) and reset runtime state (hp, flags)
        PlayerData.Load();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
