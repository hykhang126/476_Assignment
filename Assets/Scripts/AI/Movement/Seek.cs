using UnityEngine;

namespace AI
{
    public class Seek : AIMovement
    {
        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            SteeringOutput output = base.GetKinematic(agent);

            // TODO: calculate linear component
            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            output.linear = desiredVelocity.normalized * agent.maxSpeed;

            if (debug) Debug.DrawRay(transform.position, output.linear, Color.cyan);

            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            SteeringOutput output = base.GetSteering(agent);

            // TODO: calculate linear component
            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            Vector3 steering = desiredVelocity - agent.Velocity;
            
            output.linear = steering;

            if (debug) Debug.DrawRay(transform.position + agent.Velocity, output.linear, Color.green);

            return output;
        }
    }
}
