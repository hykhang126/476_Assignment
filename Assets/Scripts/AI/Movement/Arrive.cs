using UnityEngine;
using Utilities;

namespace AI
{
    public class Arrive : AIMovement
    {
        public float slowRadius;
        public float stopRadius;

        private void DrawDebug(AIAgent agent)
        {
            if (debug)
            {
                DebugUtil.DrawCircle(agent.TargetPosition, transform.up, Color.yellow, stopRadius);
                DebugUtil.DrawCircle(agent.TargetPosition, transform.up, Color.magenta, slowRadius);
            }
        }

        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            DrawDebug(agent);

            var output = base.GetKinematic(agent);

            // TODO: calculate linear component
			
			
            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            DrawDebug(agent);

            var output = base.GetSteering(agent);

            // TODO: calculate linear component

            return output;
        }
    }
}
