using UnityEngine;

public abstract class CombatEntity : MonoBehaviour
{
    [SerializeField] protected string entityName;
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected int strength;
    [SerializeField] protected int dexterity;
    [SerializeField] protected int intelligence;

    public virtual void AddHealth(float amount)
    {
        health += amount;
        health = Mathf.Min(health, maxHealth); // Prevent exceeding max health
    }

    public virtual void SubtractHealth(float amount)
    {
        health -= amount;
        health = Mathf.Max(health, 0f); // Prevent falling below 0
    }

    public string GetName() { return entityName; }
    public float GetHealth() { return health; }
    public float GetMaxHealth() { return maxHealth; }
    public int GetStrength() { return strength; }
    public int GetDexterity() { return dexterity; }
    public int GetIntelligence() { return intelligence; }

    // Other shared functionality
}
