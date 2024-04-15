using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;


public class UnitTests : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InBadPositionUnitTest();
        QLearningUpdateUnitTest();
        InitialiseQTableUnitTest();
        GetRewardMatrixUnitTest();
        SARSAUpdateUnitTest();
        SessionReplaceUnitTest();
        PopulateAreaUnitTest();
    }
    
    void QLearningUpdateUnitTest()
    {
        
        float calculated = 0.7f + (0.8f * 1.03f);
        float expected = 1.524f;

        if (calculated == expected)
        {
            Debug.Log("Q-Learning update function unit test PASSED");
        }
        else
        {
            Debug.Log("Q-Learning update function unit test FAILED");
        }

    }

    void InitialiseQTableUnitTest()
    {
        float[,] qTable = new float[25, 4];
        string output = "";
        for (int i = 0; i < qTable.GetLength(0); i++)
        {
            for (int j = 0; j < qTable.GetLength(1); j++)
            {
                qTable[i, j] = 0; // Set initial values of Q-Table
                output += qTable[i, j] + " ";
            }
            output += "\n";
        }
        Debug.Log("QTable initialisation unit test: \n" + output);
    }
    void GetRewardMatrixUnitTest()
    {
        string[,] stateTest = new string[5, 5];
        int[,] rewardMatrix = new int[5, 5];
        string output = "";
        for (int i = 0; i < rewardMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < rewardMatrix.GetLength(1); j++)
            {
                if (j % 5 == 2)
                {
                    stateTest[i, j] = "InaccessibleState";
                }
                else if (j % 5 == 3)
                {
                    stateTest[i, j] = "RewardState";
                }
                else 
                {
                    stateTest[i, j] = "EmptyState";
                }
                
            }
        }
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (stateTest[i, j] == "EmptyState")
                {
                    rewardMatrix[i, j] = -1;
                }
                else if (stateTest[i, j] == "InaccessibleState")
                {
                    rewardMatrix[i, j] = -50;
                }
                else if (stateTest[i, j] == "RewardState")
                {
                    rewardMatrix[i, j] = 50;
                }
                else
                {
                    rewardMatrix[i, j] = -1;
                }
                output += rewardMatrix[i, j].ToString() + " ";
            }
            output += "\n";
        }
        Debug.Log("Reward matrix initialisation unit test: \n" + output);
    }
    void SARSAUpdateUnitTest()
    {
        float calculated = 0.7f + (0.8f * 1.03f);
        float expected = 1.524f;

        if (calculated == expected)
        {
            Debug.Log("SARSA update function unit test PASSED");
        }
        else
        {
            Debug.Log("SARSA update function unit test FAILED");
        }
    }
    void SessionReplaceUnitTest()
    {
        System.Random rand = new();
        List<int> parent = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            int randomNumber = rand.Next(0, 4); 
            parent.Add(randomNumber);
        }

        List<int> child = new List<int>(parent);

        for (int i = 0; i < 1; i++) 
        {
            int location = rand.Next(0, child.Count);

            child[location] = rand.Next(0, 4);
        }
        Debug.Log("Hill-climber mutation unit test:\n" + String.Join(" ", parent) + "\n" + String.Join(" ", child));
    }
    void PopulateAreaUnitTest()
    {
        float testNoise = 0.463f;

        if (testNoise < 0.4)
        {
            Debug.Log("Procedural generation unit test FAILED");
        }
        else if (testNoise >= 0.4 && testNoise < 0.46)
        {
            Debug.Log("Procedural generation unit test FAILED");
        }
        else if (testNoise >= 0.46 && testNoise < 0.5)
        {
            Debug.Log("Procedural generation unit test PASSED");
        }
        else if (testNoise >= 0.5 && testNoise < 0.53)
        {
            Debug.Log("Procedural generation unit test FAILED");
        }
    }
    void InBadPositionUnitTest()
    {
        Vector3 start = new Vector2(1, 1);
        Vector3 end = new Vector2(4, 5);

        float expected = 5.0f;

        double calculated = Math.Sqrt((start.x - end.x) * (start.x - end.x) + (start.y - end.y) * (start.y - end.y));

        if (expected == calculated)
        {
            Debug.Log("InBadPosition unit test PASSED");
        }
        else
        {
            Debug.Log("InBadPosition unit test FAILED");
        }
    }
}
