using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyScalingTests
{
    private GameObject playerObject;
    private GameObject enemyObject;

    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("Player");
        playerObject.AddComponent<Player>();

        // Setup player level manually
        Player.Instance.ResetPlayer(); // Ensure clean state
        typeof(Player).GetField("playerLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(Player.Instance, 5); // Forced to level 5

        enemyObject = new GameObject("Enemy");
        enemyObject.AddComponent<TestEnemy>();
    }

    [TearDown]
    public void Teardown()
    {
        GameObject.DestroyImmediate(playerObject);
        GameObject.DestroyImmediate(enemyObject);
    }

    [Test]
    public void Enemy_Level_Scale()
    {
        var enemy = enemyObject.GetComponent<TestEnemy>();

		// Scaling to player function
		enemy.AutoScaleToPlayer();

        Assert.AreEqual(Player.Instance.GetLevel(), enemy.GetLevel(), "Enemy level does not match player's current level.");
    }

    [Test]
    public void Enemy_Stats_Scale()
    {
        var enemy = enemyObject.GetComponent<TestEnemy>();

        int baseHealth = enemy.GetHealth();
        int baseStr = enemy.GetStrength();
        int baseDex = enemy.GetDexterity();
        int baseInt = enemy.GetIntelligence();

		// Scaling to player function
		enemy.AutoScaleToPlayer();

        // Scale for player level 5: (5 - 1) * 10 = 40
        int expectedIncrease = 40;

        Assert.AreEqual(baseHealth + expectedIncrease, enemy.GetHealth(), "Health did not scale correctly.");
        Assert.AreEqual(baseStr + expectedIncrease, enemy.GetStrength(), "Strength did not scale correctly.");
        Assert.AreEqual(baseDex + expectedIncrease, enemy.GetDexterity(), "Dexterity did not scale correctly.");
        Assert.AreEqual(baseInt + expectedIncrease, enemy.GetIntelligence(), "Intelligence did not scale correctly.");
    }
}

// Enemy stat setting
public class TestEnemy : CombatEntity
{
    private void Awake()
    {
        // Base stats
        SetHealth(100);
        SetMaxHealth(100);
        SetStrength(10);
        SetDexterity(10);
        SetIntelligence(10);
    }
}
