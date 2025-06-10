using UnityEngine;

public class Player : Singleton<Player>
{
    public string playerName = "Adventurer";

    //Leveling constants
    private float powerCurve = 1.1f; //The power term for how much exp should be required for each future level.
    private int baseExp = 20; // Base exp for each level

    //Internal stats
    [SerializeField] private int experience = 0;
    [SerializeField] private int playerLevel = 1;

    //Scene references
    private Board board;
    [SerializeField] PlayerCombat playerCombat;

    //Scene references for player location
    private BoardTile currentTile;
    [SerializeField] private int tileIndex;

    //Prefabs
    private PlayerBoardPiece playerPiece;
    [SerializeField] private GameObject levelUpCanvas;
    [SerializeField] private GameObject levelupParticleSystem;
    [SerializeField] private GameObject grantedPowerupParticleSystem;

    private bool powerupPending = false;
    private int experiencePending = 0;

    protected override void Awake() // Assuming Singleton<Player> has a virtual Awake
    {
        base.Awake(); // Call base Awake if it exists and does something important

        if (playerCombat != null)
        {
            playerCombat.OnStatsChanged += HandlePlayerStatsChanged;
        }
        else
        {
            Debug.LogError("PlayerCombat is not assigned in Player.cs. UI updates on stat changes will not work.");
        }
    }

    public void ResetPlayer()
    {
        Debug.Log("Resetting player");
        playerLevel = 1;
        experience = 0;
        currentTile = null;
        tileIndex = 0;

        if (playerCombat != null)
        {
            playerCombat.ResetPlayerCombat();
        }
    }

    private bool LevelUp(int exp)
    {
        float plev = playerLevel;

        if (exp >= baseExp * Mathf.Pow(plev, powerCurve))
        {
            playerLevel++;
            return true;
        }

        return false;
    }

    public int GetExpToLevel()
    {
        return (int)(baseExp * Mathf.Pow(playerLevel, powerCurve)) - experience;
    }

    public bool AddExperience(int amount)
    {
        experience += amount;

        bool hasLeveledUp = LevelUp(experience);
        //display some messsage for leveling up, or something
        if (hasLeveledUp) {

            AbilityBase newAbility = playerCombat.LearnAbility(playerLevel);

            GameObject levelupCanvas = Instantiate(levelUpCanvas);
            LevelUpInterface lvlUpInterface = levelupCanvas.GetComponent<LevelUpInterface>();

            Debug.Log(playerLevel);

            //Send the relevant data to the canvas
            lvlUpInterface.SetCanvas(playerLevel,newAbility);

            //learn ability if available on that level

            playerPiece.SetPlayerModel();
        }

        if (board != null)
        {
            board.UpdatePlayerStatsUi();
        }

        experiencePending = 0;

        return hasLeveledUp;
    }

    public void ExperiencePending(int amount)
    {
        experiencePending = amount;
    }

    public int GetExperiencePending()
    {
        return experiencePending;
    }

    private float GetTotalXpRequiredToReachLevel(int level)
    {
        if (level <= 1)
        {
            return 0f;
        }
        return baseExp * Mathf.Pow(level - 1, powerCurve);
    }

    //There is a good argument for moving this logic elsewhere... but eh
    public float GetNormalizedExperienceProgress()
    {
        float xpAtStartOfCurrentLevel = GetTotalXpRequiredToReachLevel(playerLevel);
        float xpAtStartOfNextLevel = GetTotalXpRequiredToReachLevel(playerLevel + 1);

        float totalXpForThisLevelBand = xpAtStartOfNextLevel - xpAtStartOfCurrentLevel;

        if (totalXpForThisLevelBand <= 0)
        {

            if (GetExperience() >= xpAtStartOfCurrentLevel)
            {
                return 1.0f;
            }
            return 0.0f;
        }

        float xpGainedInCurrentLevelBand = GetExperience() - xpAtStartOfCurrentLevel;

        float normalizedProgress = xpGainedInCurrentLevelBand / totalXpForThisLevelBand;

        return Mathf.Clamp01(normalizedProgress);
    }

    public int GetExperience()
    {
        return experience;
    }

    public int GetLevel()
    {
        return playerLevel;
    }

    public BoardTile GetCurrentBoardTile()
    {
        return currentTile;
    }

    public int GetTileIndex()
    {
        return tileIndex;
    }

    public void SetCurrentBoardTile(BoardTile tile)
    {
        currentTile = tile;
        this.tileIndex = tile.GetIndex();
    }

    public PlayerCombat GetPlayerCombat()
    {
        return playerCombat;
    }

    public void SetBoard(Board b)
    {
        board = b;
    }

    public void AddPowerup(PowerupData powerup)
    {
        playerCombat.AddPowerup(powerup.name, powerup);
        if (grantedPowerupParticleSystem != null)
        {
            Debug.Log("Instantiating particles");
            GameObject go = Instantiate(grantedPowerupParticleSystem, playerPiece.transform);

            go.transform.localPosition = new Vector3(0f, 1f, 0f);
        }
        else
        {
            Debug.Log("Missing particles prefab");
        }

        //This is not great
        if (board != null)
        {
            PowerUpUIManager manager = board.GetComponentInChildren<PowerUpUIManager>();
            manager.RedrawPowerups();
        }
    }

    public void SetPlayerPiece(GameObject piece) // Set the player's model, we need this so that we can update the model when the player levels up
    {
        Debug.Log("Setting player piece");
        playerPiece = piece.GetComponent<PlayerBoardPiece>();
    }

    public void SetRandomPowerupPending()
    {
        powerupPending = true;
    }

    public bool GetRandomPowerupPending()
    {
        return powerupPending;
    }

    public void GrantRandomPowerup()
    {
        // This is stupid and hacky
        PowerupData powerup = GetComponent<PowerupDistribution>().GrantRandomPowerup();
        Debug.Log(powerup.powerupName);
        AddPowerup(powerup);
        powerupPending = false;
    }

    // Unsubscribe when the Player object is destroyed
    void OnDestroy()
    {
        if (playerCombat != null)
        {
            playerCombat.OnStatsChanged -= HandlePlayerStatsChanged;
        }
    }

    // This method will be called when PlayerCombat raises OnStatsChanged
    private void HandlePlayerStatsChanged()
    {
        if (board != null)
        {
            Debug.Log("Player stats changed, updating UI via callback.");
            board.UpdatePlayerStatsUi();
        }
        else
        {
            Debug.LogWarning("Player stats changed, but board is null. UI not updated.");
        }

        if (levelupParticleSystem != null && playerPiece != null)
        {
            GameObject go = Instantiate(levelupParticleSystem, playerPiece.transform);
            //offset upwards
            go.transform.localPosition = new Vector3(0f, 1f, 0f);

        }
    }
}
