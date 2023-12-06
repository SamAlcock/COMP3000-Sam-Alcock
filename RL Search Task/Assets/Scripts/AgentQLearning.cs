using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Xml.Schema;
using UnityEngine;

public class AgentQLearning : MonoBehaviour
{
    /*
     Q-LEARNING AGENT
     - Check for valid moves
       
     */

    float totalReward = 0.0f;
    public int generation = 0;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        GameObject env = GameObject.Find("Environment");
        QLearning qLearning = env.GetComponent<QLearning>();
        yield return new WaitUntil(() => qLearning.isInitialised); // Wait until variables from QLearning.cs have been initialised



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
        return rewardMatrix[currPosition[0], currPosition[1]];
    }

    void GetValidActions(int currentState, int[,] rewardMatrix)
    {
        List<int> validActions = new();

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
        // Debug.Log("Valid actions: " + string.Join(", ", validActions));
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
        while (running)
        {
            step++;
            List<string> validMoves = GetValidActions(currPosition, rewardMatrix);
            currPosition = MakeMove(currPosition, validMoves, grid, qTable, rewardMatrix);
            yield return new WaitForSeconds(0.0005f);
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
    int[] MakeMove(int[] currPosition, List<string> validMoves, Vector3[,] grid, float[,] qTable, int[,] rewardMatrix)
    {

        /*
         * Deciding moves:
         * - Get the valid moves
         * - Check Q-Table to see which value is the best
         * - Make sure that value would lead to a valid move
         * - If invalid, set it to -1 in Q-Table
         * - If valid, take action and update Q-Table with results
         */

        List<string> potentialMoves = new() { "LEFT", "RIGHT", "UP", "DOWN" };

        float[] qValues = new float[4];
        int qTableIdx = currPosition[0] * grid.GetLength(0) + currPosition[1];
        for (int i = 0; i < qValues.Length; i++)
        {
            qValues[i] = qTable[qTableIdx, i]; // Get all relevant Q-Values

            if (!validMoves.Contains(potentialMoves[i])) // If move direction is not a valid move
            {
                qValues[i] = -50000; // May need to change this - Q-Values are potentially going below 0 currently, need to remove qValue when its not a valid move
            }
        }


        float bestQValue = qValues.Max();
        int bestIdx = -1;

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
        float gamma = 0.99f;
        float saReward = GetCurrentReward(rewardMatrix, currPosition);
        totalReward += saReward;
        float nsReward = bestQValue;
        float qCurrentState = saReward + (gamma * nsReward);
        // Debug.Log("qCurrentState = " + qCurrentState);
        qValues[bestIdx] = qCurrentState;

        for (int i = 0; i < qValues.Length; i++)
        {
            qTable[qTableIdx, i] = qValues[i]; // Updating Q-Table values
        }

        gameObject.transform.position = new Vector3(grid[currPosition[0], currPosition[1]].x, 0.2f, grid[currPosition[0], currPosition[1]].z);
        return currPosition;
    }
}
