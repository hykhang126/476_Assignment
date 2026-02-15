using UnityEngine;

namespace AI
{
    public class Arrive : AIMovement
    {
        public float slowRadius;
        public float stopRadius;

        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            var output = base.GetKinematic(agent);

            // TODO: calculate linear component
            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            float distance = desiredVelocity.magnitude;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            if (distance <= stopRadius)
            {
                desiredVelocity = Vector3.zero;
            }
            else if (distance < slowRadius)
            {
                desiredVelocity = desiredVelocity * distance / slowRadius;
            }
            output.linear = desiredVelocity;

            if (debug) Debug.DrawRay(transform.position + agent.Velocity, output.linear, Color.cyan);




            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            var output = base.GetSteering(agent);

            // TODO: calculate linear component
            output.linear = GetKinematic(agent).linear - agent.Velocity;

            return output;
        }
    }
}
