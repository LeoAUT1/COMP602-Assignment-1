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

            // create mock encounter
            EncounterData[] mockEncounters = new EncounterData[3];
            for (int i = 0; i < mockEncounters.Length; i++)
            {
                // No GameObject needed for ScriptableObject instantiation
                var data = ScriptableObject.CreateInstance<EncounterData>();
                data.encounterName = $"Test {i}";
                // Optionally, ScriptableObjects also have a 'name' property you might want to set for debugging:
                // data.name = $"TestEncounterSO_{i}";
                createdMockEncounters[i] = data;
            }

            // use reflection to assign the private field
            typeof(EncounterManager)
                .GetField("encounters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(encounterManager, mockEncounters);
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
            var result1 = encounterManager.GetRandomEncounters(3);
            var result2 = encounterManager.GetRandomEncounters(3);

            Assert.AreNotEqual(result1[0], result2[0], "Shuffled encounters should differ between calls");
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
