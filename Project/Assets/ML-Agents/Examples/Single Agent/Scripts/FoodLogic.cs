using UnityEngine;

public class FoodLogic : MonoBehaviour
{
    public FoodCollectorArea myArea;

    public void OnEaten()
    {
        Destroy(gameObject);
        myArea.GetComponent<FoodCollectorArea>().DecrementFood();
    }
}
