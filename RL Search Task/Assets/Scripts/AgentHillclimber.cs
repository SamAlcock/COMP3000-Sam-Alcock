using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Xml.Schema;
using UnityEngine;

public class AgentHillclimber : MonoBehaviour
{

      

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

        Hillclimber hillclimber = new();

        hillclimber.Hillclimb(50, startPosition, grid, rewardMatrix); // Increase to get more chance to mutate


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

        
    class Solution
    {
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
        public double FindConstraints(List<int> instructions)
        {
            // This function will calculate the fitness/reward for this generation
            int score = 0;
           





                

            return score;
        }



    }
    class Mutate
    {

        public List<int> SessionReplace(int numOfInstructions, List<int> parent) // Swap mutation?
        {
            System.Random rand = new();

            List<int> child = new List<int>(parent);

            for (int i = 0; i < 1; i++) // May need to specify that it has to be a different swap
            {
                int location = rand.Next(0, child.Count);

                if (child[location] == 0) // Needs to be switched to a random number between 1-4
                {
                    child[location] = 1;
                }
                else
                {
                    child[location] = 0;
                }
            }


            return child;
        }

    }

    public class Hillclimber : MonoBehaviour
    {
        [SerializeField] float stepTime = 0.05f;
        public IEnumerator Hillclimb(int Niter, int[] startPosition, Vector3[,] grid, int[,] rewardMatrix)
        {
            /* 
                * HILLCLIMBER 
                * - Compare child and parent fittness
                * - Best fittness becomes next parent
                */

            Solution solution = new();
            Mutate mutate = new();

            Debug.Log("Initialising...");
            bool running = true;
            int generation = 0;
            gameObject.transform.position = new Vector3(grid[startPosition[0], startPosition[1]].x, 0.2f, grid[startPosition[0], startPosition[1]].z); // Put agent into start position
            int[] currPosition = (int[])startPosition.Clone();

            // Initialise random solution
            List<int> parentInstructions = solution.GenerateSolution();

            // Agent needs to move here and get rewards for initial instructions
            int parentReward = GetCurrentReward(rewardMatrix, currPosition);
            int childReward = 0;

            // Loop for Niter 
            while(running)
            {
                // Mutate
                List<int> childInstructions = mutate.SessionReplace(parentInstructions.Count, parentInstructions);

                // Agent needs to move here, and then get rewards for instructions performed
                for (int i = 0; i < 100; i++)
                {
                    currPosition = MakeMove(childInstructions[i], grid, currPosition);
                    yield return new WaitForSeconds(stepTime);
                    gameObject.transform.position = new Vector3(grid[currPosition[0], currPosition[1]].x, 0.2f, grid[currPosition[0], currPosition[1]].z);

                    // Evaluate
                    childReward += GetCurrentReward(rewardMatrix, currPosition);
                }
                    

                // Pick the next parent
                if (childReward > parentReward)
                {
                    parentInstructions = childInstructions.ToList();
                    parentReward = childReward;

                    Debug.Log("Generation: " + generation + ", New best reward: " + parentReward);
                }
                generation++;
            }

            Debug.Log("Inputs: " + String.Join(", ", parentInstructions));


        }
        int GetCurrentReward(int[,] rewardMatrix, int[] currPosition)
        {
            return rewardMatrix[currPosition[0], currPosition[1]];
        }

        int[] MakeMove(int instruction, Vector3[,] grid, int[] currPosition)
        {
            if (instruction == 0) // Left
            {
                currPosition[0]--;
            }
            else if (instruction == 1) // Right
            {
                currPosition[0]++;
            }
            else if (instruction == 2) // Up
            {
                currPosition[1]++;
            }
            else // Down
            {
                currPosition[1]--;
            }

            return currPosition;
        }
    }

    


}