using UnityEngine;
using UnityEngine.Subsystems;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Combat/Status Effect")]
public class StatusEffect : ScriptableObject
{
    [SerializeField] private string effectName;
    [SerializeField][TextArea(2, 4)] private string description;
    [SerializeField] private int durationTurns = 1; // -1 for permanent
    [SerializeField] private bool isHarmful = false; // For UI or AI logic

    // Properties to maintain compatibility with existing code
    public string EffectName => effectName;
    public string Description => description;
    public int DurationTurns { get; protected set; } // Runtime duration
    public bool IsHarmful => isHarmful;

    // Runtime references - not serialized
    protected CombatEntity Target;
    protected CombatManager CombatManagerRef;
    protected CombatHud CombatHudRef;

    // Initialize runtime values from serialized data
    public virtual void Initialize()
    {
        // Set runtime duration from the serialized value
        DurationTurns = durationTurns;
    }

    // Called when the effect is first applied to the target
    public virtual void OnApply(CombatEntity target, CombatManager combatManager, CombatHud hud)
    {
        Initialize(); // Set runtime values
        this.Target = target;
        this.CombatManagerRef = combatManager;
        this.CombatHudRef = hud;
    }

    // Called at the start of the target's turn
    public virtual void OnTurnStart(CombatHud hud) { }

    // Called at the end of the target's turn
    public virtual void OnTurnEnd(CombatHud hud) { }

    // Called when the target takes damage
    public virtual void OnDamageTaken(CombatEntity source, ref int damageAmount, CombatHud hud) { }

    // Called when the target deals damage
    public virtual void OnDamageDealt(CombatEntity victim, ref int damageAmount, CombatHud hud) { }

    // Called to decrement duration and check for expiration
    // Returns true if the effect expired and should be removed
    public virtual bool TickDown()
    {
        if (DurationTurns > 0)
        {
            DurationTurns--;
            if (DurationTurns == 0)
            {
                return true; // Expired
            }
        }
        return false; // Not expired or indefinite
    }

    // Called when the effect is removed (either by duration or dispel)
    public virtual void OnRemove()
    {
        // Cleanup logic, if any
    }
    public void SetEffectName(string name) => effectName = name;
    public void SetDescription(string desc) => description = desc;
    public void SetDuration(int duration) => durationTurns = duration;
    public void SetIsHarmful(bool harmful) => isHarmful = harmful;
}

//Runtime instantiation example
// Create a new instance of the RegenPassive ScriptableObject
//RegenPassive regenEffect = ScriptableObject.CreateInstance<RegenPassive>();

// Set properties manually
// Access private serialized fields using reflection or add setter methods
//regenEffect.SetEffectName("Regeneration");
//regenEffect.SetDescription("Heals 5 HP at the start of the turn.");
//regenEffect.SetDuration(3);
//regenEffect.SetHealAmount(5);

// Apply to target
//combatEntity.AddStatusEffect(regenEffect);
