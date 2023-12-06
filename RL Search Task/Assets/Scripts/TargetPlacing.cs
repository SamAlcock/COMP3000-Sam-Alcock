using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TargetPlacing : MonoBehaviour
{
    TargetCollision targetCollision;

    public bool targetsPlaced;
    // Start is called before the first frame update
    void Start()
    {
        int numTargets = 15;

        GameObject target = GameObject.Find("Target");

        PlaceTargets(numTargets, target);

    }

    List<GameObject> PlaceTargets(int numTargets, GameObject target)
    {

        List<GameObject> targets = new ();
        
        for (int i = 0; i < numTargets; i++)
        {
           
            Vector3 randomPosition = GetPosition();
            targets.Add(Instantiate(target, randomPosition, Quaternion.identity));
            targets[i].tag = "Target";

            while (InBadPosition(randomPosition))
            {
                randomPosition = GetPosition();
            }
            targets[i].transform.position = randomPosition;
        }
        Debug.Log("All targets placed successfully");
        targetsPlaced = true;
        return targets;
    }
    bool InBadPosition(Vector3 position)
    {
        Collider[] collisions = Physics.OverlapSphere(position, 0.2f);
        foreach(Collider collision in collisions)
        {
            if (collision.CompareTag("Obstacle") || collision.CompareTag("InaccessibleState"))
            {
                return true;
            }
        }
        return false;
    }

    Vector3 GetPosition()
    {
        Vector3 position = new((float)Math.Round(UnityEngine.Random.Range(-2.7f, 2.7f) / 0.3f) * 0.3f, 0.04f, (float)Math.Round(UnityEngine.Random.Range(-2.7f, 2.7f) / 0.3f) * 0.3f);

        return position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
