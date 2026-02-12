using UnityEngine;

namespace AI
{
    public class Wander : AIMovement
    {
        public float wanderDegreesDelta = 45;
        [Min(0)] public float wanderInterval = 0.75f;
        protected float wanderTimer = 0;

        private Vector3 lastWanderDirection;

        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            var output = base.GetKinematic(agent);
            wanderTimer += Time.deltaTime;

            Vector3 desiredVelocity = output.linear;
            // TODO: calculate linear component


            output.linear = desiredVelocity;
			
			if (debug) Debug.DrawRay(transform.position, output.linear, Color.cyan);
			
            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            var output = base.GetSteering(agent);

            // TODO: calculate linear component
            

            if (debug) Debug.DrawRay(transform.position + agent.Velocity, output.linear, Color.green);

            return output;
        }
    }
}
