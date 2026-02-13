using UnityEngine;

using AI;
using Utilities;

[RequireComponent(typeof(AIAgent))]
public class Flocking : AIMovement 
{

    [Header("DEBUG: Flock Settings")]
    public float flockFactor = 0.5f;   
    public float neighborRadius = 5;
    public float avoidanceRadius = 3.5f;
    public float cohesionFactor = 2f;
    public float avoidanceFactor = 2f;
    public float seekSpeed = 3f;    // Useless right now

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
        movement = new SteeringOutput();

        Collider[] neighbors = GetNeighborContext();
        Cohesion(neighbors);
        Avoidance(neighbors);
        // Alignment(neighbors);
        // Seek();

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
        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.transform == transform)
                continue;
            
            cohesiveMovement += neighbor.transform.position;
        }

        if (neighbors.Length > 0)
        {
            cohesiveMovement /= neighbors.Length;
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

    // This "force" has each of the agents try to synch their orientation.
    void Alignment(Collider[] neighbors)
    {
        // alignedDirection is equal to the average direction of neighbors 
        // TODO
		Vector3 alignedDirection = Vector3.zero;
        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.transform == transform)
                continue;

            if (neighbor.TryGetComponent<AIAgent>(out var neighborAgent))
            {
                alignedDirection += neighborAgent.Velocity.normalized;
            }
        }

        if (neighbors.Length > 0)
        {
            alignedDirection /= neighbors.Length;
            movement.angular = Quaternion.FromToRotation(transform.forward, alignedDirection);
        }
    }

    // This has each agent try to seek out its current target. It's possible to do this
    // in the FlockMind as well, but this is more of a "distributed" swarm model, so each agent
    // gets to make its decisions locally.
    void Seek()
    {
        // TODO
		
        // Already handled by the AIAgent
    }

    public void Initialize(float neighborRadius, float avoidanceRadius, float cohesionFactor, float avoidanceFactor, float seekSpeed, AIAgent agent = null)
    {
        this.neighborRadius = neighborRadius;
        this.avoidanceRadius = avoidanceRadius;
        this.cohesionFactor = cohesionFactor;
        this.avoidanceFactor = avoidanceFactor;
        this.seekSpeed = seekSpeed;
        if (agent != null)
            this.agent = agent;
    }

    public override void SetTarget(Transform target, AIAgent agent)
    {
        // TODO : set the target for this agent to seek towards
        agent.flockTarget = target;
    }
}
