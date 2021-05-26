using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

public class FoodCollectorSettings : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] blueAgents;
    [HideInInspector]
    public GameObject[] redAgents;
    [HideInInspector]
    public FoodCollectorArea[] listArea;

    public int totalScore;
    public Text scoreText;

    StatsRecorder m_Recorder;

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_Recorder = Academy.Instance.StatsRecorder;
    }

    void EnvironmentReset()
    {
        ClearObjects(GameObject.FindGameObjectsWithTag("blue"));
        ClearObjects(GameObject.FindGameObjectsWithTag("red"));

        blueAgents = GameObject.FindGameObjectsWithTag("blueAgent");
        redAgents = GameObject.FindGameObjectsWithTag("blueAgent");
        listArea = FindObjectsOfType<FoodCollectorArea>();
        foreach (var fa in listArea)
        {
            fa.ResetFoodArea(blueAgents);
            fa.ResetFoodArea(redAgents);
        }

        totalScore = 0;
    }

    void ClearObjects(GameObject[] objects)
    {
        foreach (var food in objects)
        {
            Destroy(food);
        }
    }

    public void Update()
    {
        scoreText.text = $"Score: {totalScore}";

        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.
        if ((Time.frameCount % 100) == 0)
        {
            m_Recorder.Add("TotalScore", totalScore);
        }
    }
}
