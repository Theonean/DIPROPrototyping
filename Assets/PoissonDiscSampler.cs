using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampler
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 regionWidth, Vector2 regionHeight, int maxAttempts)
    {
        float cellSize = radius / Mathf.Sqrt(2);
        int gridWidth = Mathf.CeilToInt((regionWidth.y - regionWidth.x) / cellSize);
        int gridHeight = Mathf.CeilToInt((regionHeight.y - regionHeight.x) / cellSize);

        Vector2[,] grid = new Vector2[gridWidth, gridHeight];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>
        {
            new Vector2(
                Random.Range(regionWidth.x, regionWidth.y),
                Random.Range(regionHeight.x, regionHeight.y)
            )
        };

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool validPointFound = false;

            for (int i = 0; i < maxAttempts; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCenter + direction * Random.Range(radius, 2 * radius);

                if (IsValid(candidate, regionWidth, regionHeight, cellSize, radius, grid, gridWidth, gridHeight))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)((candidate.x - regionWidth.x) / cellSize), (int)((candidate.y - regionHeight.x) / cellSize)] = candidate;
                    validPointFound = true;
                    break;
                }
            }

            if (!validPointFound)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return points;
    }

    private static bool IsValid(Vector2 candidate, Vector2 regionWidth, Vector2 regionHeight, float cellSize, float radius, Vector2[,] grid, int gridWidth, int gridHeight)
    {
        if (candidate.x < regionWidth.x || candidate.x > regionWidth.y || candidate.y < regionHeight.x || candidate.y > regionHeight.y)
            return false;

        int cellX = (int)((candidate.x - regionWidth.x) / cellSize);
        int cellY = (int)((candidate.y - regionHeight.x) / cellSize);

        int searchStartX = Mathf.Max(0, cellX - 2);
        int searchEndX = Mathf.Min(cellX + 2, gridWidth - 1);
        int searchStartY = Mathf.Max(0, cellY - 2);
        int searchEndY = Mathf.Min(cellY + 2, gridHeight - 1);

        for (int x = searchStartX; x <= searchEndX; x++)
        {
            for (int y = searchStartY; y <= searchEndY; y++)
            {
                Vector2 point = grid[x, y];
                if (point != Vector2.zero)
                {
                    float sqrDst = (candidate - point).sqrMagnitude;
                    if (sqrDst < radius * radius)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
