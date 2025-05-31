using System;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    [SerializeField] private EncounterData[] encounters;

    // Return an array containing a specificed number of random encounters
    public EncounterData[] GetRandomEncounters(int count)
    {
        // Basic sanity checks
        if (count < 1)
        {
            Debug.LogError("This should not happen");
            return new EncounterData[0];
        }

        if (encounters == null || encounters.Length == 0)
        {
            Debug.LogWarning("Encounters array is null or empty. Cannot return random encounters.");
            return null; 
        }

        EncounterData[] encounterArray = new EncounterData[count];

        //Shuffle a copy
        EncounterData[] shuffledEncounters = (EncounterData[])encounters.Clone();
        ArrayShuffleUtility.Shuffle(shuffledEncounters);

        for (int i = 0; i < count; i++)
        {
            encounterArray[i] = shuffledEncounters[i % shuffledEncounters.Length];
        }

        Debug.Log($"Returning {encounterArray.Length} encounters");

        return encounterArray;
    }
}


public static class ArrayShuffleUtility
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(T[] array)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));

        int n = array.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}