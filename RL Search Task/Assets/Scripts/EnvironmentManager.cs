using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnvironmentManager : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        GameObject env = GameObject.Find("Environment");
        QLearning qLearning = env.GetComponent<QLearning>();
        yield return new WaitUntil(() => qLearning.isInitialised); // Wait until variables from QLearning.cs have been initialised
        // Instantiate(env, new Vector3(env.transform.position.x, env.transform.position.y, env.transform.position.z + 6), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
