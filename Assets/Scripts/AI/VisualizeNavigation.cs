using UnityEngine;
using AI;

namespace Utilities
{
    // execute in edit mode so that the visualization is visible in the Scene view when the game is not running
    [ExecuteInEditMode]
    public class VisualizeNavigation : MonoBehaviour
    {
        [SerializeField] bool visualizeNavigationRays = true;
        [SerializeField] bool visualizeVelocity = true;
        [SerializeField] bool visualizeTarget = true;
        [SerializeField] bool visualizeAvoidance = true;
        [SerializeField] bool grabFromAIAgent = true;
        [SerializeField] bool visualizeCoverSphere = true;

        private AIAgent agent;

        void Start()
        {
            if (grabFromAIAgent && gameObject.GetComponent<AIAgent>())
            {
                agent = gameObject.GetComponent<AIAgent>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (visualizeNavigationRays && agent != null)
            {
                VisualizeCollisionRays();
            }
            if (visualizeVelocity && agent != null)
            {
                // draw a cyan ray from the agent in the direction of its velocity, with a length equal to the velocity magnitude
                Debug.DrawRay(transform.position, agent.Velocity, Color.red);
            }
            if (visualizeTarget && agent != null && agent.TargetPosition != null)
            {
                // draw a blue ray from the agent to its current target
                Debug.DrawRay(transform.position, agent.TargetPosition - transform.position - Vector3.up * 1f, Color.blue);
            }
            if (visualizeAvoidance && agent != null && agent.avoidanceDirection != null)
            {
                // draw a yellow ray from the agent in the direction of its avoidance direction, with a length equal to the avoidance direction magnitude
                Debug.DrawRay(transform.position, agent.avoidanceDirection * agent.avoidanceForce, Color.yellow);
            }
            if (visualizeCoverSphere && agent != null)
            {
                float rayLengthMod = agent.currentState == AIState.InDanger ? agent.inDanderRayRangeMod : 1f;
                // draw a wire sphere around the agent with a radius equal to the agent's in danger ray range
                DebugUtil.DrawWireSphere(transform.position, Color.red, agent.raylength * rayLengthMod);
            }
        }

        private void VisualizeCollisionRays()
        {
            float rayLengthMod = 1f;
            if (agent != null)
            {
                rayLengthMod = agent.currentState == AIState.InDanger ? agent.inDanderRayRangeMod : rayLengthMod;
            }

            // draw a v-shaped ray in the forward direction of the agent, with a small offset in the x-axis
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            Vector3 direction = agent.raylength * rayLengthMod * transform.forward;
            // tilt the ray direction to the left and right by 15 degrees
            Vector3 leftDirection = Quaternion.Euler(0, -agent.rayAngle, 0) * direction;
            Vector3 rightDirection = Quaternion.Euler(0, agent.rayAngle, 0) * direction;
            Debug.DrawRay(origin, leftDirection, Color.green);
            Debug.DrawRay(origin, rightDirection, Color.green); 
        }
    }

}