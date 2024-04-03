using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePoint : MonoBehaviour
{

    float numCollisions = 0;
    int generation;
    AgentQLearning agentQLearning;
    AgentSARSA agentSARSA;
    AgentHillclimber agentHillclimber;
    private void Start()
    {
        GameObject agentQL = GameObject.Find("QLAgent");
        agentQLearning = agentQL.GetComponent<AgentQLearning>();

        GameObject agentSarsa = GameObject.Find("SARSAAgent");
        agentSARSA = agentSarsa.GetComponent<AgentSARSA>();

        GameObject agentHC = GameObject.Find("HillclimberAgent");
        agentHillclimber = agentHC.GetComponent<AgentHillclimber>();
        InvokeRepeating("StateHeat", 2f, 0.5f);
    }
    private void Update()
    {

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
            if (transform.parent.tag == "envQL")
            {
                generation = agentQLearning.generation;
            }
            else if (transform.parent.tag == "envSARSA")
            {
                generation = agentSARSA.generation;
            }
            else if (transform.parent.tag == "envHillclimber")
            {
                generation = agentHillclimber.generation;
            }
            float lerpNum = 0;
            if (generation == 0)
            {
                lerpNum = numCollisions / 1;
            }
            else
            {
                lerpNum = numCollisions / generation;
            }

            Color lerpedColor = Color.Lerp(new Color(236f/255f, 236f/255f, 236f/255f), Color.red, lerpNum * 4);
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material.color = lerpedColor;
        }
        

    }

}
