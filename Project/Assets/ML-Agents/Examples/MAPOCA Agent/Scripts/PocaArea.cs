using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgentsExamples;

public class PocaArea : Area
{
    public GameObject blueFood;
    public GameObject redFood;
    public int MaxEnvironmentSteps = 10000;
    public List<PocaAgent> AgentsList = new List<PocaAgent>();
    private int m_ResetTimer;
    private SimpleMultiAgentGroup m_AgentGroup;
    private int blueScore;
    private int redScore;
    public int numBlueFood;
    public int numRedFood;
    public bool respawnFood;
    public float range;
    int remainingFood = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps )
        {
            foreach (var item in AgentsList)
            {
                item.gameObject.SetActive(false);
            }
            m_AgentGroup.GroupEpisodeInterrupted();
            ResetArea();
        } else if (remainingFood <= 0)
        {
            OnCompletion();
            foreach (var item in AgentsList)
            {
                item.gameObject.SetActive(false);
            }
            m_AgentGroup.EndGroupEpisode();
            ResetArea();
        }
    }

    public int[] GetScore()
    {
        return new int[] {blueScore, redScore};
    }

    public void OnCorrectEaten(bool isBlueAgent)
    {
        m_AgentGroup.AddGroupReward(2f);
        remainingFood--;
        if (isBlueAgent)
        {
            blueScore += 2;
        } else
        {
            redScore += 2;
        }
    }

    public void OnIncorrectEaten(bool isBlueAgent)
    {
        m_AgentGroup.AddGroupReward(-1f);
        remainingFood--;
        if (isBlueAgent)
        {
            blueScore++;
        } else
        {
            redScore++;
        }
    }

    public void OnCompletion()
    {
        m_AgentGroup.AddGroupReward(3f);
    }

    void CreateFood(int num, GameObject type)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject f = Instantiate(type, new Vector3(Random.Range(-range, range), 1f,
                Random.Range(-range, range)) + transform.position,
                Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 90f))) as GameObject;
            f.transform.parent = this.transform;
        }
    }

    public void ResetFoodArea()
    {
        foreach (Transform child in this.transform)
        {
            if (child.CompareTag("red"))
                GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in this.transform)
        {
            if (child.CompareTag("blue"))
                GameObject.Destroy(child.gameObject);
        }

        CreateFood(numBlueFood, blueFood);
        CreateFood(numRedFood, redFood);

        remainingFood = 50;
    }

    public void ResetAgents()
    {
        foreach (Agent agent in AgentsList)
        {
            agent.transform.position = new Vector3(Random.Range(-range, range), 2f,
                Random.Range(-range, range))
                + transform.position;
            agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
        }
    }

    public override void ResetArea()
    {
        //Reset Counter
        m_ResetTimer = 0;

        //Reset Score
        blueScore = 0;
        redScore = 0;

        //Reset Area & Food
        ResetFoodArea();

        //Reset Agents
        ResetAgents();

        GameObject.Find("PocaSettings").GetComponent<PocaSettings>().IncrementAttempts();

        m_AgentGroup = new SimpleMultiAgentGroup();
        foreach (var item in AgentsList)
        {
            m_AgentGroup.RegisterAgent(item);
            item.gameObject.SetActive(true);
        }
    }
}
