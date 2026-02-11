using UnityEngine;

namespace AI
{
    public class Face : AIMovement
    {
        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            SteeringOutput output = base.GetKinematic(agent);

            // TODO: calculate angular component
			Vector3 direction = agent.TargetPosition - agent.transform.position;

            if (direction.normalized == agent.transform.forward ||
            Mathf.Approximately(direction.magnitude, 0f))
            {
                output.angular = agent.transform.rotation;
            }
            else
            {
                output.angular = Quaternion.LookRotation(direction);
            }

            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            SteeringOutput output = base.GetSteering(agent);

            // TODO: calculate angular component
			Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = GetKinematic(agent).angular * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            output.angular = Quaternion.AngleAxis(angleY, Vector3.up);

            return output;
        }
    }
}
