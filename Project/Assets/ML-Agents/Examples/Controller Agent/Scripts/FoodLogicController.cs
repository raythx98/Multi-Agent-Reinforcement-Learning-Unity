using UnityEngine;

public class FoodLogicController : MonoBehaviour
{
    public void OnEaten()
    {
        Destroy(gameObject);
    }
}
