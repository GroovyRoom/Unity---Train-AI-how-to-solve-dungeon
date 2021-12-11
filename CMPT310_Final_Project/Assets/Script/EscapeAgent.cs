using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class EscapeAgent : Agent
{
    private PushBlockSettings m_PushBlockSettings;
    private Rigidbody m_AgentRb;
    private DungeonController m_GameController;
    public int hp;
    public int level;
    public int maxHp = 100;

    public override void Initialize()
    {
        m_GameController = GetComponentInParent<DungeonController>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
        hp = 100;
        level = 1;

    }

    public override void OnEpisodeBegin()
    {
        hp = 100;
        level = 1;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(hp);
        sensor.AddObservation(level);
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * m_PushBlockSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Move the agent using the action.
        MoveAgent(actionBuffers.DiscreteActions);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.CompareTag("lion"))
        {
            m_GameController.fightLion(this);
        }
        if (col.transform.CompareTag("chicken"))
        {
            m_GameController.fightChicken(this);
        }
        if (col.transform.CompareTag("dragon"))
        {
            m_GameController.fightDragon(this);
        }
        if (col.transform.CompareTag("food"))
        {
            m_GameController.EatFood(this);
        }
        if (col.transform.CompareTag("money"))
        {
            m_GameController.TakeMoney(this, col);
        }
    }

    void OnTriggerEnter(Collider col)
    {
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }
}
