using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNumExample : MonoBehaviour
{
    public void GenerateRandomNum() {
        int randomValueFromRange = RandomNumGenerator.GetRandomNum(new IntRange(0, 6, 20f),
                     new IntRange(6, 9, 20f),
                     new IntRange(9, 11, 60f));

        UnityEngine.Console.Log($"Random Num with weight is: {randomValueFromRange}");
    }
}
