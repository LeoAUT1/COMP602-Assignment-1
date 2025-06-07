using NUnit.Framework;
using UnityEngine;

namespace EditorTests
{
    public class EncounterManagerTests
    {
        private EncounterManager encounterManager;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject("EncounterManagerGO");
            encounterManager = go.AddComponent<EncounterManager>();

            // Create mock encounters
            EncounterData[] mockEncounters = new EncounterData[3];
            for (int i = 0; i < mockEncounters.Length; i++)
            {
                GameObject obj = new GameObject($"Encounter_{i}");
                var data = obj.AddComponent<EncounterData>();
                data.encounterName = $"Test {i}";
                mockEncounters[i] = data;
            }

            // Use reflection to assign the private field
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

            // Not a guaranteed test, but likely to catch shuffling
            Assert.AreNotEqual(result1[0], result2[0], "Shuffled encounters should differ between calls");
        }

        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(encounterManager.gameObject);
        }
    }
}
