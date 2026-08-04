using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Waves
{
	/// <summary>
	/// Coordinates wave timing, enemy pooling, and enemy spawning.
	/// </summary>
	/// <remarks>
	/// Assign wave assets, spawn areas, and enemy prefabs in the inspector; the manager handles the rest at runtime.
	/// </remarks>
	public class WaveManager : MonoBehaviour
	{
		private const int DEFAULT_POOL_CAPACITY = 128;
		private const int MAX_POOL_SIZE = 512;
		private const int FIRST_INDEX = 0;

		[Header("Waves")]
		[SerializeField] private List<WaveSO> Waves;
		[SerializeField] private List<WaveSpawnArea> WaveSpawnAreas;
		[SerializeField] private float TimeToStartFirstWave;

		[Header("Spawning")]
		[SerializeField] private int MaxEnemiesSpawnedPerFrame = 6;

		[Header("Pre-warming")]
		[SerializeField] private List<Enemy.Enemy> EnemiesToPrewarm;
		[SerializeField] private int PrewarmCountPerType = 10;

		/// <summary>
		/// Active spawn areas used by the current wave setup.
		/// </summary>
		public IReadOnlyList<WaveSpawnArea> SpawnAreas => WaveSpawnAreas;

		private List<WaveSO> _wavesCopy = new();
		private WaveSO _currentWave;
		private float _waveSpawnTimer;
		private readonly Dictionary<Enemy.Enemy, ObjectPool<Enemy.Enemy>> _enemyPools = new();

		private readonly Dictionary<Enemy.Enemy, EnemyPoolReturn> _poolReturns = new();

		private void Awake()
		{
			if (Waves == null || Waves.Count == 0)
			{
				this.enabled = false;
				return;
			}

			_wavesCopy = new List<WaveSO>(Waves);
			_currentWave = _wavesCopy[FIRST_INDEX];
			_waveSpawnTimer = TimeToStartFirstWave;

			PrewarmPools();
		}

		private void Update()
		{
			SpawnWaves();
		}

		private void SpawnWaves()
		{
			_waveSpawnTimer -= Time.deltaTime;
			if (_waveSpawnTimer > 0)
				return;

			SpawnEnemies();
			AdvanceWave();
		}

		private void AdvanceWave()
		{
			_wavesCopy.RemoveAt(FIRST_INDEX);
			if (_wavesCopy.Count == 0)
			{
				this.enabled = false;
				return;
			}

			_currentWave = _wavesCopy[FIRST_INDEX];
			_waveSpawnTimer = _currentWave.TimeUntilNextWave;
		}

		private void SpawnEnemies()
		{
			List<Enemy.Enemy> enemyPrefabs = _currentWave.GetEnemiesToSpawn();
			if (enemyPrefabs.Count == 0)
				return;

			List<WaveSpawnArea> spawnAreas = GetSpawnAreasForCurrentWave();
			if (spawnAreas.Count == 0)
				return;

			StartCoroutine(SpawnEnemiesRoutine(enemyPrefabs, spawnAreas));
		}

		private IEnumerator SpawnEnemiesRoutine(List<Enemy.Enemy> enemyPrefabs, List<WaveSpawnArea> spawnAreas)
		{
			List<Enemy.Enemy> validEnemyPrefabs = new(enemyPrefabs.Count);
			foreach (Enemy.Enemy enemyPrefab in enemyPrefabs)
			{
				if (!enemyPrefab)
					continue;

				validEnemyPrefabs.Add(enemyPrefab);
			}

			if (validEnemyPrefabs.Count == 0)
				yield break;

			List<List<Enemy.Enemy>> prefabBuckets = new(spawnAreas.Count);
			for (int i = 0; i < spawnAreas.Count; i++)
			{
				prefabBuckets.Add(new List<Enemy.Enemy>());
			}

			for (int i = 0; i < validEnemyPrefabs.Count; i++)
			{
				int bucketIndex = i % spawnAreas.Count;
				prefabBuckets[bucketIndex].Add(validEnemyPrefabs[i]);
			}

			for (int areaIndex = 0; areaIndex < spawnAreas.Count; areaIndex++)
			{
				WaveSpawnArea spawnArea = spawnAreas[areaIndex];
				List<Enemy.Enemy> areaPrefabs = prefabBuckets[areaIndex];
				if (areaPrefabs.Count == 0)
					continue;

				List<Vector2> spawnPoints = spawnArea.GetSpawnPoints(areaPrefabs.Count);
				int nextPointIndex = 0;

				List<Enemy.Enemy> chunkEnemies = new(MaxEnemiesSpawnedPerFrame);
				List<Vector2> chunkPoints = new(MaxEnemiesSpawnedPerFrame);

				foreach (Enemy.Enemy enemyPrefab in areaPrefabs)
				{
					ObjectPool<Enemy.Enemy> pool = GetOrCreatePool(enemyPrefab);
					Enemy.Enemy enemyInstance = pool.Get();

					chunkEnemies.Add(enemyInstance);
					chunkPoints.Add(spawnPoints[nextPointIndex]);
					nextPointIndex++;

					if (chunkEnemies.Count < MaxEnemiesSpawnedPerFrame)
						continue;

					spawnArea.PlaceEnemies(chunkEnemies, chunkPoints);
					chunkEnemies = new List<Enemy.Enemy>(MaxEnemiesSpawnedPerFrame);
					chunkPoints = new List<Vector2>(MaxEnemiesSpawnedPerFrame);
					yield return null;
				}

				if (chunkEnemies.Count > 0)
					spawnArea.PlaceEnemies(chunkEnemies, chunkPoints);
			}
		}

		private List<WaveSpawnArea> GetSpawnAreasForCurrentWave()
		{
			List<WaveSpawnArea> matchingAreas = new();

			foreach (WaveSpawnArea waveSpawnArea in WaveSpawnAreas)
			{
				if (!waveSpawnArea)
					continue;

				if (waveSpawnArea.WaveSpawnAreaType == _currentWave.WaveAreaType)
					matchingAreas.Add(waveSpawnArea);
			}

			if (matchingAreas.Count > 0)
				return matchingAreas;

			foreach (WaveSpawnArea waveSpawnArea in WaveSpawnAreas)
			{
				if (waveSpawnArea)
					matchingAreas.Add(waveSpawnArea);
			}

			return matchingAreas;
		}

		private void PrewarmPools()
		{
			if (EnemiesToPrewarm == null)
				return;

			foreach (Enemy.Enemy enemyPrefab in EnemiesToPrewarm)
			{
				if (!enemyPrefab)
					continue;

				ObjectPool<Enemy.Enemy> pool = GetOrCreatePool(enemyPrefab);
				List<Enemy.Enemy> warmed = new(PrewarmCountPerType);

				for (int i = 0; i < PrewarmCountPerType; i++)
				{
					warmed.Add(pool.Get());
				}

				foreach (Enemy.Enemy enemy in warmed)
				{
					pool.Release(enemy);
				}
			}
		}

		private ObjectPool<Enemy.Enemy> GetOrCreatePool(Enemy.Enemy enemyPrefab)
		{
			if (_enemyPools.TryGetValue(enemyPrefab, out ObjectPool<Enemy.Enemy> existingPool))
				return existingPool;

			ObjectPool<Enemy.Enemy> pool = new(
				CreateFunc,
				ActionOnGet,
				ActionOnRelease,
				ActionOnDestroy,
				true,
				DEFAULT_POOL_CAPACITY,
				MAX_POOL_SIZE
			);

			_enemyPools[enemyPrefab] = pool;
			return pool;

			void ActionOnGet(Enemy.Enemy enemy)
			{
				if (_poolReturns.TryGetValue(enemy, out EnemyPoolReturn enemyPoolReturn))
					enemyPoolReturn.Setup(enemy, _enemyPools[enemyPrefab]);

				enemy.gameObject.SetActive(true);
			}

			void ActionOnRelease(Enemy.Enemy enemy)
			{
				_poolReturns.TryGetValue(enemy, out EnemyPoolReturn enemyPoolReturn);

				if (enemyPoolReturn)
					enemyPoolReturn.IsReleasingToPool = true;

				enemy.gameObject.SetActive(false);

				if (enemyPoolReturn)
					enemyPoolReturn.IsReleasingToPool = false;
			}

			Enemy.Enemy CreateFunc()
			{
				return CreateEnemy(enemyPrefab);
			}

			void ActionOnDestroy(Enemy.Enemy enemy)
			{
				_poolReturns.Remove(enemy);

				if (enemy)
					Destroy(enemy.gameObject);
			}
		}

		private Enemy.Enemy CreateEnemy(Enemy.Enemy enemyPrefab)
		{
			Enemy.Enemy enemy = Instantiate(enemyPrefab, this.transform);

			EnemyPoolReturn enemyPoolReturn = enemy.GetComponent<EnemyPoolReturn>();
			if (!enemyPoolReturn)
				enemyPoolReturn = enemy.gameObject.AddComponent<EnemyPoolReturn>();

			_poolReturns[enemy] = enemyPoolReturn;

			enemy.gameObject.SetActive(false);
			return enemy;
		}
	}
}
