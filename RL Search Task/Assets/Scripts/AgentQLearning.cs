using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class AgentQLearning : MonoBehaviour
{
    /*
     Q-LEARNING AGENT
     - Check for valid moves
       
     */

    // Start is called before the first frame update
    IEnumerator Start()
    {
        GameObject env = GameObject.Find("Environment");
        QLearning qLearning = env.GetComponent<QLearning>();
        yield return new WaitUntil(() => qLearning.isInitialised); // Wait until variables from QLearning.cs have been initialised



        int[,] rewardMatrix = qLearning.rewards;
        Vector3[,] grid = qLearning.grid;
        GameObject[,] stateObjects = qLearning.stateObjects;

        int[] startPosition = GetAgentStartPosition(grid, stateObjects);
        

        TrainAgent(1, startPosition, rewardMatrix, grid);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int GetCurrentReward(int[,] rewardMatrix, int action, int currentState)
    {
        return rewardMatrix[currentState, action];
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
        Debug.Log("Valid actions: " + string.Join(", ", validActions));
        return validActions;
    }
    
    void TrainAgent(int nIter, int[] startPosition, int[,] rewardMatrix, Vector3[,] grid)
    { 
        for (int i = 0; i < nIter; i++) 
        {
            gameObject.transform.position = new Vector3(startPosition[0], 0.2f, startPosition[1]); // Put in start position
            StartCoroutine(StartEpisode(startPosition, rewardMatrix, grid));
        }
    }

    IEnumerator StartEpisode(int[] startPosition, int[,] rewardMatrix, Vector3[,] grid)
    {
        bool running = true;

        int[] currPosition = startPosition;

        while (running)
        {
            List<string> validMoves = GetValidActions(currPosition, rewardMatrix);
            currPosition = MakeMove(currPosition, validMoves, grid);
            Debug.Log("Starting wait...");
            yield return new WaitForSeconds(0.2f);
            Debug.Log("Done!");
            if (rewardMatrix[currPosition[0], currPosition[1]] == 50) // If on reward state - value may change from 50 in future
            {
                // if more than one reward is in environment add a reward counter, and exit loop when all rewards have been found 
                
                Debug.Log("Reached reward state!");
                break;
            }
        }
    }
    int[] MakeMove(int[] currPosition, List<string> validMoves, Vector3[,] grid)
    {
        int randAction = UnityEngine.Random.Range(0, validMoves.Count);

        if (validMoves[randAction] == "LEFT")
        {
            currPosition[0]--;
        }
        else if (validMoves[randAction] == "RIGHT")
        {
            currPosition[0]++;
        }
        else if (validMoves[randAction] == "UP")
        {
            currPosition[1]++;
        }
        else if (validMoves[randAction] == "DOWN")
        {
            currPosition[1]--;
        }
        gameObject.transform.position = new Vector3(grid[currPosition[0], currPosition[1]].x, 0.2f, grid[currPosition[0], currPosition[1]].z);
        Debug.Log("New position = " + gameObject.transform.position);
        return currPosition;
    }
}
