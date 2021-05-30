using UnityEngine;
using Unity.MLAgentsExamples;

public class FoodCollectorArea : Area
{
    public GameObject blueFood;
    public GameObject redFood;
    public GameObject blueAgent;
    public GameObject redAgent;
    public int numBlueFood;
    public int numRedFood;
    public bool respawnFood;
    public float range;
    int remainingFood = 0;

    public void Update()
    {
        if (remainingFood <= 0)
        {
            if (blueAgent.GetComponent<FoodCollectorAgent>().GetScore() > redAgent.GetComponent<FoodCollectorAgent>().GetScore())
            {
                blueAgent.GetComponent<FoodCollectorAgent>().InformallyEndEpisode(true);
                redAgent.GetComponent<FoodCollectorAgent>().InformallyEndEpisode(false);
            } else if (redAgent.GetComponent<FoodCollectorAgent>().GetScore() > blueAgent.GetComponent<FoodCollectorAgent>().GetScore())
            {
                blueAgent.GetComponent<FoodCollectorAgent>().InformallyEndEpisode(false);
                redAgent.GetComponent<FoodCollectorAgent>().InformallyEndEpisode(true);
            } else
            {
                blueAgent.GetComponent<FoodCollectorAgent>().InformallyEndEpisode(false);
                redAgent.GetComponent<FoodCollectorAgent>().InformallyEndEpisode(false);
            }

        }
    }

    public void DecrementFood()
    {
        remainingFood--;
    }

    void CreateFood(int num, GameObject type)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject f = Instantiate(type, new Vector3(Random.Range(-range, range), 1f,
                Random.Range(-range, range)) + transform.position,
                Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 90f))) as GameObject;
            f.transform.parent = this.transform;
            f.GetComponent<FoodLogic>().respawn = respawnFood;
            f.GetComponent<FoodLogic>().myArea = this;
        }
    }

    public void ResetFoodArea(GameObject[] agents)
    {
        foreach (GameObject agent in agents)
        {
            if (agent.transform.parent == gameObject.transform)
            {
                agent.transform.position = new Vector3(Random.Range(-range, range), 2f,
                    Random.Range(-range, range))
                    + transform.position;
                agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            }
        }

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

    public override void ResetArea()
    {
    }
}
