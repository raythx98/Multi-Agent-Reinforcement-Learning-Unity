using UnityEngine;

public class PocaFoodLogic : MonoBehaviour
{
    public void OnEaten()
    {
        Destroy(gameObject);
    }
}
