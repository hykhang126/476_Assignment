using UnityEngine;

using System.Collections.Generic;
using AI;

public class Flock : MonoBehaviour 
{
    [Header("Initialization Settings")]
    public bool startOnAwake = false;
    public bool hasInitialized = false;

    [Header("Flock Agent Settings")]
    public int startingFlockCount = 20;
    public GameObject flockAgentPrefab;
    public float neighborRadius = 5;
    public float avoidanceRadius = 3.5f;
    public float cohesionFactor = 1.5f;
    public float avoidanceFactor = 2f;

    [Header("Flock Targets")]
    public Transform currentTarget;
    public float targetReachThreshold = 5f;
    public Transform[] targetsPreset;

    public List<Flocking> swarm = new();
    public Queue<Transform> targetList = new();

    [Header("Listen Events")]
    public GenericEvent onFlockReleased;

    public void OnEnable()
    {
        onFlockReleased.onEventRaised.AddListener(Initialize);
    }

    public void OnDisable()
    {
        onFlockReleased.onEventRaised.RemoveListener(Initialize);
    }

    public void Initialize() 
    {
        if (hasInitialized) return;
        else hasInitialized = true; 
        GenerateTargets();
        GenerateSwarm();
        SetNewSwarmTarget();
        StartCoroutine(ChangeFlockTarget(8f));


        onFlockReleased.onEventRaised.RemoveListener(Initialize);
    }

    private void Start()
    {
        if (startOnAwake)
        {
            Initialize();
        }
    }

    private void Update()
    {
        if (swarm.Count == 0 || !hasInitialized) return;

        // Check if the target has been reached by the swarm (using the first agent as a reference)
        CheckIfTargetReached();
    }

    private void GenerateTargets()
    {
        if (targetsPreset.Length > 0)
        {
            foreach (Transform target in targetsPreset)
                targetList.Enqueue(target);
        }
        else
        {
            foreach (GameObject targetObj in GameObject.FindGameObjectsWithTag("Target"))
                targetList.Enqueue(targetObj.transform);
        }
    }

    [ContextMenu("Set New Swarm Target")]
    public void SetNewSwarmTarget()
    {
        if (!hasInitialized || targetList.Count < 1) return;
        Transform target = targetList.Dequeue();
        SetNewSwarmTarget(target);
    }

    private void SetNewSwarmTarget(Transform newTarget)
    {
        currentTarget = newTarget;
        foreach (Flocking agent in swarm)
        {
            agent.SetTarget(newTarget);
        }
    }

    private void GenerateSwarm()
    {
        swarm.Clear();
        const float AGENT_DENSITY = 2f;
        for (int i = 0; i < startingFlockCount; ++i)
        {
            // Generate a random position for the flock member within a UnitSphere, scaled by the density and count of the flock, and flattened on the y axis.
            Vector3 randomPos = Vector3.Scale(AGENT_DENSITY * startingFlockCount * Random.insideUnitSphere, new Vector3(1, 0, 1));
            
            if (Instantiate( flockAgentPrefab, 
                transform.position + randomPos, 
                Quaternion.identity, 
                transform)
                .TryGetComponent<Flocking>(out var flockAgent))
            {
                string agentName = $"Flock Agent {i}";
                flockAgent.Initialize(agentName, neighborRadius, avoidanceRadius, cohesionFactor, avoidanceFactor);
                swarm.Add(flockAgent);
                // register events
                if (flockAgent.AIAgent != null)
                    flockAgent.AIAgent.agentDiedEvent.AddListener(HandleAgentDeath);
            }
        }
    }

    public void AddTarget(Transform target)
    {
        targetList.Enqueue(target);
    }

    public void OnFlockMove()
    {
        // TODO: handle any necessary logic when the flock moves, such as checking if the target has been reached, updating the target for each agent, etc.
    }

    public void CheckIfTargetReached()
    {
        if (swarm.Count == 0)
            return;

        // Check the distance for every agent to the target, and if they're all within a certain threshold, we can say the target has been reached.
        foreach (Flocking agent in swarm)
        {
            if (Vector3.Distance(agent.transform.position, currentTarget.position) > targetReachThreshold)
                return;
        }
        SetNewSwarmTarget();
    }

    public void HandleAgentDeath(AIAgent agent)
    {
        // Remove the dead agent from the swarm list
        Flocking agentToRemove = swarm.Find(a => a.GetComponent<AIAgent>() == agent);
        if (agentToRemove != null)
        {
            swarm.Remove(agentToRemove);
            GameManager.Instance.Announce($"{agent.name} has died. {swarm.Count} remaining in the flock.");
            Destroy(agentToRemove.gameObject);
        }
    }

    private System.Collections.IEnumerator ChangeFlockTarget(float delayBetweenChanges = 8f)
    {
        while (swarm.Count > 0)
        {
            yield return new WaitForSeconds(delayBetweenChanges);
            SetNewSwarmTarget();
        }
    }
}
