using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PowerupDataTests
{

    [Test]
    public void PowerupData_Initialization_SetsPropertiesCorrectly()
    {
        // arrange
        PowerupData powerup = ScriptableObject.CreateInstance<PowerupData>();
        string expectedName = "Speed";
        string expectedDescription = "Increases player speed.";

        // Act
        powerup.powerupName = expectedName;
        powerup.description = expectedDescription;
        powerup.count = 5;

        // Assert
        Assert.AreEqual(expectedName, powerup.powerupName);
        Assert.AreEqual(expectedDescription, powerup.description);
        Assert.AreEqual(5, powerup.count);
    }


    [Test]
    public void Increment_IncreasesCountByOne()
    {
        // Arrange
        PowerupData powerup = ScriptableObject.CreateInstance<PowerupData>();
        powerup.count = 1;
        int expectedCount = 2;

        // Act
        powerup.Increment();

        // Assert
        Assert.AreEqual(expectedCount, powerup.count);
    }
}
