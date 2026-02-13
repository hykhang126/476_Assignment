using UnityEngine;

namespace AI
{
    public abstract class AIMovement : MonoBehaviour
    {
        public bool debug;

        public virtual SteeringOutput GetKinematic(AIAgent agent)
        {
            return new SteeringOutput { angular = agent.transform.rotation };
        }

        public virtual SteeringOutput GetSteering(AIAgent agent)
        {
            return new SteeringOutput { angular = Quaternion.identity };
        }

        public virtual void SetTarget(Transform target, AIAgent agent)
        {
            if (agent != null)
            {
                agent.TrackTarget(target);
            }
        }

        public virtual AIState GetState(AIAgent agent)
        {
            return agent.currentState;
        }

        public virtual void SetState(AIState state, AIAgent agent)
        {
            if (agent != null)
            {
                agent.SetState(state);
            }
        }
    }
}
