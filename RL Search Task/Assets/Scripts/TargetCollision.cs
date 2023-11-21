using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollision : MonoBehaviour
{
    public bool badPosition;

    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("InaccessibleState")) // if collision with obstacle or inaccessible state
        {
            badPosition = true;
            Debug.Log("Collision");
        }


    }
    private void OnCollisionExit(Collision collision)
    {
        badPosition = false;
        Debug.Log("No more collision");
    }

}
