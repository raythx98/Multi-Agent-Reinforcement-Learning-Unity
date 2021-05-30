using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class ControlAgent : Agent
{
    public GameObject area;
    public GameObject blueAgent;
    public GameObject redAgent;
    ControlArea m_MyArea;
    bool m_BlueFrozen;
    bool m_BlueShoot;
    float m_BlueFrozenTime;
    bool m_RedFrozen;
    bool m_RedShoot;
    float m_RedFrozenTime;
    Rigidbody m_BlueAgentRb;
    Rigidbody m_RedAgentRb;
    float m_LaserLength;
    int blueScore;
    int redScore;
    int numFoodRemaining;
    // Speed of agent rotation.
    public float turnSpeed = 300;

    // Speed of agent movement.
    public float moveSpeed = 2;
    public Material blueAgentMaterial;
    public Material redAgentMaterial;
    public Material redMaterial;
    public Material blueMaterial;
    public Material frozenMaterial;
    public GameObject blueLaser;
    public GameObject redLaser;
    public bool useVectorObs;
    [Tooltip("Use only the frozen flag in vector observations. If \"Use Vector Obs\" " +
             "is checked, this option has no effect. This option is necessary for the " +
             "VisualFoodCollector scene.")]
    public bool useVectorFrozenFlag;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_BlueAgentRb = blueAgent.GetComponent<Rigidbody>();
        m_RedAgentRb = redAgent.GetComponent<Rigidbody>();
        m_MyArea = area.GetComponent<ControlArea>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        m_BlueFrozen = false;
        m_BlueShoot = false;
        m_RedFrozen = false;
        m_RedShoot = false;
        blueScore = 0;
        redScore = 0;
        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            var blueVelocity = transform.InverseTransformDirection(m_BlueAgentRb.velocity);
            sensor.AddObservation(blueVelocity.x);
            sensor.AddObservation(blueVelocity.z);
            sensor.AddObservation(m_BlueFrozen);
            sensor.AddObservation(m_BlueShoot);
            var redVelocity = transform.InverseTransformDirection(m_RedAgentRb.velocity);
            sensor.AddObservation(redVelocity.x);
            sensor.AddObservation(redVelocity.z);
            sensor.AddObservation(m_RedFrozen);
            sensor.AddObservation(m_RedShoot);
        }
        else if (useVectorFrozenFlag)
        {
            sensor.AddObservation(m_BlueFrozen);
            sensor.AddObservation(m_RedFrozen);
        }
    }

    public int[] GetScore()
    {
        return new int[] {this.blueScore, this.redScore};
    }

    public Color32 ToColor(int hexVal)
    {
        var r = (byte)((hexVal >> 16) & 0xFF);
        var g = (byte)((hexVal >> 8) & 0xFF);
        var b = (byte)(hexVal & 0xFF);
        return new Color32(r, g, b, 255);
    }

    public void MoveAgent(ActionBuffers actionBuffers)
    {
        m_BlueShoot = false;
        m_RedShoot = false;

        if (Time.time > m_BlueFrozenTime + 5f && m_BlueFrozen)
        {
            UnfreezeBlue();
        }
        if (Time.time > m_RedFrozenTime + 5f && m_RedFrozen)
        {
            UnfreezeRed();
        }

        var continuousActions = actionBuffers.ContinuousActions;
        var discreteActions = actionBuffers.DiscreteActions;

        if (!m_BlueFrozen)
        {
            var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
            var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
            var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);

            var dirToGo = blueAgent.transform.forward * forward;
            dirToGo += blueAgent.transform.right * right;
            var rotateDir = -blueAgent.transform.up * rotate;

            var shootCommand = discreteActions[0] > 0;
            if (shootCommand)
            {
                m_BlueShoot = true;
                dirToGo *= 0.5f;
                m_BlueAgentRb.velocity *= 0.75f;
            }
            m_BlueAgentRb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
            blueAgent.transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
        }

        if (!m_RedFrozen)
        {
            var forward = Mathf.Clamp(continuousActions[3], -1f, 1f);
            var right = Mathf.Clamp(continuousActions[4], -1f, 1f);
            var rotate = Mathf.Clamp(continuousActions[5], -1f, 1f);

            var dirToGo = redAgent.transform.forward * forward;
            dirToGo += redAgent.transform.right * right;
            var rotateDir = -redAgent.transform.up * rotate;

            var shootCommand = discreteActions[1] > 0;
            if (shootCommand)
            {
                m_RedShoot = true;
                dirToGo *= 0.5f;
                m_RedAgentRb.velocity *= 0.75f;
            }
            m_RedAgentRb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
            redAgent.transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
        }

        if (m_BlueAgentRb.velocity.sqrMagnitude > 25f) // slow it down
        {
            m_BlueAgentRb.velocity *= 0.95f;
        }

        if (m_RedAgentRb.velocity.sqrMagnitude > 25f) // slow it down
        {
            m_RedAgentRb.velocity *= 0.95f;
        }

        if (m_BlueShoot)
        {
            var myTransform = blueAgent.transform;
            blueLaser.transform.localScale = new Vector3(1f, 1f, m_LaserLength);
            var rayDir = 25.0f * myTransform.forward;
            Debug.DrawRay(myTransform.position, rayDir, Color.red, 0f, true);
            RaycastHit hit;
            if (Physics.SphereCast(blueAgent.transform.position, 2f, rayDir, out hit, 25f))
            {
                if (hit.collider.gameObject.CompareTag("agent"))
                {
                    FreezeBlue();
                }
            }
        }
        else
        {
            blueLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        }

        if (m_RedShoot)
        {
            var myTransform = redAgent.transform;
            redLaser.transform.localScale = new Vector3(1f, 1f, m_LaserLength);
            var rayDir = 25.0f * myTransform.forward;
            Debug.DrawRay(myTransform.position, rayDir, Color.red, 0f, true);
            RaycastHit hit;
            if (Physics.SphereCast(redAgent.transform.position, 2f, rayDir, out hit, 25f))
            {
                if (hit.collider.gameObject.CompareTag("agent"))
                {
                    FreezeRed();
                }
            }
        }
        else
        {
            blueLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    void FreezeBlue()
    {
        blueAgent.tag = "frozenAgent";
        m_BlueFrozen = true;
        m_BlueFrozenTime = Time.time;
        blueAgent.GetComponentInChildren<Renderer>().material = frozenMaterial;
    }

    void FreezeRed()
    {
        redAgent.tag = "frozenAgent";
        m_RedFrozen = true;
        m_RedFrozenTime = Time.time;
        redAgent.GetComponentInChildren<Renderer>().material = frozenMaterial;
    }

    void UnfreezeBlue()
    {
        m_BlueFrozen = false;
        blueAgent.tag = "agent";
        blueAgent.GetComponentInChildren<Renderer>().material = blueAgentMaterial;
    }

    void UnfreezeRed()
    {
        m_RedFrozen = false;
        redAgent.tag = "agent";
        redAgent.GetComponentInChildren<Renderer>().material = redAgentMaterial;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);
        if (numFoodRemaining <= 0)
        {
            AddReward(3f);
            this.EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Random.Range(-1.0f, 1.0f);
        continuousActionsOut[1] = Random.Range(-1.0f, 1.0f);
        continuousActionsOut[2] = Random.Range(-1.0f, 1.0f);
        continuousActionsOut[3] = Random.Range(-1.0f, 1.0f);
        continuousActionsOut[4] = Random.Range(-1.0f, 1.0f);
        continuousActionsOut[5] = Random.Range(-1.0f, 1.0f);
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Random.Range(0, 2);
        discreteActionsOut[1] = Random.Range(0, 2);
    }

    public override void OnEpisodeBegin()
    {
        UnfreezeBlue();
        UnfreezeRed();
        m_BlueShoot = false;
        m_RedShoot = false;
        blueScore = 0;
        redScore = 0;
        m_BlueAgentRb.velocity = Vector3.zero;
        m_RedAgentRb.velocity = Vector3.zero;
        blueLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        redLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        blueAgent.transform.position = new Vector3(Random.Range(-m_MyArea.range, m_MyArea.range),
            2f, Random.Range(-m_MyArea.range, m_MyArea.range))
            + area.transform.position;
        redAgent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
        area.GetComponent<ControlArea>().ResetFoodArea();
        numFoodRemaining = 50;
        GameObject.Find("FoodCollectorSettings").GetComponent<FoodCollectorSettings>().IncrementAttempts();
        SetResetParameters();
    }

    public void onEaten(bool isBlueAgent, bool isBlueFood)
    {
        if (isBlueAgent && isBlueFood) // Double reward for own colour
        {
            AddReward(2f);
            blueScore += 2;
        } else if (!isBlueAgent && !isBlueFood) // Double reward for own colour
        {
            AddReward(2f);
            redScore += 2;
        } else if (isBlueAgent)
        {
            AddReward(1f);
            blueScore++;
        } else
        {
            AddReward(1f);
            redScore++;
        }
        numFoodRemaining--;
    }

    public void SetLaserLengths()
    {
        m_LaserLength = m_ResetParams.GetWithDefault("laser_length", 0.5f);
    }

    public void SetAgentScale()
    {
        float agentScale = m_ResetParams.GetWithDefault("agent_scale", 2f);
        gameObject.transform.localScale = new Vector3(agentScale, agentScale, agentScale);
    }

    public void SetResetParameters()
    {
        SetLaserLengths();
        SetAgentScale();
    }
}
