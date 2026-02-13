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
        [SerializeField] float rayLength = 5.0f;
        [SerializeField] float rayAngle = 15.0f;

        private AIAgent agent;

        void Start()
        {
            if (grabFromAIAgent && gameObject.GetComponent<AIAgent>())
            {
                agent = gameObject.GetComponent<AIAgent>();
                rayLength = agent.raylength;
                rayAngle = agent.rayAngle;
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
                Debug.DrawRay(transform.position, agent.TargetPosition - transform.position - Vector3.up * 0.5f, Color.blue);
            }
            if (visualizeAvoidance && agent != null && agent.avoidanceDirection != null)
            {
                // draw a yellow ray from the agent in the direction of its avoidance direction, with a length equal to the avoidance direction magnitude
                Debug.DrawRay(transform.position, agent.avoidanceDirection, Color.yellow);
            }
        }

        private void VisualizeCollisionRays()
        {
            // draw a v-shaped ray in the forward direction of the agent, with a small offset in the x-axis
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            Vector3 direction = transform.forward * rayLength;
            // tilt the ray direction to the left and right by 15 degrees
            Vector3 leftDirection = Quaternion.Euler(0, -rayAngle, 0) * direction;
            Vector3 rightDirection = Quaternion.Euler(0, 15, 0) * direction;
            Debug.DrawRay(origin, leftDirection, Color.green);
            Debug.DrawRay(origin, rightDirection, Color.green); 
        }
    }

}