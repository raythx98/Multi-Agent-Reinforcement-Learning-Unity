using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

public class FoodCollectorSettings : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] agents;
    [HideInInspector]
    public FoodCollectorArea[] listArea;

    public GameObject observingBlue;
    public GameObject observingRed;
    public int highScore;
    public int attempts;
    public Text highscoreText;
    public Text attemptText;

    StatsRecorder m_Recorder;

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_Recorder = Academy.Instance.StatsRecorder;
    }

    public void IncrementAttempts()
    {
        attempts++;
    }

    void EnvironmentReset()
    {
        ClearObjects(GameObject.FindGameObjectsWithTag("blue"));
        ClearObjects(GameObject.FindGameObjectsWithTag("red"));

        agents = GameObject.FindGameObjectsWithTag("agent");
        listArea = FindObjectsOfType<FoodCollectorArea>();
        foreach (var fa in listArea)
        {
            fa.ResetFoodArea();
        }
        listArea[0].ResetAgents(agents);
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
        
        int blue = observingBlue.GetComponent<FoodCollectorAgent>().GetScore();
        int red = observingRed.GetComponent<FoodCollectorAgent>().GetScore();
        int total = blue + red;
        if (total > highScore)
        {
            highScore = total;
        }
        highscoreText.text = $"Blue score: {blue} \nRed score: {red} \nTotal score: {total} \nHigh score: {highScore}";
        attemptText.text = $"Attempts: {attempts - 7}";

        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.
        /*
        if ((Time.frameCount % 100) == 0)
        {
            m_Recorder.Add("TotalScore", totalScore);
        }
        */
    }
}
