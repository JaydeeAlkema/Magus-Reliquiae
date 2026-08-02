using System.Collections.Generic;
using UnityEngine;
using Waves;
using Random = UnityEngine.Random;

namespace Enemy
{
	[DefaultExecutionOrder(-100)]
	public class EnemyLodSystem : MonoBehaviour
	{
		private enum EnemySimulationState
		{
			Active = 0,
			Sleeping = 1,
		}

		private const int DEFAULT_REHOME_ATTEMPTS = 8;

		[Header("References")]
		[SerializeField] private Player.Player Player;
		[SerializeField] private WaveManager WaveManager;

		[Header("Distance bands")]
		[SerializeField][Min(0f)] private float ActiveRadius = 18f;
		[SerializeField][Min(0f)] private float SleepRadius = 24f;
		[SerializeField][Min(0f)] private float RehomeRadius = 40f;

		[Header("Batching")]
		[SerializeField][Min(1)] private int ChecksPerFixedUpdate = 32;

		[Header("Rehome")]
		[SerializeField][Min(1)] private int RehomeAttemptsPerEnemy = DEFAULT_REHOME_ATTEMPTS;

		private readonly List<Enemy> _enemies = new(256);
		private readonly List<WaveSpawnArea> _spawnAreas = new(8);
		private int _nextEnemyIndex;

		private void Awake()
		{
			RefreshSpawnAreas();
		}

		private void OnEnable()
		{
			Enemy.EnemyEnabled += Register;
			Enemy.EnemyDisabled += Unregister;
		}

		private void OnDisable()
		{
			Enemy.EnemyEnabled -= Register;
			Enemy.EnemyDisabled -= Unregister;
		}

		private void FixedUpdate()
		{
			if (!EnsureReady())
				return;

			int enemyCount = _enemies.Count;
			if (enemyCount == 0)
				return;

			int checks = Mathf.Min(enemyCount, Mathf.Max(1, ChecksPerFixedUpdate));
			for (int i = 0; i < checks; i++)
			{
				if (_nextEnemyIndex >= _enemies.Count)
					_nextEnemyIndex = 0;

				Enemy enemy = _enemies[_nextEnemyIndex];
				_nextEnemyIndex++;

				if (!enemy || !enemy.isActiveAndEnabled)
					continue;

				EvaluateEnemy(enemy);
			}
		}

		public void Register(Enemy enemy)
		{
			if (!enemy || _enemies.Contains(enemy))
				return;

			_enemies.Add(enemy);

			if (!EnsureReady())
			{
				enemy.SetSimulationActive(true);
				return;
			}

			EvaluateEnemy(enemy);
		}

		public void Unregister(Enemy enemy)
		{
			int index = _enemies.IndexOf(enemy);
			if (index < 0)
				return;

			int last = _enemies.Count - 1;
			_enemies[index] = _enemies[last];
			_enemies.RemoveAt(last);

			if (_nextEnemyIndex > index)
				_nextEnemyIndex--;

			if (_nextEnemyIndex > _enemies.Count)
				_nextEnemyIndex = _enemies.Count;
		}

		private bool EnsureReady()
		{
			if (!Player)
				return false;

			if (_spawnAreas.Count == 0)
				RefreshSpawnAreas();

			ClampDistances();
			return true;
		}

		private void RefreshSpawnAreas()
		{
			_spawnAreas.Clear();
			if (!WaveManager)
				return;

			foreach (WaveSpawnArea area in WaveManager.SpawnAreas)
			{
				if (area)
					_spawnAreas.Add(area);
			}
		}

		private void ClampDistances()
		{
			ActiveRadius = Mathf.Max(0f, ActiveRadius);
			SleepRadius = Mathf.Max(ActiveRadius, SleepRadius);
			RehomeRadius = Mathf.Max(SleepRadius, RehomeRadius);
		}

		private void EvaluateEnemy(Enemy enemy)
		{
			Vector2 playerPos = Player.transform.position;
			Vector2 enemyPos = enemy.Position;
			float sqrDistance = (enemyPos - playerPos).sqrMagnitude;
			float rehomeRadiusSqr = RehomeRadius * RehomeRadius;

			if (sqrDistance > rehomeRadiusSqr && TryRehomeEnemy(enemy, playerPos))
			{
				enemyPos = enemy.Position;
				sqrDistance = (enemyPos - playerPos).sqrMagnitude;
			}

			EnemySimulationState nextState = GetDesiredState(sqrDistance, enemy.IsSimulationActive);
			enemy.SetSimulationActive(nextState == EnemySimulationState.Active);
		}

		private EnemySimulationState GetDesiredState(float sqrDistance, bool isCurrentlyActive)
		{
			float activeRadiusSqr = ActiveRadius * ActiveRadius;
			float sleepRadiusSqr = SleepRadius * SleepRadius;

			if (isCurrentlyActive)
				return sqrDistance > sleepRadiusSqr ? EnemySimulationState.Sleeping : EnemySimulationState.Active;

			return sqrDistance <= activeRadiusSqr ? EnemySimulationState.Active : EnemySimulationState.Sleeping;
		}

		private bool TryRehomeEnemy(Enemy enemy, Vector2 playerPos)
		{
			if (_spawnAreas.Count == 0)
				return false;

			float minDistanceSqr = ActiveRadius * ActiveRadius;
			int attempts = Mathf.Max(1, RehomeAttemptsPerEnemy);
			Vector2 fallbackPoint = default;
			bool hasFallback = false;

			for (int i = 0; i < attempts; i++)
			{
				WaveSpawnArea area = GetRandomAllowedSpawnArea(enemy);
				if (!area)
					continue;

				Vector2 candidate = area.GetRandomPoint();
				if (!hasFallback)
				{
					fallbackPoint = candidate;
					hasFallback = true;
				}

				if ((candidate - playerPos).sqrMagnitude < minDistanceSqr)
					continue;

				enemy.Teleport(candidate);
				return true;
			}

			if (!hasFallback)
				return false;

			enemy.Teleport(fallbackPoint);
			return true;
		}

		private WaveSpawnArea GetRandomAllowedSpawnArea(Enemy enemy)
		{
			if (!enemy.TryGetSpawnAreaType(out WaveSpawnAreaType spawnAreaType))
				return _spawnAreas[Random.Range(0, _spawnAreas.Count)];

			int matchingCount = 0;
			foreach (WaveSpawnArea area in _spawnAreas)
			{
				if (area && area.WaveSpawnAreaType == spawnAreaType)
					matchingCount++;
			}

			if (matchingCount == 0)
				return _spawnAreas[Random.Range(0, _spawnAreas.Count)];

			int targetMatchIndex = Random.Range(0, matchingCount);
			foreach (WaveSpawnArea area in _spawnAreas)
			{
				if (!area || area.WaveSpawnAreaType != spawnAreaType)
					continue;

				if (targetMatchIndex == 0)
					return area;

				targetMatchIndex--;
			}

			return _spawnAreas[Random.Range(0, _spawnAreas.Count)];
		}

		private void OnDrawGizmosSelected()
		{
			Vector3 transformPosition = this.transform.position;

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transformPosition, ActiveRadius);

			Gizmos.color = Color.orange;
			Gizmos.DrawWireSphere(transformPosition, SleepRadius);

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transformPosition, RehomeRadius);
		}
	}
}
