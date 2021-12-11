using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class DungeonController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public EscapeAgent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
        [HideInInspector]
        public Collider Col;
    }

    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;
    private int m_ResetTimer;

    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    Material m_GroundMaterial; //cached on Awake()

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;

    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();
    private Dictionary<PushAgentEscape, PlayerInfo> m_PlayerDict = new Dictionary<PushAgentEscape, PlayerInfo>();
    public bool UseRandomAgentRotation = true;
    public bool UseRandomAgentPosition = false;
    PushBlockSettings m_PushBlockSettings;

    public GameObject TombstoneChicken;
    public GameObject TombstoneLion;
    public GameObject TombstoneDragon;
    public GameObject TombstoneAgent;
    public GameObject lion;
    public GameObject chicken;
    public GameObject food;
    public GameObject dragon;
    public GameObject money1;
    public GameObject money2;
    private SimpleMultiAgentGroup m_AgentGroup;
    void Start()
    {
        // Get the ground renderer so we can change the material when a goal is scored
        m_GroundRenderer = ground.GetComponent<Renderer>();
        // Starting material
        m_GroundMaterial = m_GroundRenderer.material;
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();

        // Initialize TeamManager
        m_AgentGroup = new SimpleMultiAgentGroup();
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            item.Col = item.Agent.GetComponent<Collider>();
            // Add to team manager
            m_AgentGroup.RegisterAgent(item.Agent);
        }

        ResetScene();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_AgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public void AgentKilled(EscapeAgent agent)
    {
        m_AgentGroup.EndGroupEpisode();
        agent.gameObject.SetActive(false);
        TombstoneAgent.transform.SetPositionAndRotation(agent.transform.position, agent.transform.rotation);
        TombstoneAgent.SetActive(true);
        lose();
        ResetScene();
    }

    public void AgentDamaged(EscapeAgent agent, int damage)
    {
        int newHp = agent.hp - damage;
        if (newHp > 0)
        {
            agent.hp = newHp;
        }
        else
        {
            agent.hp = 0;
        }
    }

    public void AgentHealed(EscapeAgent agent, int heal)
    {
        int newHp = agent.hp + heal;
        if (newHp <= 100)
        {
            agent.hp = newHp;
        } else
        {
            agent.hp = 100;
        }
    }

    public void EatFood(EscapeAgent agent)
    {
        food.SetActive(false);
        AgentHealed(agent, 100);
    }
    public void TakeMoney(EscapeAgent agen, Collision moneyCol)
    {
        moneyCol.gameObject.SetActive(false);
        m_AgentGroup.AddGroupReward(1f);
    }
    public void fightChicken(EscapeAgent agent)
    {
        AgentDamaged(agent, 25);
        if (agent.hp == 0)
        {
            AgentKilled(agent);
        } else
        {
            chicken.gameObject.SetActive(false);
            TombstoneChicken.transform.SetPositionAndRotation(chicken.transform.position, chicken.transform.rotation);
            TombstoneChicken.SetActive(true);
            agent.level++;
        }
    }
    public void fightLion(EscapeAgent agent)
    {
        AgentDamaged(agent, 25);
        if (agent.hp == 0)
        {
            AgentKilled(agent);
        }
        else
        {
            lion.gameObject.SetActive(false);
            TombstoneLion.transform.SetPositionAndRotation(lion.transform.position, lion.transform.rotation);
            TombstoneLion.SetActive(true);
            agent.level++;
        }
    }
    public void fightDragon(EscapeAgent agent)
    {
        if (agent.level == 3)
        {
            AgentDamaged(agent, 50);
            if (agent.hp == 0)
            {
                AgentKilled(agent);
            }
            else
            {
                dragon.gameObject.SetActive(false);
                TombstoneDragon.transform.SetPositionAndRotation(dragon.transform.position, dragon.transform.rotation);
                TombstoneDragon.SetActive(true);
                m_AgentGroup.AddGroupReward(5f);
                win();
                m_AgentGroup.EndGroupEpisode();
                ResetScene();
            }
        } else
        {
            AgentDamaged(agent, 100);
            AgentKilled(agent);
        }
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        m_GroundRenderer.material = m_GroundMaterial;
    }

    public void lose()
    {
        StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.failMaterial, 0.5f));
    }

    public void win()
    {
        StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.goalScoredMaterial, 0.5f));
    }

    Quaternion GetRandomRot()
    {
        return Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }

    void ResetScene()
    {

        //Reset counter
        m_ResetTimer = 0;

        //Random platform rot
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        //Reset Agents
        foreach (var item in AgentsList)
        {
            //var pos = UseRandomAgentPosition ? GetRandomSpawnPos() : item.StartingPos;
            var rot = UseRandomAgentRotation ? GetRandomRot() : item.StartingRot;
            item.Agent.transform.SetPositionAndRotation(new Vector3(0.0f, 0.5f, 0.0f), rot);
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
            item.Agent.hp = 100;
            item.Agent.level = 1;
            item.Agent.gameObject.SetActive(true);
            m_AgentGroup.RegisterAgent(item.Agent);
        }

        //Reset Tombstone
        TombstoneChicken.SetActive(false);
        TombstoneDragon.SetActive(false);
        TombstoneLion.SetActive(false);
        TombstoneAgent.SetActive(false);

        //Reset other things
        chicken.SetActive(true);
        dragon.SetActive(true);
        lion.SetActive(true);
        food.SetActive(true);
        money1.SetActive(true);
        money2.SetActive(true);
    }
}
