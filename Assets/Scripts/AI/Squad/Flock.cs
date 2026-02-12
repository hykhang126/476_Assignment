using UnityEngine;

using System.Collections.Generic;

public class Flock : MonoBehaviour 
{
    public bool debug;
    public int startingFlockCount = 20;
    public GameObject flockAgentPrefab;
    public float neighborRadius = 5;
    public float avoidanceRadius = 3.5f;
    public float cohesionFactor = 1.5f;
    public float avoidanceFactor = 2f;
    public float seekSpeed = 3f;

    private List<FlockAgent> swarm = new List<FlockAgent>();

    [SerializeField] Transform[] targetsPreset;
    private Queue<Transform> targetList = new Queue<Transform>();

    private void Start()
    {
        GenerateTargets();

        GenerateSwarm();
        SetNewSwarmTarget();
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        //     SetNewSwarmTarget();

        // if (Input.GetKey(KeyCode.Q))
        //     cohesionFactor += Time.deltaTime;
        // else if (Input.GetKey(KeyCode.A))
        //     cohesionFactor -= Time.deltaTime;

        // if (Input.GetKey(KeyCode.W))
        //     avoidanceFactor += Time.deltaTime;
        // else if (Input.GetKey(KeyCode.S))
        //     avoidanceFactor -= Time.deltaTime;

        // if (Input.GetKey(KeyCode.E))
        //     seekSpeed += Time.deltaTime;
        // else if (Input.GetKey(KeyCode.D))
        //     seekSpeed -= Time.deltaTime;

        // // synchronize swarm settings across all agents
        // foreach (FlockAgent agent in swarm)
        // {
        //     agent.debug = debug;
        //     agent.neighborRadius = neighborRadius;
        //     agent.avoidanceRadius = avoidanceRadius;
        //     agent.cohesionFactor = cohesionFactor;
        //     agent.avoidanceFactor = avoidanceFactor;
        //     agent.seekSpeed = seekSpeed;
        // }
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

    private void SetNewSwarmTarget()
    {
        // Managing the Queue. We grab the target, set the swarmlings on it, then requeue it at the back.
        Transform target = targetList.Dequeue();
        foreach (FlockAgent agent in swarm)
        {
            agent.SetTarget(target);
        }
        targetList.Enqueue(target);
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
                .TryGetComponent<FlockAgent>(out var agent))
            {
                swarm.Add(agent);
            }
        }
    }

    public void AddTarget(Transform target)
    {
        targetList.Enqueue(target);
    }
}
