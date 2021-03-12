using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Poisson
{
	public static List<Vector2> GeneratePoints(float radius, Vector2 size, int k = 30)
	{
		float cellSize = radius / Mathf.Sqrt(2);

		int[,] grid = new int[Mathf.CeilToInt(size.x / cellSize), Mathf.CeilToInt(size.y / cellSize)];
		List<Vector2> spawnPoints = new List<Vector2>();
		List<Vector2> points = new List<Vector2>();

		points.Add(size / 2);
		while (points.Count > 0)
		{
			int index = Random.Range(0, points.Count);
			Vector2 center = points[index];
			bool pointAccepted = false;

			for (int i = 0; i < k; i++)
			{
				float angle = Random.value * Mathf.PI * 2;
				Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				Vector2 point = center + direction * Random.Range(radius, 2 * radius);

				if (IsValid(point, size, cellSize, radius, spawnPoints, grid))
				{
					spawnPoints.Add(point);
					points.Add(point);
					grid[(int)(point.x / cellSize), (int)(point.y / cellSize)] = spawnPoints.Count;
					pointAccepted = true;
					break;
				}
			}
			if (!pointAccepted)
			{
				points.RemoveAt(index);
			}
		}

		return spawnPoints;
	}

	static bool IsValid(Vector2 point, Vector2 size, float cellSize, float radius, List<Vector2> points, int[,] grid)
	{
		if (point.x >= 0 && point.x < size.x && point.y >= 0 && point.y < size.y)
		{
			int cellX = (int)(point.x / cellSize);
			int cellY = (int)(point.y / cellSize);
			int startX = Mathf.Max(0, cellX - 2);
			int endX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
			int startY = Mathf.Max(0, cellY - 2);
			int endY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

			for (int x = startX; x <= endX; x++)
			{
				for (int y = startY; y <= endY; y++)
				{
					int index = grid[x, y] - 1;
					if (index != -1)
					{
						float sqrDistancest = (point - points[index]).sqrMagnitude;
						if (sqrDistancest < radius * radius)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}
}
