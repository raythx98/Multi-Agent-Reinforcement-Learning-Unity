using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class PocaAgent : Agent
{
    public bool m_isBlue;
    public GameObject area;
    PocaArea m_MyArea;
    bool m_Frozen;
    bool m_Shoot;
    float m_FrozenTime;
    Rigidbody m_AgentRb;
    float m_LaserLength;
    // Speed of agent rotation.
    public float turnSpeed = 300;

    // Speed of agent movement.
    public float moveSpeed = 2;
    public Material normalMaterial;
    public Material redMaterial;
    public Material blueMaterial;
    public Material frozenMaterial;
    public GameObject myLaser;
    public bool useVectorObs;
    [Tooltip("Use only the frozen flag in vector observations. If \"Use Vector Obs\" " +
             "is checked, this option has no effect. This option is necessary for the " +
             "VisualFoodCollector scene.")]
    public bool useVectorFrozenFlag;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_MyArea = area.GetComponent<PocaArea>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        m_Shoot = false;
        m_Frozen = false;
        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            var localVelocity = transform.InverseTransformDirection(m_AgentRb.velocity);
            sensor.AddObservation(localVelocity.x);
            sensor.AddObservation(localVelocity.z);
            sensor.AddObservation(m_isBlue);
        }
        else if (useVectorFrozenFlag)
        {
            sensor.AddObservation(m_isBlue);
        }
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
        myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        m_Shoot = false;

        if (Time.time > m_FrozenTime + 5f && m_Frozen)
        {
            Unfreeze();
        }

        var continuousActions = actionBuffers.ContinuousActions;
        var discreteActions = actionBuffers.DiscreteActions;

        if (!m_Frozen)
        {
            var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
            var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
            var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);

            var dirToGo = transform.forward * forward;
            dirToGo += transform.right * right;
            var rotateDir = -transform.up * rotate;

            var shootCommand = discreteActions[0] > 0;
            if (shootCommand)
            {
                m_Shoot = true;
                dirToGo *= 0.5f;
                m_AgentRb.velocity *= 0.75f;
            }
            m_AgentRb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
            transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
        }

        if (m_AgentRb.velocity.sqrMagnitude > 25f) // slow it down
        {
            m_AgentRb.velocity *= 0.95f;
        }

        if (m_Shoot)
        {
            var myTransform = transform;
            myLaser.transform.localScale = new Vector3(0.8f, 0.8f, m_LaserLength);
            var rayDir = 20.0f * myTransform.forward;
            Debug.DrawRay(myTransform.position, rayDir, Color.red, 0f, true);
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 1f, rayDir, out hit, 20f))
            {
                if (hit.collider.gameObject.CompareTag("agent"))
                {
                    hit.collider.gameObject.GetComponent<PocaAgent>().Freeze();
                }
            }
        }
        else
        {
            myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    public void Freeze()
    {
        gameObject.tag = "frozenAgent";
        m_Frozen = true;
        m_FrozenTime = Time.time;
        gameObject.GetComponentInChildren<Renderer>().material = frozenMaterial;
    }

    void Unfreeze()
    {
        m_Frozen = false;
        gameObject.tag = "agent";
        gameObject.GetComponentInChildren<Renderer>().material = normalMaterial;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Random.Range(-1.0f, 1.0f);
        continuousActionsOut[1] = Random.Range(-1.0f, 1.0f);
        continuousActionsOut[2] = Random.Range(-1.0f, 1.0f);
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Random.Range(0, 2);
    }

    public override void OnEpisodeBegin()
    {
        Unfreeze();
        m_Shoot = false;
        m_AgentRb.velocity = Vector3.zero;
        myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        SetResetParameters();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("blue"))
        {
            if (this.m_isBlue)
            {
                m_MyArea.OnCorrectEaten(true);
                collision.gameObject.GetComponent<PocaFoodLogic>().OnEaten();
            } else
            {
                m_MyArea.OnIncorrectEaten(false);
                collision.gameObject.GetComponent<PocaFoodLogic>().OnEaten();
            }

        } else if (collision.gameObject.CompareTag("red"))
        {
            if (!this.m_isBlue)
            {
                m_MyArea.OnCorrectEaten(false);
                collision.gameObject.GetComponent<PocaFoodLogic>().OnEaten();
            } else
            {
                m_MyArea.OnIncorrectEaten(true);
                collision.gameObject.GetComponent<PocaFoodLogic>().OnEaten();
            }

        }
    }

    public void SetLaserLengths()
    {
        m_LaserLength = m_ResetParams.GetWithDefault("laser_length", 0.4f);
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
