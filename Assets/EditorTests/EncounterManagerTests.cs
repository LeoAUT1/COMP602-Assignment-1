using NUnit.Framework;
using UnityEngine;
using System.Reflection; // Added for BindingFlags clarity

namespace EditorTests
{
    public class EncounterManagerTests
    {
        private EncounterManager encounterManager;
        private EncounterData[] createdMockEncounters; // To keep track for cleanup

        [SetUp]
        public void Setup()
        {
            // Setup EncounterManager (assuming it's still a MonoBehaviour)
            var go = new GameObject("EncounterManagerGO");
            encounterManager = go.AddComponent<EncounterManager>();

            // Create mock encounters as ScriptableObjects
            createdMockEncounters = new EncounterData[3];
            for (int i = 0; i < createdMockEncounters.Length; i++)
            {
                // No GameObject needed for ScriptableObject instantiation
                var data = ScriptableObject.CreateInstance<EncounterData>();
                data.encounterName = $"Test {i}";
                // Optionally, ScriptableObjects also have a 'name' property you might want to set for debugging:
                // data.name = $"TestEncounterSO_{i}";
                createdMockEncounters[i] = data;
            }

            // Use reflection to assign the private field in EncounterManager
            FieldInfo encountersField = typeof(EncounterManager)
                .GetField("encounters", BindingFlags.NonPublic | BindingFlags.Instance);

            if (encountersField != null)
            {
                encountersField.SetValue(encounterManager, createdMockEncounters);
            }
            else
            {
                Assert.Fail("Could not find the private 'encounters' field in EncounterManager via reflection. Check the field name and binding flags.");
            }
        }

        [Test]
        public void GetRandomEncounters_ReturnsCorrectCount()
        {
            var result = encounterManager.GetRandomEncounters(5);

            Assert.IsNotNull(result, "Returned array should not be null");
            Assert.AreEqual(5, result.Length, "Should return exactly 5 encounters");
        }

        [Test]
        public void GetRandomEncounters_HandlesRequestLargerThanPool()
        {
            var result = encounterManager.GetRandomEncounters(10);

            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Length);
        }

        [Test]
        public void GetRandomEncounters_ShufflesContent()
        {
            // Ensure we have enough unique items to make shuffling meaningful
            // If createdMockEncounters.Length is less than 3, this test might be flaky.
            // For this test, we'll assume the pool size (3) is sufficient.
            if (createdMockEncounters.Length < 2)
            {
                Assert.Ignore("Skipping shuffle test: not enough mock encounters to reliably test shuffling.");
                return;
            }

            var result1 = encounterManager.GetRandomEncounters(createdMockEncounters.Length);
            var result2 = encounterManager.GetRandomEncounters(createdMockEncounters.Length);

            // This test is probabilistic. For a small set, it might occasionally pass even if not shuffled,
            // or fail if the "shuffle" coincidentally results in the same order for the first element.
            // A more robust test would compare the entire arrays or run multiple times.
            // However, for simplicity and common practice:
            bool areDifferent = false;
            for (int i = 0; i < result1.Length; i++)
            {
                if (result1[i] != result2[i])
                {
                    areDifferent = true;
                    break;
                }
            }
            // If the pool size is small (e.g., 3), and we request all 3,
            // the chance of getting the exact same order after shuffling is 1/3! = 1/6.
            // If the shuffle is truly random, this test will occasionally fail.
            // A better assertion might be to check that the sequence of names is different.
            Assert.IsTrue(areDifferent, "Shuffled encounters should likely differ between calls. If this fails sporadically, the shuffle might be weak or the pool too small.");
        }

        [TearDown]
        public void Cleanup()
        {
            // Destroy the GameObject for EncounterManager
            if (encounterManager != null && encounterManager.gameObject != null)
            {
                Object.DestroyImmediate(encounterManager.gameObject);
            }

            // Destroy the created ScriptableObject instances
            if (createdMockEncounters != null)
            {
                for (int i = 0; i < createdMockEncounters.Length; i++)
                {
                    if (createdMockEncounters[i] != null)
                    {
                        Object.DestroyImmediate(createdMockEncounters[i]);
                    }
                }
                createdMockEncounters = null; // Clear the reference
            }
        }
    }
}
