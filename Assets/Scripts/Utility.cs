using System;
using System.Collections;
using System.Collections.Generic;

public class Utility
{
    //Fisher-Yates Shuffle
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        Random randNumGen = new Random(seed);
        for (int i = 0; i < array.Length-1; i++)
        {
            int randomIndex = randNumGen.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

}
