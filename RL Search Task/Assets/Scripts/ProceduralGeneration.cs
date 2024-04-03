using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using JetBrains.Annotations;

public class ProceduralGeneration : MonoBehaviour
{


    [SerializeField] bool visualiseGrid = true;

    GameObject positiveMarker;
    GameObject negativeMarker;


    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.name.Contains("Clone") || gameObject.name == "Environment Hillclimber")
        {
            ProceduralGeneration proceduralGeneration = gameObject.GetComponent<ProceduralGeneration>();
            proceduralGeneration.enabled = false;
        }
        else
        {
            positiveMarker = GameObject.Find("Marker ++");
            negativeMarker = GameObject.Find("Marker --");

            float threshold = 0.3f;

            Vector3[,] grid = GenerateGrid(positiveMarker, negativeMarker);

            float[,] noiseMap = GenerateNoiseMap(grid.GetLength(0), grid.GetLength(1), 5);

            if (visualiseGrid)
            {
                VisualiseGrid(grid, noiseMap, threshold);
            }


            List<GameObject> obstaclesInArea = PopulateArea(grid, noiseMap, positiveMarker, negativeMarker, threshold);

            List<GameObject> obstacles = ObstaclesToList();

            Debug.Log("Amount of obstacles in area: " + obstacles.Count);


        }


    }
    Vector3[,] GenerateGrid(GameObject positiveMarker, GameObject negativeMarker)
    {

        // Calculate size of area using corner markers
        float widthX = positiveMarker.transform.position.x - negativeMarker.transform.position.x;
        float widthZ = positiveMarker.transform.position.z - negativeMarker.transform.position.z;

        // Gap between grid points - decrease gap to increase point density
        float xGap = 0.3f;
        float zGap = 0.3f;

        // Calculate how many points can fit between both corner markers using the defined gaps
        int elementsX = Convert.ToInt32(widthX / xGap);
        int elementsZ = Convert.ToInt32(widthZ / zGap);

        Vector3[,] grid = new Vector3[elementsX - 1, elementsZ - 1]; // create grid of dimensions elementsX - 1, elementsZ - 1 

        // Define initial coordinates, with an initial offset of xGap and zGap to avoid placing points in wall
        Vector3 initialCoords = new(negativeMarker.transform.position.x + xGap, 0, negativeMarker.transform.position.z + zGap);

        for (int x = 0; x < elementsX - 1; x++)
        {
            for (int z = 0; z < elementsZ - 1; z++)
            {
                grid[x, z] = new Vector3(initialCoords.x + (xGap * x), 0.01f, initialCoords.z + (zGap * z));
            }
        }
        return grid;
    }

    float[,] GenerateNoiseMap(int mapWidth, int mapDepth, float scale)
    {
        float[,] noiseMap = new float[mapWidth, mapDepth]; // create an empty noise map with mapWidth and mapDepth coordinates

        System.Random rnd = new System.Random();
        float seed = rnd.Next(0, 100000) + 0.01f; // random seed to sample new Perlin noise each time

        Debug.Log("Seed is: " + seed);

        for (int xIndex = 0; xIndex < mapWidth; xIndex++)
        {
            for (int zIndex = 0; zIndex < mapDepth; zIndex++)
            {
                // Calculate samples based on coordinates and scale
                float sampleX = xIndex / scale;
                float sampleZ = zIndex / scale;

                float noise = Mathf.PerlinNoise(sampleX + seed, sampleZ + seed); // generate noise using PerlinNoise

                noiseMap[xIndex, zIndex] = noise;
            }
        }

        return noiseMap;
    }
    List<Vector3> GetMiddlePoints(Vector3[,] grid)
    {
        List<Vector3> middlePoints = new();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                if (x % 9 == 0 && x != 0 && x != grid.GetLength(0) - 1 && z % 9 == 0 && z != 0 && z != grid.GetLength(1) - 1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int[] operators = GetOperators(i);
                        middlePoints.Add(grid[x + operators[0], z + operators[1]]);
                    }
                }
            }

        }

        return middlePoints;

    }

    List<float> GetMiddleNoise(Vector3[,] grid, float[,] noiseMap)
    {
        List<float> middleNoise = new();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                if (x % 9 == 0 && x != 0 && x != grid.GetLength(0) - 1 && z % 9 == 0 && z != 0 && z != grid.GetLength(1) - 1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int[] operators = GetOperators(i);
                        middleNoise.Add(noiseMap[x + operators[0], z + operators[1]]);
                    }
                }
            }

        }

        return middleNoise;
    }

    void VisualiseGrid(Vector3[,] grid, float[,] noiseMap, float threshold)
    {

        GameObject coordPoint = GameObject.Find("Coordinate point");
        List<GameObject> currInstance = new();
        List<Color> colors = new();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                if (x % 9 == 0 && x != 0 && x != grid.GetLength(0) - 1 && z % 9 == 0 && z != 0 && z != grid.GetLength(1) - 1)
                {

                    for (int i = 0; i < 4; i++)
                    {
                        int[] operators = GetOperators(i);

                        colors.Add(Color.Lerp(Color.red, Color.blue, noiseMap[x + operators[0], z + operators[1]])); // assign colour based on Perlin noise value 
                        currInstance.Add(Instantiate(coordPoint, grid[x + operators[0], z + operators[1]], Quaternion.identity)); // show coordinate points on floor (just to visualise the grid)
                        currInstance.Last().GetComponent<MeshRenderer>().material.SetColor("_Color", colors.Last());
                    }
                }


            }

        }
    }

    int[] GetOperators(int key) // used to find middle points in each quadrant
    {
        int[] operators = new int[2];
        int operator1 = 0;
        int operator2 = 0;

        if (key == 0)
        {
            operator1 = 5;
            operator2 = 5;
        }
        else if (key == 1)
        {
            operator1 = 5;
            operator2 = -5;
        }
        else if (key == 2)
        {
            operator1 = -5;
            operator2 = 5;
        }
        else if (key == 3)
        {
            operator1 = -5;
            operator2 = -5;
        }

        operators[0] = operator1;
        operators[1] = operator2;

        return operators;
    }

    List<GameObject> PopulateArea(Vector3[,] grid, float[,] noiseMap, GameObject positiveMarker, GameObject negativeMarker, float threshold)
    {
        List<GameObject> obstacles = new();

        GameObject chunk1 = GameObject.Find("Chunk 1");
        GameObject chunk2 = GameObject.Find("Chunk 2");
        GameObject chunk3 = GameObject.Find("Chunk 3");
        GameObject chunk4 = GameObject.Find("Chunk 4");
        GameObject chunk5 = GameObject.Find("Chunk 5");
        GameObject chunk6 = GameObject.Find("Chunk 6");
        GameObject chunk7 = GameObject.Find("Chunk 7");
        GameObject chunk8 = GameObject.Find("Chunk 8");

        List<GameObject> currChunks = new();

        List<Vector3> middlePoints = GetMiddlePoints(grid);
        List<float> middleNoise = GetMiddleNoise(grid, noiseMap);

        for (int i = 0; i < middlePoints.Count; i++) // generate chunk from middle point to avoid overlap
        {
            int rotation = GetRandomRotation();

            Vector3 position = middlePoints[i];

            if (middleNoise[i] < 0.4)
            {
                currChunks.Add(Instantiate(chunk1, position, Quaternion.Euler(0, rotation, 0)));
            }
            else if (middleNoise[i] >= 0.4 && middleNoise[i] < 0.46)
            {
                currChunks.Add(Instantiate(chunk2, position, Quaternion.Euler(0, rotation, 0)));
            }
            else if (middleNoise[i] >= 0.46 && middleNoise[i] < 0.5)
            {
                currChunks.Add(Instantiate(chunk3, position, Quaternion.Euler(0, rotation, 0)));
            }
            else if (middleNoise[i] >= 0.5 && middleNoise[i] < 0.53)
            {
                currChunks.Add(Instantiate(chunk4, position, Quaternion.Euler(0, rotation, 0)));
            }
            else if (middleNoise[i] >= 0.53 && middleNoise[i] < 0.59)
            {
                currChunks.Add(Instantiate(chunk5, position, Quaternion.Euler(0, rotation, 0)));
            }
            else if (middleNoise[i] >= 0.59 && middleNoise[i] < 0.65)
            {
                currChunks.Add(Instantiate(chunk6, position, Quaternion.Euler(0, rotation, 0)));
            }
            else if (middleNoise[i] >= 0.65 && middleNoise[i] < 0.7)
            {
                currChunks.Add(Instantiate(chunk7, position, Quaternion.Euler(0, rotation, 0)));
            }
            else if (middleNoise[i] >= 0.7 && middleNoise[i] < 1)
            {
                currChunks.Add(Instantiate(chunk8, position, Quaternion.Euler(0, rotation, 0)));
            }
        }

        chunk1.SetActive(false);
        chunk2.SetActive(false);
        chunk3.SetActive(false);
        chunk4.SetActive(false);
        chunk5.SetActive(false);
        chunk6.SetActive(false);
        chunk7.SetActive(false);
        chunk8.SetActive(false);



        GameObject env = GameObject.Find("Environment");

        foreach(GameObject chunk in currChunks)
        {
            chunk.transform.parent = env.transform;
        }

        return obstacles;
    }

    int GetRandomRotation()
    {
        System.Random rnd = new System.Random();

        int rotationKey = rnd.Next(0, 4);

        if (rotationKey == 0)
        {
            return 0;
        }
        else if (rotationKey == 1)
        {
            return 90;
        }
        else if (rotationKey == 2)
        {
            return 180;
        }
        else if (rotationKey == 3)
        {
            return 270;
        }
        else
        {
            return 0;
        }
    }

    List<GameObject> ObstaclesToList()
    {
        List<GameObject> obstacles = new();

        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            obstacles.Add(obstacle);
        }

        return obstacles;
    }

}
