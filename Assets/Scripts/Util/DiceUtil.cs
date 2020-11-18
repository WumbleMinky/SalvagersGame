using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceUtil
{
    public static int rollDie(int numOfSides=6)
    {
        return Random.Range(1, numOfSides+1);
    }

    public static int[] rollDice(int number)
    {
        int[] results = new int[number];
        for(int i = 0; i < number; i++)
        {
            results[i] = rollDie();
        }
        return results;
    }

    public static bool hasPairLessThan(int[] roll, int lessThan)
    {
        Dictionary<int, int> b = roll.GroupBy(item => item).ToDictionary(item => item.Key, item => item.Count());
        foreach(int val in new int[] { 1, 2, 3, 4, 5, 6 })
        {
            if (val > lessThan)
                break;
            if (b.ContainsKey(val) && b[val] >= 2)
            {
                return true;
            }
        }
        return false;
    }
}
