using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Player player;
    [SerializeField] private EncounterData currentEncounter;
    [SerializeField] private CombatManager combatManager;
    private Board board;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        // Subscribe when enabled
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        if (!IsFirstLoad && scene.name == "GameBoard")
        {
            if (player != null)
            {
                Debug.Log($"Player index is {player.GetTileIndex()}");
                //Player has loaded back into the board, get their 
                board = FindObjectOfType<Board>();
                board.SetPlayer(player);
                board.SetIsNewGame(false);
            }
        }


        if (scene.name == "Combat") //Maybe not use strings like this? im not sure
        {
            InitialiseCombat();
        }
    }

    void InitialiseCombat()
    {
        // Find the combat controller in the scene
        combatManager = FindObjectOfType<CombatManager>();

        if (combatManager == null)
        {
            Debug.LogError("No CombatManager found in scene!");
            return;
        }

        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogError("No Player found!");
                return;
            }
        }

        if (currentEncounter == null)
        {
            Debug.LogError("No EncounterData set before entering combat!");
            return;
        }

        combatManager.InitialiseCombat(player, currentEncounter);
    }


    public void ExitCombat()
    {
        //Do any housekeeping for leaving the combat scene
        if (player.GetPlayerCombat().GetIsAlive())
        {
            if (currentEncounter.isFinalBoss)
            {
                //Player has beat the final boss

                SceneLoader.Instance.LoadVictoryScene();
                return;
            }

            //Player has finished the encounter
            SceneLoader.Instance.LoadGameScene();
        }
        else //Game over
        {

        }
    }

    public EncounterData CurrentEncounter()
    {
        return currentEncounter;
    }

    public void SetCurrentEncounter(EncounterData data)
    {
        currentEncounter = data;
    }
}
