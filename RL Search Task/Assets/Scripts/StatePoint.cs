using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePoint : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            //meshRenderer.material.color = Color.grey;
            gameObject.tag = "InaccessibleState";
        }
        else if (collision.gameObject.CompareTag("Target"))
        {
            //meshRenderer.material.color = Color.yellow;
            gameObject.tag = "RewardState";
        }
    }

}
