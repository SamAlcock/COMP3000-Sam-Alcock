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
     *   
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

        CreateStates(grid, statePoint);

        return grid;

    }

    void CreateStates(Vector3[,] grid, GameObject statePoint)
    {


        List<GameObject> stateObjects = new();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                stateObjects.Add(Instantiate(statePoint, grid[x, z], Quaternion.identity));
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
