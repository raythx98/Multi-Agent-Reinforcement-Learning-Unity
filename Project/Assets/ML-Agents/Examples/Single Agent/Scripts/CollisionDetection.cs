using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public GameObject enemy;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit!");
        transform.parent.GetComponent<FoodCollectorAgent>().HitEnemy();
        enemy.GetComponent<FoodCollectorAgent>().Freeze();
    }
}
