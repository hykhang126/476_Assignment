using UnityEngine;

using AI;
using Utilities;

public class FlockAgent : AIMovement 
{

    [Header("DEBUG: Flock Settings")]
    public float neighborRadius = 5;
    public float avoidanceRadius = 3.5f;
    public float cohesionFactor = 1.5f;
    public float avoidanceFactor = 2f;
    public float seekSpeed = 3f;

    private Vector3 movement;
	
	public override SteeringOutput GetSteering(AIAgent agent) 
    {
        SteeringOutput output = new SteeringOutput();
        Collider[] neighbors = GetNeighborContext();
        Cohesion(neighbors);
        Avoidance(neighbors);
        Alignment(neighbors);
        Seek();

        // TODO : steer and align the boid in the direction of the movement
		return output;
    }

    public Collider[] GetNeighborContext()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, neighborRadius);

        if (debug)
            DebugUtil.DrawWireSphere(transform.position, Color.Lerp(Color.white, Color.red, neighbors.Length), neighborRadius);

        return neighbors;
    }

    // This is the force that keeps the swarm together.
    void Cohesion(Collider[] neighbors)
    {
        // movement is equal to the relative offset from the flock agent to the center of the flock
        // TODO
		
    }

    // This is the force that dictates the spacing of the swarm.
    void Avoidance(Collider[] neighbors)
    {
        // movement is equal to the average of the sum of all vectors going from neighbor to flock agent within the avoidance radius
        // TODO
		
    }

    // This "force" has each of the agents try to synch their orientation.
    void Alignment(Collider[] neighbors)
    {
        // alignedDirection is equal to the average direction of neighbors 
        // TODO
		
    }

    // This has each agent try to seek out its current target. It's possible to do this
    // in the FlockMind as well, but this is more of a "distributed" swarm model, so each agent
    // gets to make its decisions locally.
    void Seek()
    {
        // TODO
		
    }

    public void SetTarget(Transform target)
    {
        // TODO : set the target for this agent to seek towards
    }
}
