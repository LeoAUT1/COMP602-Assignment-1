using UnityEngine;

public abstract class CombatEntity : MonoBehaviour
{
    [SerializeField] protected string entityName;
    [SerializeField] protected int health;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int strength;
    [SerializeField] protected int dexterity;
    [SerializeField] protected int intelligence;

    private bool isAlive = true;

    public virtual int SetHealth(int amount)
    {
        health = amount;
        return health;
    }

    public virtual int AddHealth(int amount)
    {
        health += amount;
        health = Mathf.Min(health, maxHealth); // Prevent exceeding max health

        return health;
    }

    public virtual int SubtractHealth(int amount)
    {
        health -= amount;
        health = Mathf.Max(health, 0); // Prevent falling below 0

        if (health == 0)
        {
            isAlive = false;
        }

        return health;
    }

    public string GetName() { return entityName; }
    public int GetHealth() { return health; }
    public int GetMaxHealth() { return maxHealth; }
    public int GetStrength() { return strength; }
    public int GetDexterity() { return dexterity; }
    public int GetIntelligence() { return intelligence; }
    public bool GetIsAlive() {  return isAlive; }   

    public void SetStrength(int x) { strength = x; }
    public void SetDexterity(int x) { dexterity = x; }
    public void SetIntelligence (int x) { intelligence = x; }

    // Other shared functionality
}
