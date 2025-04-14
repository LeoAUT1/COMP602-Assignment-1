using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Player player;

    [SerializeField] private EncounterData currentEncounter;
    [SerializeField] private CombatManager combatManager;

    protected override void Awake()
    {
        base.Awake();

        // Register for scene load notifications
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Always unregister events when the object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        if (scene.name == "Combat") //Maybe not use strings like this? im not sure
        {
            InitialiseCombat();
        }

        else if (scene.name == "GameBoard")
        {
            // The board scene has loaded
            ExitCombat();
        }
    }

    void InitialiseCombat()
    {
        // Find the combat controller in the scene
        if (combatManager == null)
        {
            combatManager = FindObjectOfType<CombatManager>();
        }

        combatManager.InitialiseCombatScene(player, currentEncounter);
    }

    void ExitCombat()
    {

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
