using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedControlled : MonoBehaviour
{
    public GameObject area;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("blue"))
        {
            collision.gameObject.GetComponent<FoodLogicController>().OnEaten();
            area.GetComponent<ControlAgent>().OnEaten(false, true);
        }
        else if (collision.gameObject.CompareTag("red"))
        {
            collision.gameObject.GetComponent<FoodLogicController>().OnEaten();
            area.GetComponent<ControlAgent>().OnEaten(false, false);
        }
    }
}
