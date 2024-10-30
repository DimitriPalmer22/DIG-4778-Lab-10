using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Pathfinding : MonoBehaviour
{
    private List<Vector2Int> path = new List<Vector2Int>();
    [SerializeField] private Vector2Int start = new Vector2Int(0, 1);
    [SerializeField] private Vector2Int goal = new Vector2Int(4, 4);
    private Vector2Int next;
    private Vector2Int current;

    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private int[,] grid = new int[,]
    {
        { 0, 1, 0, 0, 0 },
        { 0, 1, 0, 1, 0 },
        { 0, 0, 0, 1, 0 },
        { 0, 1, 1, 1, 0 },
        { 0, 0, 0, 0, 0 }
    };

    [SerializeField] [Range(0, 1)] private float probability = 0.15f;

    private Vector2Int previousStart;
    private Vector2Int previousGoal;


    private void Start()
    {
        GenerateRandomGrid(grid.GetLength(0), grid.GetLength(1), probability);

        FindPath(start, goal);
    }

    private void Update()
    {
        // Update the path if the start or end are changed
        if (previousStart != start || previousGoal != goal)
            FindPath(start, goal);

        previousStart = start;
        previousGoal = goal;
    }

    private void OnDrawGizmos()
    {
        var cellSize = 1f;

        // Draw grid cells
        for (var y = 0; y < grid.GetLength(0); y++)
        {
            for (var x = 0; x < grid.GetLength(1); x++)
            {
                var cellPosition = new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.color = grid[y, x] == 1 ? Color.black : Color.white;
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }

        // Draw path
        foreach (var step in path)
        {
            var cellPosition = new Vector3(step.x * cellSize, 0, step.y * cellSize);

            Gizmos.color = Color.blue;
            Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
        }

        // Draw start and goal
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(start.x * cellSize, 0, start.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(goal.x * cellSize, 0, goal.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));
    }

    private bool IsInBounds(Vector2Int point)
    {
        return point.x >= 0 && point.x < grid.GetLength(1) && point.y >= 0 && point.y < grid.GetLength(0);
    }

    private void FindPath(Vector2Int start, Vector2Int goal)
    {
        // Clear the path
        path.Clear();

        var frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);

        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (var direction in directions)
            {
                next = current + direction;

                if (IsInBounds(next) && grid[next.y, next.x] == 0 && !cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            Debug.Log("Path not found.");
            return;
        }

        // Trace path from goal to start
        var step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Add(start);
        path.Reverse();
    }

    #region New Functions

    private void GenerateRandomGrid(int width, int height, float obstacleProbability)
    {
        for (var cY = 0; cY < height; cY++)
        {
            for (var cX = 0; cX < width; cX++)
            {
                // First, clear the current cell
                RemoveObstacle(new Vector2Int(cX, cY));

                // Skip the current cell if it's the start or goal
                if ((cX == start.x && cY == start.y) || (cX == goal.x && cY == goal.y))
                    continue;

                //Determine if the probability is less than the obstacle probability
                if (Random.value < obstacleProbability)
                    AddObstacle(new Vector2Int(cX, cY));
            }
        }
    }

    public void AddObstacle(Vector2Int position)
    {
        // Test if the position is in bounds
        // If it isn't return
        if (!IsInBounds(position))
            return;

        // Set the grid value at the position to 1
        grid[position.y, position.x] = 1;
    }

    public void RemoveObstacle(Vector2Int position)
    {
        // Test if the position is in bounds
        // If it isn't return
        if (!IsInBounds(position))
            return;

        // Set the grid value at the position to 0
        grid[position.y, position.x] = 0;
    }

    #endregion
}