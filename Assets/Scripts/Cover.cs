using System.Collections.Generic;
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

        // Find the closest unoccupied cover target to the agent and assign it to them
        List<CoverTarget> sortedCoverTargets = 
            coverTargets.OrderBy(target => Vector3.Distance(agent.transform.position, target.coverTransform.position)).ToList();
        for (int i = 0; i < sortedCoverTargets.Count; i++)
        {
            if (!sortedCoverTargets[i].IsOccupied())
            {
                int originalIndex = System.Array.IndexOf(coverTargets, sortedCoverTargets[i]);
                coverTargets[originalIndex].occupyingAgent = agent;
                occupyingAgents[originalIndex] = agent;
                coverTransform = coverTargets[originalIndex].coverTransform;
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

    public void RemoveCoverOccupant(AIAgent agent)
    {
        if (agent == null) return;
        // Find the cover target that the agent is occupying and set it to null
        if (occupyingAgents.Contains(agent))
        {
            int index = System.Array.IndexOf(occupyingAgents, agent);
            coverTargets[index].occupyingAgent = null;
            occupyingAgents[index] = null;
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
