using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePoint : MonoBehaviour
{

    float numCollisions = 0;
    int generation;
    AgentQLearning agentQLearning;
    private void Start()
    {
        GameObject agentQL = GameObject.Find("QLAgent");
        agentQLearning = agentQL.GetComponent<AgentQLearning>();
        
        InvokeRepeating("StateHeat", 2f, 1f);
    }
    private void Update()
    {
        generation = agentQLearning.generation;
    }
    private void OnCollisionEnter(Collision collision)
    {
        // This code is likely not needed
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

        if (collision.gameObject.CompareTag("QLAgent"))
        {
            numCollisions++;
        }
    }

    void StateHeat()
    {
        if (gameObject.tag == "EmptyState")
        {
            float lerpNum = 0;
            if (generation == 0)
            {
                lerpNum = numCollisions / 1;
            }
            else
            {
                lerpNum = numCollisions / generation;
            }

            Color lerpedColor = Color.Lerp(Color.red, Color.white, lerpNum * 4);
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material.color = lerpedColor;
        }
        

    }

}
