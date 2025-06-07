using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace EditorTests
{
    public class EncounterManagerTests
    {
        private EncounterManager encounterManager;
        private EncounterData[] mockEncounters;

        [SetUp]
        public void Setup()
        {
            // create EncounterManager mono
            var go = new GameObject("EncounterManagerGO");
            encounterManager = go.AddComponent<EncounterManager>();

            // build n store our fake EncounterData array
            mockEncounters = new EncounterData[3];
            for (int i = 0; i < mockEncounters.Length; i++)
            {
                var data = ScriptableObject.CreateInstance<EncounterData>();
                data.encounterName = $"Test {i}";
                mockEncounters[i] = data;
            }

            // inject the private encounter
            var field = typeof(EncounterManager)
                .GetField("encounters", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, "Could not find private field 'encounters' on EncounterManager");
            field.SetValue(encounterManager, mockEncounters);
        }

        [Test]
        public void GetRandomEncounters_ReturnsRequestedCount()
        {
            var result = encounterManager.GetRandomEncounters(5);
            Assert.NotNull(result, "Returned array should not be null");
            Assert.AreEqual(5, result.Length, "Should return exactly 5 encounters");
        }

        [Test]
        public void GetRandomEncounters_LoopsWhenCountExceedsPool()
        {
            var result = encounterManager.GetRandomEncounters(10);
            Assert.NotNull(result);
            Assert.AreEqual(10, result.Length);
        }

        [Test]
        public void GetRandomEncounters_VariesOrderBetweenCalls()
        {
            var first  = encounterManager.GetRandomEncounters(3);
            var second = encounterManager.GetRandomEncounters(3);
            Assert.AreNotEqual(first[0], second[0],
                "Expected first element to differ between two shuffled calls.");
        }

        [TearDown]
        public void Teardown()
        {
            // destroy manager
            if (encounterManager != null)
                Object.DestroyImmediate(encounterManager.gameObject);

            // clean up our ScriptableObjects
            if (mockEncounters != null)
            {
                foreach (var e in mockEncounters)
                    Object.DestroyImmediate(e);
            }
        }
    }
}
