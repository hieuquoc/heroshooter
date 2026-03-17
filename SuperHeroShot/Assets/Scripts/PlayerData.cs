using UnityEngine;

/// <summary>
/// Static data store for the player.
/// - Level and Kills are persisted via PlayerPrefs.
/// - HP, Invincible, FreeWeapon are runtime-only (reset on Load).
/// Usage: PlayerData.Load() once at startup, then read/write properties freely.
/// </summary>
public static class PlayerData
{
    public static bool IsInvincible { get; set; } = false;
    public static bool IsFreeFire { get; set; } = false;

    // ── PlayerPrefs keys ─────────────────────────────────────────────────
    private const string KEY_LEVEL = "player_level";
    private const string KEY_KILLS = "player_kills";

    // ── Persisted properties ─────────────────────────────────────────────

    private static int _level;
    /// <summary>Player level. Clamped to ≥ 1. Saved instantly on set.</summary>
    public static int Level
    {
        get => _level;
        set
        {
            _level = Mathf.Max(1, value);
            PlayerPrefs.SetInt(KEY_LEVEL, _level);
            PlayerPrefs.Save();
        }
    }

    private static int _kills;
    /// <summary>Total enemy kills. Saved instantly on set.</summary>
    public static int Kills
    {
        get => _kills;
        set
        {
            _kills = Mathf.Max(0, value);
            PlayerPrefs.SetInt(KEY_KILLS, _kills);
            PlayerPrefs.Save();
        }
    }

    // ── Runtime-only properties (not persisted) ──────────────────────────

    private static int _maxHp = 100;
    private static int _hp;
    /// <summary>Current HP. Clamped between 0 and MaxHp.</summary>
    public static int HP
    {
        get => _hp;
        set => _hp = Mathf.Clamp(value, 0, _maxHp);
    }

    /// <summary>Maximum HP. Defaults to 100.</summary>
    public static int MaxHp
    {
        get => _maxHp;
        set
        {
            _maxHp = Mathf.Max(1, value);
            _hp    = Mathf.Clamp(_hp, 0, _maxHp);
        }
    }

    /// <summary>Whether the player takes no damage this session.</summary>
    public static bool Invincible { get; set; } = false;

    /// <summary>Whether all weapons are unlocked/free this session.</summary>
    public static bool FreeWeapon { get; set; } = false;

    // ── Is the player dead? ───────────────────────────────────────────────
    public static bool IsDead => _hp <= 0;

    // ── Load / Reset ─────────────────────────────────────────────────────

    /// <summary>
    /// Load persisted values from PlayerPrefs and reset runtime state.
    /// Call once at game startup (e.g. in GameManager.Awake).
    /// </summary>
    public static void Load()
    {
        _level = Mathf.Max(1, PlayerPrefs.GetInt(KEY_LEVEL, 1));
        _kills = Mathf.Max(0, PlayerPrefs.GetInt(KEY_KILLS, 0));

        // Reset runtime state
        _maxHp     = 100;
        _hp        = _maxHp;
        Invincible = false;
        FreeWeapon = false;
    }

    /// <summary>
    /// Erase all saved PlayerPrefs data and reload defaults.
    /// </summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(KEY_LEVEL);
        PlayerPrefs.DeleteKey(KEY_KILLS);
        PlayerPrefs.Save();
        Load();
    }

    // ── Convenience helpers ───────────────────────────────────────────────

    /// <summary>Add a kill and optionally level-up every <paramref name="killsPerLevel"/> kills.</summary>
    public static void AddKill(int killsPerLevel = 10)
    {
        Kills++;
        if (killsPerLevel > 0 && Kills % killsPerLevel == 0)
            Level++;
    }

    /// <summary>Apply damage, respecting Invincible flag. Returns true if player died.</summary>
    public static bool TakeDamage(int amount)
    {
        if (Invincible || amount <= 0) return false;
        HP -= amount;
        return IsDead;
    }

    /// <summary>Restore HP by <paramref name="amount"/>.</summary>
    public static void Heal(int amount)
    {
        if (amount > 0) HP += amount;
    }
}
