using System.Collections.Generic;
using UnityEngine;

namespace Waves
{
	public class WaveSpawnArea : MonoBehaviour
	{
		private const int DEFAULT_CANDIDATE_ATTEMPTS = 12;
		private const float DEFAULT_BASE_MIN_SPACING = 1f;
		private const float MIN_SPACING_CLAMP = 0.05f;
		private const float MIN_HALF_EXTENT = 0.5f;
		private const float DENSITY_SPACING_FACTOR = 0.45f;
		private const float FULL_AREA_MULTIPLIER = 2f;
		private const int MIN_ENEMY_COUNT_FOR_DENSITY = 1;
		private const float INITIAL_BEST_SCORE = -1f;
		private const float MIN_GIZMO_DIMENSION = 1f;
		private const float GIZMO_DEPTH = 0.01f;
		private const float GIZMO_CENTER_RADIUS = 0.08f;

		private static readonly Color DefaultGizmoFillColor = new(0.1f, 0.8f, 1f, 0.12f);
		private static readonly Color DefaultGizmoOutlineColor = new(0.1f, 0.8f, 1f, 0.9f);

		[Header("Settings")]
		[SerializeField] private Vector2Int SpawnArea;
		[SerializeField] private WaveSpawnAreaType WaveAreaType;

		public WaveSpawnAreaType WaveSpawnAreaType => WaveAreaType;

		public void DistributeEnemies(List<Enemy.Enemy> enemies)
		{
			if (enemies == null || enemies.Count == 0)
				return;

			List<Vector2> points = GetSpawnPoints(enemies.Count);
			PlaceEnemies(enemies, points);
		}

		public List<Vector2> GetSpawnPoints(int count)
		{
			List<Vector2> points = new(count);

			Vector2 center = this.transform.position;
			float maxX = Mathf.Max(MIN_HALF_EXTENT, SpawnArea.x * MIN_HALF_EXTENT);
			float maxY = Mathf.Max(MIN_HALF_EXTENT, SpawnArea.y * MIN_HALF_EXTENT);
			Vector2 halfExtents = new(
				maxX,
				maxY);

			float area = halfExtents.x * FULL_AREA_MULTIPLIER * (halfExtents.y * FULL_AREA_MULTIPLIER);
			float densitySpacing = Mathf.Sqrt(area / Mathf.Max(MIN_ENEMY_COUNT_FOR_DENSITY, count)) * DENSITY_SPACING_FACTOR;
			float minSpacing = Mathf.Max(MIN_SPACING_CLAMP, Mathf.Min(DEFAULT_BASE_MIN_SPACING, densitySpacing));
			float minSpacingSqr = minSpacing * minSpacing;

			for (int i = 0; i < count; i++)
			{
				Vector2 bestCandidate = RandomPointInArea(center, halfExtents);
				float bestScore = INITIAL_BEST_SCORE;

				for (int attempt = 0; attempt < DEFAULT_CANDIDATE_ATTEMPTS; attempt++)
				{
					Vector2 candidate = RandomPointInArea(center, halfExtents);
					float score = GetNearestDistanceSqr(candidate, points);

					if (score > bestScore)
					{
						bestScore = score;
						bestCandidate = candidate;
					}

					if (score >= minSpacingSqr)
						break;
				}

				points.Add(bestCandidate);
			}

			return points;
		}

		public void PlaceEnemies(List<Enemy.Enemy> enemies, List<Vector2> points)
		{
			for (int i = 0; i < enemies.Count; i++)
			{
				Enemy.Enemy enemy = enemies[i];
				if (!enemy)
					continue;

				Vector3 current = enemy.transform.position;
				Vector2 point = points[i];
				enemy.transform.position = new Vector3(point.x, point.y, current.z);
			}
		}

		private static float GetNearestDistanceSqr(Vector2 point, List<Vector2> existingPoints)
		{
			if (existingPoints.Count == 0)
				return float.PositiveInfinity;

			float nearest = float.PositiveInfinity;
			foreach (Vector2 existingPoint in existingPoints)
			{
				float d = (point - existingPoint).sqrMagnitude;
				if (d < nearest)
					nearest = d;
			}

			return nearest;
		}

		private static Vector2 RandomPointInArea(Vector2 center, Vector2 halfExtents)
		{
			float rangeX = Random.Range(-halfExtents.x, halfExtents.x);
			float rangeY = Random.Range(-halfExtents.y, halfExtents.y);
			return center + new Vector2(rangeX, rangeY);
		}

		private void OnDrawGizmos()
		{
			Vector3 center = this.transform.position;
			float width = Mathf.Max(MIN_GIZMO_DIMENSION, SpawnArea.x);
			float height = Mathf.Max(MIN_GIZMO_DIMENSION, SpawnArea.y);
			Vector3 size = new(width, height, GIZMO_DEPTH);

			Gizmos.color = DefaultGizmoFillColor;
			Gizmos.DrawCube(center, size);

			Gizmos.color = DefaultGizmoOutlineColor;
			Gizmos.DrawWireCube(center, size);

			Gizmos.DrawSphere(center, GIZMO_CENTER_RADIUS);
		}
	}
}
