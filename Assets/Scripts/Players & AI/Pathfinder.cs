using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    public static List<Vector2> Dijkstra(Vector2 start, Vector2 end, LayerMask obstacleMask)
    {
        float gridSize = 0.6f; // Smaller grid for precision
        var openSet = new PriorityQueue<Vector2>();
        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        while (openSet.Count > 0)
        {
            Vector2 current = openSet.Dequeue();

            // Early exit if close to target
            if (Vector2.Distance(current, end) < gridSize)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (Vector2 neighbor in GetNeighbors(current, gridSize, obstacleMask))
            {
                float tentativeGScore = gScore[current] + gridSize;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    float fScore = tentativeGScore + Heuristic(neighbor, end);
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        return new List<Vector2>(); // Empty if no path
    }

    private static List<Vector2> GetNeighbors(Vector2 pos, float gridSize, LayerMask obstacleMask)
    {
        List<Vector2> neighbors = new List<Vector2>();
        Vector2[] directions = {
            Vector2.up * gridSize,
            Vector2.right * gridSize,
            Vector2.down * gridSize,
            Vector2.left * gridSize
        };

        foreach (Vector2 dir in directions)
        {
            Vector2 neighbor = pos + dir;
            if (!Physics2D.OverlapCircle(neighbor, gridSize / 2, obstacleMask))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private static float Heuristic(Vector2 a, Vector2 b) => Vector2.Distance(a, b);

    private static List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        var path = new List<Vector2> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
}