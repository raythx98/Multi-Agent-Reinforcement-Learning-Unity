using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueControlled : MonoBehaviour
{
    public GameObject area;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("blue"))
        {
            collision.gameObject.GetComponent<FoodLogicController>().OnEaten();
            area.GetComponent<ControlAgent>().onEaten(true, true);
        }
        else if (collision.gameObject.CompareTag("red"))
        {
            collision.gameObject.GetComponent<FoodLogicController>().OnEaten();
            area.GetComponent<ControlAgent>().onEaten(true, false);
        }
        else {}
    }
}
