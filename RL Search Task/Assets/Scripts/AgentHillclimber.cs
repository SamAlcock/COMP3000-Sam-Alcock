using Mono.Reflection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Transactions;
using System.Xml.Schema;
using UnityEngine;

public class AgentHillclimber : MonoBehaviour
{
    [SerializeField] float stepTime = 0.05f;


    IEnumerator Start()
    {
        Debug.Log("Hillclimber starting");
        GameObject master = GameObject.Find("Master");
        EnvironmentManager environmentManager = master.GetComponent<EnvironmentManager>();
        yield return new WaitUntil(() => environmentManager.isInitialised);

        GameObject env = GameObject.Find("Environment Hillclimber");


        yield return new WaitUntil(() => env.GetComponent<QLearning>().isInitialised); // Wait until variables from QLearning.cs have been initialised
        QLearning qLearning = env.GetComponent<QLearning>();

        int[,] rewardMatrix = qLearning.rewards;
        Vector3[,] grid = qLearning.grid;
        GameObject[,] stateObjects = qLearning.stateObjects;

        int[] startPosition = GetAgentStartPosition(grid, stateObjects);

        Debug.Log("Starting Hillclimber coroutine");
        StartCoroutine(Hillclimb(startPosition, grid, rewardMatrix)); // Increase to get more chance to mutate


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

    int numOfInputs = 4;
    int numOfInstructions = 250;

    public List<int> GenerateSolution() // Generate a random array of inputs
    {
        List<int> instructions = new();
        System.Random rand = new();

        for (int i = 0; i < numOfInstructions; i++)
        {
            instructions.Add(rand.Next(0, numOfInputs)); // maybe add something to ensure agent doesn't go back on itself
        }

        return instructions;
    }

    public List<int> SessionReplace(int numOfInstructions, List<int> parent) // Swap mutation?
    {
        System.Random rand = new();

        List<int> child = new List<int>(parent);

        for (int i = 0; i < 1; i++) // May need to specify that it has to be a different swap
        {
            int location = rand.Next(0, child.Count);

            child[location] = rand.Next(0, 4); // Pick number from 1 to 3 (each number relates to a direction)
        }


        return child;
    }

    public IEnumerator Hillclimb(int[] startPosition, Vector3[,] grid, int[,] rewardMatrix)
    {
        /* 
            * HILLCLIMBER 
            * - Compare child and parent fittness
            * - Best fittness becomes next parent
            */

        

        Debug.Log("Initialising...");
        bool running = true;
        int generation = 0;
        gameObject.transform.position = new Vector3(grid[startPosition[0], startPosition[1]].x, 0.2f, grid[startPosition[0], startPosition[1]].z); // Put agent into start position
        int[] currPosition = (int[])startPosition.Clone();

        // Initialise random solution
        List<int> parentInstructions = GenerateSolution();

        // Agent needs to move here and get rewards for initial instructions
        int parentReward = GetCurrentReward(rewardMatrix, currPosition);
        int childReward = 0;

        // Loop for Niter 
        while(running)
        {
            // Mutate
            List<int> childInstructions = SessionReplace(parentInstructions.Count, parentInstructions);

            // Agent needs to move here, and then get rewards for instructions performed
            for (int i = 0; i < 100; i++)
            {
                currPosition = MakeMove(childInstructions[i], grid, currPosition, rewardMatrix, i, childInstructions);
                yield return new WaitForSeconds(stepTime);
                gameObject.transform.position = new Vector3(grid[currPosition[0], currPosition[1]].x, 0.2f, grid[currPosition[0], currPosition[1]].z);

                // Evaluate
                childReward += GetCurrentReward(rewardMatrix, currPosition);

                if (rewardMatrix[currPosition[0], currPosition[1]] == 50)
                {
                    Debug.Log("Generation: " + generation + ", reached reward state! Total reward = " + childReward);
                    break;
                }
                else if (i == 100)
                {
                    Debug.Log("Generation: " + generation + ", maximum steps for single iteration reached. Total reward = " + childReward);
                    break;
                }
                Debug.Log("i = " + i);
            }
                    

            // Pick the next parent
            if (childReward > parentReward)
            {
                parentInstructions = childInstructions.ToList();
                parentReward = childReward;

                Debug.Log("Hillclimber Generation: " + generation + ", New best reward: " + parentReward);
            }
            generation++;
        }

        Debug.Log("Hillclimber Inputs: " + String.Join(", ", parentInstructions));


    }
    int GetCurrentReward(int[,] rewardMatrix, int[] currPosition)
    {
        return rewardMatrix[currPosition[0], currPosition[1]];
    }

    int[] MakeMove(int instruction, Vector3[,] grid, int[] currPosition, int[,] rewardMatrix, int iteration, List<int> allInstructions)
    {
        System.Random rand = new();
        List<int> validInstructions = GetValidActions(currPosition, rewardMatrix);

        if (!validInstructions.Contains(instruction))
        {
            int newInstructionIdx = rand.Next(0, validInstructions.Count);
            instruction = validInstructions[newInstructionIdx];
            allInstructions[iteration] = instruction;
        }

        currPosition = DetermineMovement(currPosition, instruction);


        return currPosition;
    }

    List<int> GetValidActions(int[] currPosition, int[,] rewardMatrix)
    {
        List<int> validActions = new();

        if (currPosition[0] + 1 < rewardMatrix.GetLength(0) && rewardMatrix[currPosition[0] + 1, currPosition[1]] != -50)
        {
            validActions.Add(1);
        }
        if (currPosition[0] - 1 >= 0 && rewardMatrix[currPosition[0] - 1, currPosition[1]] != -50)
        {
            validActions.Add(0);
        }
        if (currPosition[1] - 1 >= 0 && rewardMatrix[currPosition[0], currPosition[1] - 1] != -50)
        {
            validActions.Add(3);
        }
        if (currPosition[1] + 1 < rewardMatrix.GetLength(1) && rewardMatrix[currPosition[0], currPosition[1] + 1] != -50)
        {
            validActions.Add(2);
        }
        Debug.Log("Hillclimber valid actions: " + string.Join(", ", validActions));
        return validActions;
    }

    int[] DetermineMovement(int[] pos, int instruction)
    {
        if (instruction == 0) // Left
        {
            pos[0]--;
        }
        else if (instruction == 1) // Right
        {
            pos[0]++;
        }
        else if (instruction == 2) // Up
        {
            pos[1]++;
        }
        else // Down
        {
            pos[1]--;
        }
        return pos;
    }

    
}