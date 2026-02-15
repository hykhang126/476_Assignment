using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    [RequireComponent(typeof(AIAgent))]
    public class Pathfinding : MonoBehaviour
    {
        private AIAgent agent;
        public float degreeOfArrival = 2f;

        [Header("DEBUG:")]
        [SerializeField] private GridGraphNode currentTargetNode;
        [SerializeField] private int targetNodeIndex = 0;

        public void Initialize()
        {
            agent = gameObject.GetComponent<AIAgent>();

            targetNodeIndex = 0;
        }

        void Start()
        {
            Initialize();
        }

        private bool CheckifAroundDestination(Vector3 start, Vector3 end, float degreeOfArrival = 1f)
        {
            // check if the distance between the start and end is less than the degree of arrival
            return Vector3.Distance(start, end) < degreeOfArrival;
        }

        void Update()
        {
            if (agent == null || !agent.usePathFinding) return;

            if (currentTargetNode == null)
            {
                if (agent.currentPath != null && agent.currentPath.Count > 0)
                {
                    currentTargetNode = agent.currentPath[targetNodeIndex];
                }
                else
                {
                    return;
                }
            }

            if (CheckifAroundDestination(agent.transform.position, currentTargetNode.transform.position, degreeOfArrival))
            {
                // get the next target from the path. Don't go over the last target of the path.
                if (agent.currentPath.Count > 0)
                {
                    targetNodeIndex = targetNodeIndex < agent.currentPath.Count - 1 ? targetNodeIndex + 1 : targetNodeIndex;
                    if (agent.currentPath[targetNodeIndex] != null)
                    {
                        currentTargetNode = agent.currentPath[targetNodeIndex];
                    }
                }
            }

            if (agent.currentState == AIState.Moving)
            {
                agent.TrackTarget(currentTargetNode.transform);
            }
        }
    }
}
