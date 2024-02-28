using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class QLearning : MonoBehaviour
{
    [SerializeField] bool visualiseStates = false;

    public bool isInitialised = false;

    public int[,] rewards;
    public int[] startStateCoords;
    public float[,] qTable;
    public Vector3[,] grid;
    public GameObject[,] stateObjects;


    /*
     *   Q-Learning
     *   - Set up a grid in area - each square in grid acts as a state
     *   - If state point collides with obstacle, it's not accessible
     *   - Reward matrix
     */

    // IF STARTING STATE HAS COLLIDED WITH OBSTACLE, THEN IT WILL SPAWN THE AGENT IN THE CORNER

    // Start is called before the first frame update
    void Start()
    {
        GameObject statePoint = GameObject.Find("State point"), positiveMarker = null, negativeMarker = null;

        foreach(Transform child in gameObject.transform)
        {
            if(child.gameObject.name == "Marker ++")
            {
                positiveMarker = GameObject.Find("Marker ++");
            }
            else if (child.gameObject.name == "Marker --")
            {
                negativeMarker = GameObject.Find("Marker --");
            }
        }
        
        grid = GenerateStateGrid(statePoint, positiveMarker, negativeMarker);
        qTable = InitialiseQTable(grid);

        //isInitialised = true;

    }
    float[,] InitialiseQTable(Vector3[,] grid)
    {
        float[,] qTable = new float[grid.GetLength(0) * grid.GetLength(1), 4]; // Array size: every state * number of actions in every state
        
        for (int i = 0; i < qTable.GetLength(0); i++)
        {
            for (int j = 0; j < qTable.GetLength(1); j++)
            {
                qTable[i, j] = 0; // Set initial values of Q-Table
            }
        }

        return qTable;
    }
    Vector3[,] GenerateStateGrid(GameObject statePoint, GameObject positiveMarker, GameObject negativeMarker)
    {
        // Calculate size of area using corner markers
        float widthX = positiveMarker.transform.position.x - negativeMarker.transform.position.x;
        float widthZ = positiveMarker.transform.position.z - negativeMarker.transform.position.z;

        // Gap between grid points - decrease gap to increase point density
        float xGap = 0.3f;
        float zGap = 0.3f;

        // Calculate how many state points can fit between both corner markers using the defined gaps
        int elementsX = Convert.ToInt32(widthX / xGap);
        int elementsZ = Convert.ToInt32(widthZ / zGap);

        grid = new Vector3[elementsX - 1, elementsZ - 1]; // create grid of dimensions elementsX - 1, elementsZ - 1 

        // Define initial coordinates, with an initial offset of xGap and zGap
        Vector3 initialCoords = new(negativeMarker.transform.position.x + xGap + gameObject.transform.position.x, 0, negativeMarker.transform.position.z + zGap + gameObject.transform.position.z);
        Debug.Log("initialCoords = " + initialCoords.x + ", " + initialCoords.y + ", " + initialCoords.z);

        for (int x = 0; x < elementsX - 1; x++)
        {
            for (int z = 0; z < elementsZ - 1; z++)
            {
                grid[x, z] = new Vector3(initialCoords.x + (xGap * x), 0.01f, initialCoords.z + (zGap * z));
            }
        }

        

        if (gameObject.name != "Environment")
        {

            List<GameObject> stateObjects1D = new();

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;

                if(child.tag == "StartState" || child.tag == "EmptyState" || child.tag == "RewardState" || child.tag == "InaccessibleState")
                {
                    stateObjects1D.Add(child);

                }
            }
            int idx = 0;
            GameObject[,] stateObjects2D = new GameObject[grid.GetLength(0), grid.GetLength(1)];
            for (int x = 0; x < grid.GetLength(0); x++) // formatting for 2D array
            {
                for (int z = 0; z < grid.GetLength(1); z++)
                {
                    stateObjects2D[x, z] = stateObjects1D[idx];
                    idx++;
                }

            }

            stateObjects = stateObjects2D;
            GameObject env = GameObject.Find("Environment");
            QLearning qLearning = env.GetComponent<QLearning>();
            startStateCoords = qLearning.startStateCoords;

            rewards = GetRewardMatrix(grid, stateObjects);

        }
        else
        {
            stateObjects = CreateStates(grid, statePoint);
            startStateCoords = PickSpawnPoint(stateObjects);
            StartCoroutine(TagStates());
        }




        return grid;

    }

    GameObject[,] CreateStates(Vector3[,] grid, GameObject statePoint)
    {

        GameObject[,] stateObjects = new GameObject[grid.GetLength(0), grid.GetLength(1)];

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                stateObjects[x, z] = Instantiate(statePoint, grid[x, z], Quaternion.identity);
                stateObjects[x, z].tag = "EmptyState";

                stateObjects[x, z].transform.parent = gameObject.transform;
            }

        }

        return stateObjects;
    }

    int[] PickSpawnPoint(GameObject[,] stateObjects)
    {
        int x = UnityEngine.Random.Range(0, stateObjects.GetLength(0) - 1);
        int z = UnityEngine.Random.Range(0, stateObjects.GetLength(1) - 1);

        MeshRenderer meshRenderer;

        if (stateObjects[x,z].CompareTag("Obstacle") || stateObjects[x, z].CompareTag("InaccessibleState")) // Might need to add reward state to this
        {
            Debug.Log("Bad state pick");
            PickSpawnPoint(stateObjects);
        }
        else
        {
            meshRenderer = stateObjects[x,z].GetComponent<MeshRenderer>();

            meshRenderer.material.color = Color.blue;
            stateObjects[x, z].tag = "StartState";
            Debug.Log("Picked start state");
        }
        int[] startCoords = new int[2] { x, z };
        return startCoords;
    }

    IEnumerator TagStates()
    {
        TargetPlacing targetPlacing = GetComponent<TargetPlacing>();
        yield return new WaitUntil(() => targetPlacing.targetsPlaced); // Wait until targets have been placed
        Collider[] collisions;

        for (int i = 0; i < stateObjects.GetLength(0); i++)
        {
            for (int j = 0; j < stateObjects.GetLength(1); j++)
            {
                collisions = Physics.OverlapSphere(stateObjects[i,j].transform.position, 0.05f);
                foreach (Collider collision in collisions)
                {
                    if (collision.CompareTag("Obstacle"))
                    {
                        stateObjects[i, j].tag = "InaccessibleState";
                        MeshRenderer meshRenderer = stateObjects[i,j].GetComponent<MeshRenderer>();
                        meshRenderer.material.color = Color.grey;
                    }
                    else if (collision.CompareTag("Target"))
                    {
                        stateObjects[i, j].tag = "RewardState";
                        MeshRenderer meshRenderer = stateObjects[i, j].GetComponent<MeshRenderer>();
                        meshRenderer.material.color = Color.yellow;
                    }
                }
            }
        }
        rewards = GetRewardMatrix(grid, stateObjects);
        isInitialised = true;

    }

    int[,] GetRewardMatrix(Vector3[,] grid, GameObject[,] stateObjects)
    {
        int[,] rewardMatrix = new int[grid.GetLength(0), grid.GetLength(1)];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (stateObjects[i, j].CompareTag("EmptyState"))
                {
                    rewardMatrix[i, j] = -1;
                }
                else if (stateObjects[i, j].CompareTag("InaccessibleState"))
                {
                    rewardMatrix[i, j] = -50;
                }
                else if (stateObjects[i, j].CompareTag("RewardState"))
                {
                    rewardMatrix[i, j] = 50;
                }
                else
                {
                    rewardMatrix[i, j] = -1;
                }
            }
        }
        string str = "";
        for (int i = 0; i < rewardMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < rewardMatrix.GetLength(1); j++)
            {
                str += " " + rewardMatrix[i, j];

            }
            str += "\n";
        }
        Debug.Log(str);

        return rewardMatrix;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
