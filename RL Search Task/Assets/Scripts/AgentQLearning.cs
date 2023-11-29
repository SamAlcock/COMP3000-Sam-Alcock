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
        int[] currPosition = GetCurrentPosition(grid, stateObjects);
        

        TrainAgent(100, startPosition, currPosition, rewardMatrix);

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

        if (rewardMatrix[currPosition[0] + 1, currPosition[1]] != -50 && currPosition[0] + 1 < rewardMatrix.GetLength(0))
        {
            validActions.Add("RIGHT");
        }
        if (rewardMatrix[currPosition[0] - 1, currPosition[1]] != -50 && currPosition[0] - 1 >= 0)
        {
            validActions.Add("LEFT");
        }
        if (rewardMatrix[currPosition[0], currPosition[1] - 1] != -50 && currPosition[1] - 1 >= 0)
        {
            validActions.Add("DOWN");
        }
        if (rewardMatrix[currPosition[0], currPosition[1] + 1] != -50 && currPosition[0] + 1 < rewardMatrix.GetLength(1))
        {
            validActions.Add("UP");
        }

        return validActions;
    }
    
    void TrainAgent(int nIter, int[] startPosition, int[] currPosition, int[,] rewardMatrix)
    { 
        for (int i = 0; i < nIter; i++) 
        {
            gameObject.transform.position = new Vector3(startPosition[0], 0.2f, startPosition[1]); // Put in start position
            StartEpisode(startPosition, rewardMatrix);
        }
    }

    void StartEpisode(int[] startPosition, int[,] rewardMatrix)
    {
        bool running = true;

        int[] currGridPos = new int[2];

        while (running)
        {
            List<string> validMoves = GetValidActions(currGridPos, rewardMatrix);
        }
    }
    int[] MakeMove(int[] currentGridPos, List<string> validMoves)
    {
        int randAction = UnityEngine.Random.Range(0, validMoves.Count);
        int action = 0;

        if (validMoves[randAction] == "LEFT")
        {
            action = 1;
        }
        else if (validMoves[randAction] == "RIGHT")
        {
            action = 2;
        }
        else if (validMoves[randAction] == "UP")
        {
            action = 3;
        }
        else if (validMoves[randAction] == "DOWN")
        {
            action = 4;
        }

        return new int[] { action }; // remove this 
        //int action = validMoves[randAction];
    }
}
