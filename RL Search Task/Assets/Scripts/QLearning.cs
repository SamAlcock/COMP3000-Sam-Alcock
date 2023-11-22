using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Timeline;

public class QLearning : MonoBehaviour
{
    [SerializeField] bool visualiseStates = false;

    /*
     *   Q-Learning
     *   - Set up a grid in area - each square in grid acts as a state
     *   - If state point collides with obstacle, it's not accessible
     *   - Reward matrix
     */


    // Start is called before the first frame update
    void Start()
    {
        GameObject statePoint = GameObject.Find("State point");
        GameObject positiveMarker = GameObject.Find("Marker ++");
        GameObject negativeMarker = GameObject.Find("Marker --");

        GenerateStateGrid(statePoint, positiveMarker, negativeMarker);  
    }

    Vector3[,] GenerateStateGrid(GameObject statePoint, GameObject positiveMarker, GameObject negativeMarker)
    {


        // Calculate size of area using corner markers
        float widthX = positiveMarker.transform.position.x - negativeMarker.transform.position.x;
        float widthZ = positiveMarker.transform.position.z - negativeMarker.transform.position.z;

        // Gap between grid points - decrease gap to increase point density
        float xGap = 0.2f;
        float zGap = 0.2f;

        // Calculate how many state points can fit between both corner markers using the defined gaps
        int elementsX = Convert.ToInt32(widthX / xGap);
        int elementsZ = Convert.ToInt32(widthZ / zGap);

        Vector3[,] grid = new Vector3[elementsX - 1, elementsZ - 1]; // create grid of dimensions elementsX - 1, elementsZ - 1 

        // Define initial coordinates, with an initial offset of xGap and zGap
        Vector3 initialCoords = new(negativeMarker.transform.position.x + xGap, 0, negativeMarker.transform.position.z + zGap);

        for (int x = 0; x < elementsX - 1; x++)
        {
            for (int z = 0; z < elementsZ - 1; z++)
            {
                grid[x, z] = new Vector3(initialCoords.x + (xGap * x), 0.01f, initialCoords.z + (zGap * z));
            }
        }

        GameObject[,] stateObjects = CreateStates(grid, statePoint);
        PickSpawnPoint(stateObjects);
        GetRewardMatrix(grid, stateObjects);

        return grid;

    }

    GameObject[,] CreateStates(Vector3[,] grid, GameObject statePoint)
    {


        GameObject[,] stateObjects = new GameObject[grid.GetLength(0), grid.GetLength(1)];

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                stateObjects[x,z] = Instantiate(statePoint, grid[x, z], Quaternion.identity);
                stateObjects[x, z].tag = "EmptyState";
            }

        }

        return stateObjects;
    }

    void PickSpawnPoint(GameObject[,] stateObjects)
    {
        int x = UnityEngine.Random.Range(0, stateObjects.GetLength(0) - 1);
        int z = UnityEngine.Random.Range(0, stateObjects.GetLength(1) - 1);

        MeshRenderer meshRenderer;

        if (stateObjects[x,z].CompareTag("Obstacle") || stateObjects[x, z].CompareTag("InaccessibleState"))
        {
            Debug.Log("Bad state pick");
            PickSpawnPoint(stateObjects);
        }
        else
        {
            meshRenderer = stateObjects[x,z].GetComponent<MeshRenderer>();

            meshRenderer.material.color = Color.blue;
            stateObjects[x, z].tag = "StartState";
        }
    }

    int[,] GetRewardMatrix(Vector3[,] grid, GameObject[,] stateObjects)
    {
        int[,] rewardMatrix = new int[grid.GetLength(0), grid.GetLength(1)];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                Debug.Log(stateObjects[i, j].tag);
                if (stateObjects[i, j].CompareTag("EmptyState"))
                {
                    rewardMatrix[i, j] = -1;
                }
                else if (stateObjects[i, j].CompareTag("InaccessibleState"))
                {
                    rewardMatrix[i, j] = -50;
                    Debug.Log(rewardMatrix[i, j]);
                }
                else if (stateObjects[i, j].CompareTag("RewardState"))
                {
                    rewardMatrix[i, j] = 50;
                    Debug.Log(rewardMatrix[i, j]);
                }
                else
                {
                    rewardMatrix[i, j] = 0;
                    Debug.Log(rewardMatrix[i, j]);
                }
                
            }
        }

        

        return rewardMatrix;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
