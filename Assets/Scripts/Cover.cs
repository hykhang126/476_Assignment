using System.Linq;
using AI;
using UnityEngine;

public class Cover : MonoBehaviour
{
    [Header("DEBUG: Cover Settings")]
    public CoverTarget[] coverTargets;
    public AIAgent[] occupyingAgents;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindCoverTargets();
    }

    public bool IsCoverAvailable()
    {
        foreach (CoverTarget target in coverTargets)
        {
            if (!target.IsOccupied())
                return true;
        }
        return false;
    }

    public bool TryOccupyCover(AIAgent agent, out Transform coverTransform)
    {
        // Check if agent is already occupying a cover, then don't give this cover to them
        if (occupyingAgents.Contains(agent))
        {
            coverTransform = null;
            return false;
        }

        for (int i = 0; i < coverTargets.Length; i++)
        {
            if (!coverTargets[i].IsOccupied())
            {
                // Update Cover Targets and Occupying Agents list
                coverTargets[i].occupyingAgent = agent;
                occupyingAgents[i] = agent;

                // Return the free cover transform to the caller
                coverTransform = coverTargets[i].coverTransform;
                return true;
            }
        }
        coverTransform = null;
        return false;
    }

    [ContextMenu("Find Cover Targets")]
    void FindCoverTargets()
    {
        int numberOfChildren = transform.childCount;
        coverTargets = new CoverTarget[numberOfChildren];
        occupyingAgents = new AIAgent[numberOfChildren];

        for (int i = 0; i < coverTargets.Length; i++)
        {
            coverTargets[i].coverTransform = transform.GetChild(i);
        }
    }

    public static void RemoveCoverOccupant(AIAgent agent, Cover cover)
    {
        // Find the cover target that the agent is occupying and set it to null
        if (cover.occupyingAgents.Contains(agent))
        {
            int index = System.Array.IndexOf(cover.occupyingAgents, agent);
            cover.coverTargets[index].occupyingAgent = null;
            cover.occupyingAgents[index] = null;
        }
    }

    [System.Serializable]
    public struct CoverTarget
    {
        public Transform coverTransform;
        public AIAgent occupyingAgent;

        public readonly bool IsOccupied()
        {
            return occupyingAgent != null;
        }
    }
}
