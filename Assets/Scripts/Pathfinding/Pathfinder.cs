using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

[RequireComponent(typeof(GridGraph))]
public class Pathfinder : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool debug;
    public GameObject openPointPrefab;
    public GameObject closedPointPrefab;
    public GameObject pathPointPrefab;

    [Header("Pathfinder Settings")]
    [SerializeField] private GridGraph graph;
    public GridGraphNode startNode;
    public GridGraphNode goalNode;

    [Header("Listen Events")]
    public GenericEvent onGridGenerated;

    public delegate float Heuristic(Transform start, Transform end);

    #region Unity

    public void Initialize()
    {
        if (graph == null)
        {
            graph = GetComponent<GridGraph>();
        }
        if (startNode == null)
        {
            startNode = graph.nodes[0];
        }
        if (goalNode == null)
        {
            goalNode = graph.nodes[^1];
        }
        onGridGenerated.onEventRaised.RemoveListener(Initialize);
    }

    public void OnEnable()
    {
        onGridGenerated.onEventRaised.AddListener(Initialize);
    }

    public void OnDisable()
    {
        onGridGenerated.onEventRaised.RemoveListener(Initialize);
    }

    #endregion

    #region Pathfind

    public List<GridGraphNode> GetAstarPathFromTransforms(Transform startTransform, Transform goalTransform)
    {
        if (FindGridNodeByTransform(startTransform, out GridGraphNode startNode, true) 
            && FindGridNodeByTransform(goalTransform, out GridGraphNode goalNode, true))
        {
            return FindPath(startNode, goalNode, CalculateHeuristic);
        }
        else
        {
            Debug.LogWarning($"{startTransform} or {goalTransform} transform does not correspond to any node in the graph.");
            return new List<GridGraphNode>();
        }
    }

    private bool FindGridNodeByTransform(Transform target, out GridGraphNode node, bool approximatePos = true)
    {
        foreach (GridGraphNode n in graph.nodes)
        {
            if (n.transform == target)
            {
                node = n;
                return true;
            }
            else if (approximatePos && Vector3.Distance(n.transform.position, target.position) < graph.generationGridCellSize)
            {
                node = n;
                return true;
            }
        }
        node = null;
        return false;
    }

    private float CalculateHeuristic(Transform start, Transform end)
    {
        // Manhattan distance
        return Mathf.Abs(start.position.x - end.position.x)
            + Mathf.Abs(start.position.y - end.position.y)
            + Mathf.Abs(start.position.z - end.position.z);
    }

    public List<GridGraphNode> FindPath(GridGraphNode start, GridGraphNode goal, Heuristic heuristic = null, bool isAdmissible = true)
    {
        if (graph == null) return new List<GridGraphNode>();

        // if no heuristic is provided then set heuristic = 0
        if (heuristic == null) heuristic = (Transform s, Transform e) => 0;

        List<GridGraphNode> path = null;
        bool solutionFound = false;

        // dictionary to keep track of g(n) values (movement costs)
        Dictionary<GridGraphNode, float> gnDict = new()
        {
            { start, default }
        };

        // dictionary to keep track of f(n) values (movement cost + heuristic)
        Dictionary<GridGraphNode, float> fnDict = new()
        {
            { start, heuristic(start.transform, goal.transform) + gnDict[start] }
        };

        // dictionary to keep track of our path (came_from)
        Dictionary<GridGraphNode, GridGraphNode> pathDict = new()
        {
            { start, null }
        };

        List<GridGraphNode> openList = new()
        {
            start
        };

        HashSet<GridGraphNode> closedSet = new();

        int debugIteration = 0;

        while (openList.Count > 0 && debugIteration < 1000)
        {
            // mimic priority queue and remove from the back of the open list (lowest fn value)
            GridGraphNode current = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);

            closedSet.Add(current);

            // early exit
            if (current == goal && isAdmissible)
            {
                solutionFound = true;
                break;
            }
            else if (closedSet.Contains(goal))
            {
                // early exit strategy if heuristic is not admissible (try to avoid this if possible)
                float gGoal = gnDict[goal];
                bool pathIsTheShortest = true;

                foreach (GridGraphNode entry in openList)
                {
                    if (gGoal > gnDict[entry])
                    {
                        pathIsTheShortest = false;
                        break;
                    }
                }

                if (pathIsTheShortest) break;
            }

            List<GridGraphNode> neighbors = graph.GetNeighbors(current);
            foreach (GridGraphNode n in neighbors)
            {
				// the edge cost
                float movement_cost = 1;

                // TODO
                if (closedSet.Contains(n)) continue;

                // find gNeighbor (g_next)
                // ...
                float g_neighbor = gnDict[current] + movement_cost;
                gnDict[n] = g_neighbor;

                // check if you need to update tables, calculate fn, and update open_list using FakePQListInsert() function
                // and do so if necessary
                // ...

                float fn_current = g_neighbor + heuristic(n.transform, goal.transform);
                fnDict[n] = fn_current;

                pathDict[n] = current;

                FakePQListInsert(openList, fnDict, n);
            }
            debugIteration++;
        }

        // if the closed list contains the goal node then we have found a solution
        if (!solutionFound && closedSet.Contains(goal))
            solutionFound = true;

        if (solutionFound)
        {
            // TODO
            // create the path by traversing the previous nodes in the pathDict
            // starting at the goal and finishing at the start
            path = new List<GridGraphNode>
            {
                goal
            };
            GridGraphNode current = goal;

            while (current != start)
            {
                current = pathDict[current];
                path.Add(current);
            }


            // reverse the path since we started adding nodes from the goal 
            path.Reverse();
        }

        if (debug && openPointPrefab != null && closedPointPrefab != null && pathPointPrefab != null)
        {
            ClearPoints();

            List<Transform> openListPoints = new();
            foreach (GridGraphNode node in openList)
            {
                openListPoints.Add(node.transform);
            }
            SpawnPoints(openListPoints, openPointPrefab, Color.magenta);

            List<Transform> closedListPoints = new();
            foreach (GridGraphNode node in closedSet)
            {
                if (solutionFound && !path.Contains(node))
                    closedListPoints.Add(node.transform);
            }
            SpawnPoints(closedListPoints, closedPointPrefab, Color.red);

            if (solutionFound)
            {
                List<Transform> pathPoints = new();
                foreach (GridGraphNode node in path)
                {
                    pathPoints.Add(node.transform);
                }
                SpawnPoints(pathPoints, pathPointPrefab, Color.green);
            }
        }

        return path;
    }

    #endregion

    #region Debug

    private void SpawnPoints(List<Transform> points, GameObject prefab, Color color)
    {
        for (int i = 0; i < points.Count; ++i)
        {
#if UNITY_EDITOR
            // Scene view visuals
            points[i].GetComponent<GridGraphNode>()._nodeGizmoColor = color;
#endif

            // Game view visuals
            GameObject obj = Instantiate(prefab, points[i].position, Quaternion.identity, points[i]);
            obj.name = "DEBUG_POINT";
            obj.transform.localPosition += Vector3.up * 0.5f;
        }
    }

    private void ClearPoints()
    {
        foreach (GridGraphNode node in graph.nodes)
        {
			node._nodeGizmoColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f);
            for (int c = 0; c < node.transform.childCount; ++c)
            {
                if (node.transform.GetChild(c).name == "DEBUG_POINT")
                {
                    Destroy(node.transform.GetChild(c).gameObject);
                }
            }
        }
    }

    #endregion

    #region Utility

    /// <summary>
    /// mimics a priority queue here by inserting at the right position using a loop
    /// not a very good solution but ok for this lab example
    /// </summary>
    /// <param name="pqList"></param>
    /// <param name="fnDict"></param>
    /// <param name="node"></param>
    private void FakePQListInsert(List<GridGraphNode> pqList, Dictionary<GridGraphNode, float> fnDict, GridGraphNode node)
    {
        if (pqList.Count == 0)
            pqList.Add(node);
        else
        {
            for (int i = pqList.Count - 1; i >= 0; --i)
            {
                if (fnDict[pqList[i]] > fnDict[node])
                {
                    pqList.Insert(i + 1, node);
                    break;
                }
                else if (i == 0)
                    pqList.Insert(0, node);
            }
        }
    }

    #endregion
}
