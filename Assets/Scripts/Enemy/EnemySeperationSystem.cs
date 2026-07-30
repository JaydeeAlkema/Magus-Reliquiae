using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
	public class EnemySeparationSystem : MonoBehaviour
	{
		[SerializeField] private float SeparationSpeed = 2f;
		[SerializeField] private float CellSize = 0.6f;
		[SerializeField][Min(1)]
		private int ExpectedEnemyCount = 256;
		[SerializeField][Min(1)]
		private int ExpectedActiveCells = 128;
		[SerializeField][Min(1)]
		private int ExpectedCellBucketSize = 4;
		[SerializeField][Min(1)]
		private int ExpectedDebugEntries = 256;
		[SerializeField][Min(1)]
		private int CellPruneIntervalFrames = 120;
		[SerializeField][Min(2)]
		private int StaleCellPruneRatio = 4;

		[Header("Debug")]
		[SerializeField] private bool DrawDebug = true;
		[SerializeField] private bool DrawGrid = true;
		[SerializeField] private bool DrawCollisionRadii = true;
		[SerializeField] private bool DrawPushVectors = true;
		[SerializeField][Min(0f)]
		private float PushVectorScale = 1f;
		[SerializeField] private Color GridColor = new(0.2f, 0.8f, 1f, 0.35f);
		[SerializeField] private Color RadiusColor = new(0.2f, 1f, 0.35f, 0.8f);
		[SerializeField] private Color PushColor = new(1f, 0.35f, 0.2f, 0.95f);

		private Dictionary<(int, int), List<EnemyContact>> _grid;
		private Dictionary<EnemyContact, Vector2> _debugLastPush;
		private List<(int, int)> _activeCells;
		private List<(int, int)> _staleCells;
		private int _cellBucketCapacity;
		private int _framesUntilPrune;

		private void Awake()
		{
			int enemyCapacity = Mathf.Max(1, ExpectedEnemyCount);
			int activeCellCapacity = Mathf.Max(1, ExpectedActiveCells);
			_cellBucketCapacity = Mathf.Max(1, ExpectedCellBucketSize);
			int debugEntryCapacity = Mathf.Max(1, ExpectedDebugEntries);
			_framesUntilPrune = Mathf.Max(1, CellPruneIntervalFrames);

			EnemyPushRegistry.EnsureCapacity(enemyCapacity);
			_grid = new Dictionary<(int, int), List<EnemyContact>>(activeCellCapacity);
			_debugLastPush = new Dictionary<EnemyContact, Vector2>(debugEntryCapacity);
			_activeCells = new List<(int, int)>(activeCellCapacity);
			_staleCells = new List<(int, int)>(activeCellCapacity);
		}

		private void FixedUpdate()
		{
			BuildGrid();
			ResolveSeparation();
		}

		private void OnDisable()
		{
			_debugLastPush?.Clear();
		}

		private void BuildGrid()
		{
			foreach ((int, int) activeCell in _activeCells)
			{
				_grid[activeCell].Clear();
			}
			_activeCells.Clear();

			List<EnemyContact> enemies = EnemyPushRegistry.Active;
			foreach (EnemyContact enemy in enemies)
			{
				(int, int) cell = CellOf(enemy.Position);
				if (!_grid.TryGetValue(cell, out List<EnemyContact> bucket))
				{
					bucket = new List<EnemyContact>(_cellBucketCapacity);
					_grid[cell] = bucket;
				}

				if (bucket.Count == 0)
					_activeCells.Add(cell);

				bucket.Add(enemy);
			}

			_framesUntilPrune--;
			if (_framesUntilPrune > 0)
				return;
			
			PruneEmptyCells();
			_framesUntilPrune = Mathf.Max(1, CellPruneIntervalFrames);
		}

		private void PruneEmptyCells()
		{
			int pruneRatio = Mathf.Max(2, StaleCellPruneRatio);
			if (_grid.Count <= _activeCells.Count * pruneRatio)
				return;

			_staleCells.Clear();
			foreach (KeyValuePair<(int, int), List<EnemyContact>> entry in _grid)
			{
				if (entry.Value.Count == 0)
					_staleCells.Add(entry.Key);
			}

			foreach ((int, int) staleCell in _staleCells)
			{
				_grid.Remove(staleCell);
			}
		}

		private (int, int) CellOf(Vector2 position)
		{
			return (Mathf.FloorToInt(position.x / CellSize), Mathf.FloorToInt(position.y / CellSize));
		}

		private void ResolveSeparation()
		{
			float maxStep = SeparationSpeed * Time.fixedDeltaTime;
			List<EnemyContact> enemies = EnemyPushRegistry.Active;
			bool collectDebugPush = DrawDebug && DrawPushVectors;
			if (collectDebugPush)
				_debugLastPush.Clear();

			foreach (EnemyContact enemy in enemies)
			{
				(int cx, int cy) = CellOf(enemy.Position);
				Vector2 push = Vector2.zero;

				// Only the 3x3 neighborhood of cells around this enemy, not
				// every enemy in the scene.
				for (int dx = -1; dx <= 1; dx++)
				{
					for (int dy = -1; dy <= 1; dy++)
					{
						if (!_grid.TryGetValue((cx + dx, cy + dy), out List<EnemyContact> neighbors))
							continue;

						foreach (EnemyContact other in neighbors)
						{
							if (other == enemy)
								continue;

							Vector2 offset = enemy.Position - other.Position;
							float combinedRadius = enemy.Radius + other.Radius;
							float sqrDist = offset.sqrMagnitude;

							if (sqrDist >= combinedRadius * combinedRadius || sqrDist < 1e-6f)
								continue;

							float dist = Mathf.Sqrt(sqrDist);
							float overlap = combinedRadius - dist;
							push += offset / dist * (overlap * 0.5f);
						}
					}
				}

				if (push.sqrMagnitude > maxStep * maxStep)
					push = push.normalized * maxStep;

				if (collectDebugPush && push != Vector2.zero)
					_debugLastPush[enemy] = push;

				if (push != Vector2.zero)
					enemy.Push(push);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (!DrawDebug || !Application.isPlaying)
				return;

			if (DrawGrid)
			{
				Gizmos.color = GridColor;
				foreach ((int cx, int cy) in _grid.Keys)
				{
					Vector3 center = new(
						(cx + 0.5f) * CellSize,
						(cy + 0.5f) * CellSize, this.transform.position.z
					);
					Gizmos.DrawWireCube(center, new Vector3(CellSize, CellSize, 0f));
				}
			}

			List<EnemyContact> enemies = EnemyPushRegistry.Active;

			if (DrawCollisionRadii)
			{
				Gizmos.color = RadiusColor;
				foreach (EnemyContact enemy in enemies)
				{
					Gizmos.DrawWireSphere(enemy.Position, enemy.Radius);
				}
			}

			if (DrawPushVectors)
			{
				Gizmos.color = PushColor;
				foreach (EnemyContact enemy in enemies)
				{
					if (!_debugLastPush.TryGetValue(enemy, out Vector2 push) || push == Vector2.zero)
						continue;

					Vector3 start = enemy.Position;
					Vector3 end = start + (Vector3)(push * PushVectorScale);
					Gizmos.DrawLine(start, end);
					Gizmos.DrawSphere(end, 0.03f);
				}
			}
		}
	}
}
