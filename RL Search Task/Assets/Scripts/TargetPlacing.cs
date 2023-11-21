using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPlacing : MonoBehaviour
{
    TargetCollision targetCollision;
    // Start is called before the first frame update
    void Start()
    {
        int numTargets = 30;

        GameObject target = GameObject.Find("Target");

        PlaceTargets(numTargets, target);

    }

    List<GameObject> PlaceTargets(int numTargets, GameObject target)
    {

        List<GameObject> targets = new ();
        
        for (int i = 0; i < numTargets; i++)
        {
            
            Vector3 randomPosition = new(Random.Range(-2.7f, 2.7f), 0.01f, Random.Range(-2.7f, 2.7f));
            targets.Add(Instantiate(target, randomPosition, Quaternion.identity));

            targetCollision = targets[i].GetComponent<TargetCollision>();
            bool badPosition = targetCollision.badPosition;

            Debug.Log("badPosition = " + badPosition);


            while (badPosition)
            {
                Debug.Log("Target has instantiated in bad position, retrying...");
                randomPosition = new(Random.Range(-2.7f, 2.7f), 0.01f, Random.Range(-2.7f, 2.7f));
                targets[i].transform.position = randomPosition;

            }
        }
        Debug.Log("All targets placed successfully");
        return targets;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
