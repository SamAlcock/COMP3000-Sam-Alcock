using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Xml.Schema;
using UnityEngine;

public class AgentSARSA : MonoBehaviour
{
    /*
     Q-LEARNING AGENT
     - Check for valid moves
     */

    float totalReward = 0.0f;
    public int generation = 0;

    [SerializeField] float stepTime = 0.05f;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        GameObject master = GameObject.Find("Master");
        EnvironmentManager environmentManager = master.GetComponent<EnvironmentManager>();

        yield return new WaitUntil(() => environmentManager.isInitialised);

        GameObject env = GameObject.Find("Environment(Clone)");
        

        yield return new WaitUntil(() => env.GetComponent<QLearning>().isInitialised); // Wait until variables from QLearning.cs have been initialised
        QLearning qLearning = env.GetComponent<QLearning>();

        int[,] rewardMatrix = qLearning.rewards;
        Vector3[,] grid = qLearning.grid;
        GameObject[,] stateObjects = qLearning.stateObjects;

        float[,] qTable = qLearning.qTable;
        
        int[] startPosition = GetAgentStartPosition(grid, stateObjects);
        

        TrainAgent(1, startPosition, rewardMatrix, grid, qTable);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int GetCurrentReward(int[,] rewardMatrix, int[] currPosition)
    {
        // currPosition going to -1 for some reason
        Debug.Log("currPosition[0], currPosition[1] = " + currPosition[0] + ", " + currPosition[1]);
        return rewardMatrix[currPosition[0], currPosition[1]];
    }

    
    int[] GetAgentStartPosition(Vector3[,] grid, GameObject[,] stateObjects)
    {
        int[] startPosition = new int[2];
        for (int i = 0; i < stateObjects.GetLength(0); i++)
        {
            for (int j = 0; j < stateObjects.GetLength(1); j++)
            {

                if (stateObjects[i, j].CompareTag("StartState"))
                {

                    startPosition[0] = i;
                    startPosition[1] = j;

                    return startPosition;
                }
            }
        }
        return startPosition;
    }
    int[] GetCurrentPosition(Vector3[,] grid, GameObject[,] stateObjects)
    {
        int[] currPosition = new int[2];
        for (int i = 0; i < stateObjects.GetLength(0); i++)
        {
            for (int j = 0; j < stateObjects.GetLength(1); j++)
            {
                if(gameObject.transform.position.x == stateObjects[i, j].transform.position.x && gameObject.transform.position.z == stateObjects[i, j].transform.position.z)
                {
                    currPosition[0] = i;
                    currPosition[1] = j;

                    return currPosition;
                }
            }
        }


        return currPosition;
    }
    List<string> GetValidActions(int[] currPosition, int[,] rewardMatrix)
    {
        List<string> validActions = new();

        if (currPosition[0] + 1 < rewardMatrix.GetLength(0) && rewardMatrix[currPosition[0] + 1, currPosition[1]] != -50)
        {
            validActions.Add("RIGHT");
        }
        if (currPosition[0] - 1 >= 0 && rewardMatrix[currPosition[0] - 1, currPosition[1]] != -50)
        {
            validActions.Add("LEFT");
        }
        if (currPosition[1] - 1 >= 0 && rewardMatrix[currPosition[0], currPosition[1] - 1] != -50)
        {
            validActions.Add("DOWN");
        }
        if (currPosition[1] + 1 < rewardMatrix.GetLength(1) && rewardMatrix[currPosition[0], currPosition[1] + 1] != -50)
        {
            validActions.Add("UP");
        }
        return validActions;
    }
    
    void TrainAgent(int nIter, int[] startPosition, int[,] rewardMatrix, Vector3[,] grid, float[,] qTable)
    { 
        for (int i = 0; i < nIter; i++) 
        {
            
            StartCoroutine(StartEpisode(startPosition, rewardMatrix, grid, qTable));
        }
    }

    IEnumerator StartEpisode(int[] startPosition, int[,] rewardMatrix, Vector3[,] grid, float[,] qTable)
    {
        bool running = true;
        totalReward = 0;
        gameObject.transform.position = new Vector3(grid[startPosition[0], startPosition[1]].x, 0.2f, grid[startPosition[0], startPosition[1]].z); // Put in start position
        int[] currPosition = (int[])startPosition.Clone();
        int step = 0;
        string prevAction = null;
        while (running)
        {
            step++;
            List<string> validMoves = GetValidActions(currPosition, rewardMatrix);
            var values = MakeMove(currPosition, validMoves, grid, qTable, rewardMatrix, prevAction);
            currPosition = values.Item1;
            prevAction = values.Item2;
            yield return new WaitForSeconds(stepTime);
            if (rewardMatrix[currPosition[0], currPosition[1]] == 50) // If on reward state - value may change from 50 in future
            {
                // if more than one reward is in environment add a reward counter, and exit loop when all rewards have been found 

                Debug.Log("Generation: " + generation + ", reached reward state! Total reward = " + totalReward);
                break;
            }
            else if (step == 100)
            {
                Debug.Log("Generation: " + generation + ", maximum steps for single iteration reached. Total reward = " + totalReward);
                break;
            }
        }
        generation++;
        StartCoroutine(StartEpisode(startPosition, rewardMatrix, grid, qTable));
    }
    (int[], string) MakeMove(int[] currPosition, List<string> validMoves, Vector3[,] grid, float[,] qTable, int[,] rewardMatrix, string prevAction) // negative currPosition values could be because of SARSA deciding future move before taking it
    {

        /*
             * Q-Learning takes the maximum Q-Value of the state it has moved to in its calculations
             * SARSA decides the action to take in the next state and adds it to its calculations
             * nsReward needs to be Q-Value of the next action
             * 
             * SARSA - get decided action, choose next action, update, repeat
        */

        List<string> potentialMoves = new() { "LEFT", "RIGHT", "UP", "DOWN" };

        float[] qValues = new float[4];

        int qTableIdx = currPosition[0] * grid.GetLength(0) + currPosition[1];
        qValues = GetRelevantQValues(qTableIdx, qTable, qValues, validMoves, potentialMoves);
        float bestQValue = qValues.Max();
        int bestIdx = -1;

        if (prevAction == null)
        {

            for (int i = 0; i < qValues.Length; i++)
            {
                if (bestQValue == qValues[i])
                {
                    bestIdx = i; // This is the index value of the chosen move
                }
            }

            if (bestIdx == 0) // Left
            {
                currPosition[0]--;
            }
            else if (bestIdx == 1) // Right
            {
                currPosition[0]++;
            }
            else if (bestIdx == 2) // Up
            {
                currPosition[1]++;
            }
            else if (bestIdx == 3) // Down
            {
                currPosition[1]--;
            }
        }
      
        else if (prevAction == "LEFT") // Left
        {
            currPosition[0]--;
            bestIdx = 0;
        }
        else if (prevAction == "RIGHT") // Right
        {
            currPosition[0]++;
            bestIdx = 1;
        }
        else if (prevAction == "UP") // Up
        {
            currPosition[1]++;
            bestIdx = 2;
        }
        else if (prevAction == "DOWN") // Down
        {
            currPosition[1]--;
            bestIdx = 3;
        }

        float gamma = 0.99f;
        float saReward = GetCurrentReward(rewardMatrix, currPosition);
        totalReward += saReward;

        float[] qValuesNew = new float[4];
        int qTableIdxNew = currPosition[0] * grid.GetLength(0) + currPosition[1];
        List<string> newValidMoves = GetValidActions(currPosition, rewardMatrix);
        qValuesNew = GetRelevantQValues(qTableIdxNew, qTable, qValuesNew, newValidMoves, potentialMoves);
        prevAction = GetNextPrevAction(qValuesNew);
        bestQValue = qValuesNew.Max();
        float nsReward = bestQValue;

        float qCurrentState = saReward + (gamma * nsReward);
        qValues[bestIdx] = qCurrentState;

        for (int i = 0; i < qValues.Length; i++)
        {
            qTable[qTableIdx, i] = qValues[i]; // Updating Q-Table values
        }

        gameObject.transform.position = new Vector3(grid[currPosition[0], currPosition[1]].x, 0.2f, grid[currPosition[0], currPosition[1]].z);
        return (currPosition, prevAction);
    }
    float[] GetRelevantQValues(int qTableIdx, float[,] qTable, float[] qValues, List<string> validMoves, List<string> potentialMoves)
    {
        for (int i = 0; i < qValues.Length; i++)
        {
            qValues[i] = qTable[qTableIdx, i]; // Get all relevant Q-Values

            if (!validMoves.Contains(potentialMoves[i])) // If move direction is not a valid move
            {
                qValues[i] = -50000; // May need to change this - Q-Values are potentially going below 0 currently, need to remove qValue when its not a valid move
            }
        }
        return qValues;
    }

    string GetNextPrevAction(float[] qValues)
    {
        int bestIdx = -1;
        float bestQValue = qValues.Max();
        string prevAction = null;
        for (int i = 0; i < qValues.Length; i++)
        {
            if (bestQValue == qValues[i])
            {
                bestIdx = i; // This is the index value of the chosen move
            }
        }
        if (bestIdx == 0) // Left
        {
            prevAction = "LEFT";
        }
        else if (bestIdx == 1) // Right
        {
            prevAction = "RIGHT";
        }
        else if (bestIdx == 2) // Up
        {
            prevAction = "UP";
        }
        else if (bestIdx == 3) // Down
        {
            prevAction = "DOWN";
        }

        Debug.Log("Next prevAction is " + prevAction + ", Q-Value is " + bestQValue);
        return prevAction;
    }
}
