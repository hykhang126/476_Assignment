using UnityEngine;

namespace AI
{
    public class Seek : AIMovement
    {
        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            var output = base.GetKinematic(agent);

            // TODO: calculate linear component
            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            output.linear = desiredVelocity.normalized * agent.maxSpeed;

            if (debug) Debug.DrawRay(transform.position, output.linear, Color.cyan);

            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            var output = base.GetSteering(agent);

            // TODO: calculate linear component
            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            output.linear = desiredVelocity.normalized * agent.maxSpeed - agent.Velocity;

            if (debug) Debug.DrawRay(transform.position + agent.Velocity, output.linear, Color.green);

            return output;
        }
    }
}
