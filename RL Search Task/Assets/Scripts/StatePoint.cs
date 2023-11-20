using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePoint : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material.color = Color.grey;
            gameObject.tag = "InaccessibleState";
        }
       
    }

}
