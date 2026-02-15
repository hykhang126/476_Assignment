using UnityEngine;

using AI;
using Utilities;
using UnityEngine.AI;

[RequireComponent(typeof(AIAgent))]
public class Flocking : AIMovement 
{

    [Header("DEBUG: Flock Settings")]
    public float flockFactor = 0.5f;   
    public float neighborRadius = 5;
    public float avoidanceRadius = 3.5f;
    public float cohesionFactor = 2f;
    public float avoidanceFactor = 2f;

    private SteeringOutput movement;
    private AIAgent agent;
    public AIAgent AIAgent => agent;
    [SerializeField] private Collider[] neighborBuffer = new Collider[50];
	
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<AIAgent>();
    }

	public override SteeringOutput GetSteering(AIAgent agent) 
    {
        // reset movement
        movement = base.GetSteering(agent);

        Collider[] neighbors = GetNeighborContext();
        Cohesion(neighbors);
        Avoidance(neighbors);

        // Movement is the combination of all the forces, but is capped at the agent's max speed * flock factor. 
        // The orientation is handled by the LookWhereYouAreGoing movement.
        movement.linear = Vector3.ClampMagnitude(movement.linear, agent.maxSpeed * flockFactor);

        // TODO : steer and align the boid in the direction of the movement
        if (debug) 
        {
            Debug.DrawRay(transform.position + agent.Velocity, movement.linear, Color.magenta);
        }

		return movement;
    }

    public Collider[] GetNeighborContext()
    {
        int agentLayerMask = LayerMask.GetMask("Agent");
        int neighborCount = Physics.OverlapSphereNonAlloc(transform.position, neighborRadius, neighborBuffer, agentLayerMask);

        if (debug)
            DebugUtil.DrawWireSphere(transform.position, Color.Lerp(Color.white, Color.red, neighborCount), neighborRadius);

        System.Array.Resize(ref neighborBuffer, neighborCount);
        return neighborBuffer;
    }

    // This is the force that keeps the swarm together.
    void Cohesion(Collider[] neighbors)
    {
        // movement is equal to the relative offset from the flock agent to the center of the flock
        // TODO
		Vector3 cohesiveMovement = Vector3.zero;
        int count = 0;
        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.transform == transform)
                continue;
            
            cohesiveMovement += neighbor.transform.position;
            count++;
        }

        if (count > 0)
        {
            cohesiveMovement /= count;
            cohesiveMovement -= transform.position;
        }

        movement.linear += cohesionFactor * cohesiveMovement.normalized;
    }

    // This is the force that dictates the spacing of the swarm.
    void Avoidance(Collider[] neighbors)
    {
        // movement is equal to the average of the sum of all vectors going from neighbor to flock agent within the avoidance radius
        // TODO
		Vector3 avoidanceMovement = Vector3.zero;
        float avoidanceRadiusSqr = avoidanceRadius * avoidanceRadius;
        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.transform == transform)
                continue;

            Vector3 toNeighbor = neighbor.transform.position - transform.position;
            if (toNeighbor.sqrMagnitude < avoidanceRadiusSqr)
            {
                Vector3 neighborAvoidance = transform.position - neighbor.transform.position;
                if (neighborAvoidance == Vector3.zero)
                    neighborAvoidance = Random.insideUnitSphere * 0.1f;
                
                avoidanceMovement += neighborAvoidance;
            }
        }

        if (neighbors.Length > 0)
        {
            avoidanceMovement = avoidanceMovement.normalized / neighbors.Length;
        }

        movement.linear += avoidanceFactor * avoidanceMovement;
    }

    public void Initialize( string name, float neighborRadius, float avoidanceRadius, float cohesionFactor, float avoidanceFactor, AIAgent agent = null)
    {
        this.name = name;
        this.neighborRadius = neighborRadius;
        this.avoidanceRadius = avoidanceRadius;
        this.cohesionFactor = cohesionFactor;
        this.avoidanceFactor = avoidanceFactor;
        if (agent != null)
            this.agent = agent;
        else
        {
            this.agent = GetComponent<AIAgent>();
        }
        if (agent != null)
        {
            agent.Initialize();
        }
    }

    public override void SetTarget(Transform target, AIAgent agent)
    {
        // TODO : set the target for this agent to seek towards
        agent.flockTarget = target;
    }
}
