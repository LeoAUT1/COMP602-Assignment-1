using NUnit.Framework;
using UnityEngine;

public class StatusEffectTests
{
    private StatusEffect statusEffectInstance;

    [SetUp]
    public void SetUp()
    {
        // Create a fresh instance for each test
        statusEffectInstance = ScriptableObject.CreateInstance<StatusEffect>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the ScriptableObject instance after each test
        Object.DestroyImmediate(statusEffectInstance);
    }

    [Test]
    public void Initialization_SetsRuntimeDurationFromSerializedValue()
    {
        statusEffectInstance.SetDuration(5); // This sets the private 'durationTurns'
        statusEffectInstance.Initialize();   // This should copy 'durationTurns' to the 'DurationTurns' property
        Assert.AreEqual(5, statusEffectInstance.DurationTurns, "Runtime DurationTurns should be initialized from the serialized value after Initialize().");
    }

    [Test]
    public void OnApply_CallsInitializeAndSetsRuntimeDuration()
    {
        statusEffectInstance.SetDuration(3);
        statusEffectInstance.OnApply(null, null, null);

        Assert.AreEqual(3, statusEffectInstance.DurationTurns, "OnApply should trigger initialization, setting the runtime DurationTurns.");
    }

    [Test]
    public void TickDown_DecrementsFiniteDurationAndReturnsFalseIfNotExpired()
    {
        statusEffectInstance.SetDuration(3);
        statusEffectInstance.Initialize(); // Runtime DurationTurns = 3

        bool expired = statusEffectInstance.TickDown(); // Runtime DurationTurns should become 2

        Assert.IsFalse(expired, "TickDown should return false as the effect is not yet expired.");
        Assert.AreEqual(2, statusEffectInstance.DurationTurns, "DurationTurns should be decremented by TickDown.");
    }

    [Test]
    public void TickDown_ReturnsTrueWhenDurationReachesZero()
    {
        statusEffectInstance.SetDuration(1);
        statusEffectInstance.Initialize(); // Runtime DurationTurns = 1

        bool expired = statusEffectInstance.TickDown(); // Runtime DurationTurns should become 0

        Assert.IsTrue(expired, "TickDown should return true when effect duration reaches zero.");
        Assert.AreEqual(0, statusEffectInstance.DurationTurns, "DurationTurns should be zero upon expiration.");
    }

    [Test]
    public void TickDown_PermanentEffectDoesNotDecrementOrExpire()
    {
        statusEffectInstance.SetDuration(-1); // -1 signifies a permanent effect
        statusEffectInstance.Initialize();    // Runtime DurationTurns = -1

        bool expired = statusEffectInstance.TickDown();

        Assert.IsFalse(expired, "TickDown should return false for permanent effects.");
        Assert.AreEqual(-1, statusEffectInstance.DurationTurns, "DurationTurns for permanent effects should remain unchanged after TickDown.");

        // Let's be thorough and tick it again
        expired = statusEffectInstance.TickDown();
        Assert.IsFalse(expired, "TickDown should consistently return false for permanent effects on subsequent ticks.");
        Assert.AreEqual(-1, statusEffectInstance.DurationTurns, "DurationTurns for permanent effects should still be unchanged.");
    }

    [Test]
    public void TickDown_WithInitialZeroDurationReturnsFalse()
    {
        statusEffectInstance.SetDuration(0);
        statusEffectInstance.Initialize(); // Runtime DurationTurns = 0

        // According to the logic (if DurationTurns > 0), this should not decrement or expire.
        // Might be a good idea to change this...?
        bool expired = statusEffectInstance.TickDown();

        Assert.IsFalse(expired, "TickDown should return false if duration is already zero, as it doesn't meet the 'DurationTurns > 0' condition.");
        Assert.AreEqual(0, statusEffectInstance.DurationTurns, "DurationTurns should remain zero.");
    }

    [Test]
    public void Setters_CorrectlyModifyBackingFieldsAndPropertiesReflectThem()
    {
        statusEffectInstance.SetEffectName("Inferno");
        Assert.AreEqual("Inferno", statusEffectInstance.EffectName, "EffectName property did not update after SetEffectName.");

        statusEffectInstance.SetDescription("A raging fire.");
        Assert.AreEqual("A raging fire.", statusEffectInstance.Description, "Description property did not update after SetDescription.");

        statusEffectInstance.SetIsHarmful(true);
        Assert.IsTrue(statusEffectInstance.IsHarmful, "IsHarmful property did not update after SetIsHarmful.");

        statusEffectInstance.SetDuration(7);
        statusEffectInstance.Initialize();
        Assert.AreEqual(7, statusEffectInstance.DurationTurns, "Runtime DurationTurns property did not update correctly after SetDuration and Initialize.");
    }

    [Test]
    public void Properties_ReflectDefaultValuesUponCreation()
    {

        Assert.IsNull(statusEffectInstance.EffectName, "Default EffectName should be null.");
        Assert.IsNull(statusEffectInstance.Description, "Default Description should be null.");
        Assert.IsFalse(statusEffectInstance.IsHarmful, "Default IsHarmful should be false (as per bool default).");

        statusEffectInstance.Initialize();
        Assert.AreEqual(1, statusEffectInstance.DurationTurns, "Default runtime DurationTurns after Initialize should be 1.");
    }

    [Test]
    public void OnTurnStart_BaseImplementation_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => statusEffectInstance.OnTurnStart(null),
            "Base OnTurnStart should not throw an exception.");
    }
}
