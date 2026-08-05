using System.Collections.Generic;
using UnityEngine;

namespace Waves
{
	/// <summary>
	///     Wave definition asset.
	/// </summary>
	/// <remarks>
	///     Create one asset per wave and assign enemy prefabs, the spawn area type, and the delay until the next wave.
	/// </remarks>
	[CreateAssetMenu(fileName = "Wave", menuName = "ScriptableObjects/Enemy/Wave", order = 0)]
	public class WaveSO : ScriptableObject
	{
		[Header("Wave")]
		/// <summary>
		/// Total enemies to spawn for the wave.
		/// </summary>
		public int EnemiesToSpawn;
		/// <summary>
		///     Chance-based enemy entries.
		/// </summary>
		public List<WaveEnemy> PossibleEnemies;
		/// <summary>
		///     Enemies that are always included.
		/// </summary>
		public List<WaveEnemy> GuaranteedEnemies;
		/// <summary>
		///     Delay before the next wave begins.
		/// </summary>
		public float TimeUntilNextWave;
		/// <summary>
		///     Spawn area type used by this wave.
		/// </summary>
		public WaveSpawnAreaType WaveAreaType;

		/// <summary>
		///     Builds the list of enemy prefabs to spawn for this wave.
		/// </summary>
		/// <returns>Ordered list of enemy prefabs for spawning.</returns>
		public List<Enemy.Enemy> GetEnemiesToSpawn()
		{
			List<Enemy.Enemy> enemies = new();
			int enemiesSpawned = 0;

			// First add guaranteed enemies to spawn. These could be elites or bosses, doesn't really matter.
			foreach (WaveEnemy guaranteedEnemy in GuaranteedEnemies)
			{
				enemies.Add(guaranteedEnemy.EnemyPrefab);
				enemiesSpawned++;
			}

			// Then spawn the rest of the enemies based on their spawn chance.
			while (enemiesSpawned < EnemiesToSpawn)
			{
				foreach (WaveEnemy possibleEnemy in PossibleEnemies)
				{
					if (enemiesSpawned >= EnemiesToSpawn)
						break;

					int roll = Random.Range(0, 100);
					if (roll >= possibleEnemy.EnemySpawnChance)
						continue;

					enemies.Add(possibleEnemy.EnemyPrefab);
					enemiesSpawned++;
				}
			}

			return enemies;
		}
	}
}
