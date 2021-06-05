using UnityEngine;

public class PocaFoodLogic : MonoBehaviour
{
    public PocaArea myArea;

    public void OnEaten()
    {
        Destroy(gameObject);
        myArea.GetComponent<PocaArea>().DecrementFood();
    }
}
