using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnvironmentManager : MonoBehaviour
{
    public bool isInitialised = false;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        GameObject env = GameObject.Find("Environment");
        QLearning qLearning = env.GetComponent<QLearning>();
        yield return new WaitUntil(() => qLearning.isInitialised); // Wait until variables from QLearning.cs have been initialised
        GameObject envSARSA = Instantiate(env, new Vector3(env.transform.position.x - 6, env.transform.position.y, env.transform.position.z), Quaternion.identity);
        envSARSA.tag = "envSARSA";

        GameObject envHillclimber = Instantiate(env, new Vector3(env.transform.position.x + 6, env.transform.position.y, env.transform.position.z), Quaternion.identity);
        envHillclimber.tag = "envHillclimber";
        envHillclimber.name = "Environment Hillclimber";


        QLearning sarsa = envSARSA.GetComponent<QLearning>();
        yield return new WaitUntil(() => sarsa.isInitialised);
        isInitialised = true;

        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
        foreach(GameObject marker in markers)
        {
            marker.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
