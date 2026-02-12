using AI;
using UnityEngine;

public class Cover : MonoBehaviour
{
    [Header("DEBUG: Cover Settings")]
    public CoverTarget[] coverTargets;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindCoverTargets();
    }

    // Update is called once per frame
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
        for (int i = 0; i < coverTargets.Length; i++)
        {
            if (!coverTargets[i].IsOccupied())
            {
                coverTargets[i].occupyingAgent = agent;
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

        for (int i = 0; i < coverTargets.Length; i++)
        {
            coverTargets[i].coverTransform = transform.GetChild(i);
        }
    }

    [System.Serializable]
    public struct CoverTarget
    {
        public Transform coverTransform;
        public AIAgent occupyingAgent;

        public bool IsOccupied()
        {
            return occupyingAgent != null;
        }
    }
}
