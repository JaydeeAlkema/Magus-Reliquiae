using UnityEngine;
using UnityEngine.Pool;

namespace Waves
{
	/// <summary>
	///     Release bridge for pooled enemy prefabs.
	/// </summary>
	/// <remarks>
	///     Attach this to the enemy prefab so the pool can reclaim the instance when the GameObject disables outside a release
	///     call.
	/// </remarks>
	// Pool release bridge for enemy prefabs. Add this to pooled enemy instances so WaveManager can return them to the right ObjectPool.
	public class EnemyPoolReturn : MonoBehaviour
	{
		/// <summary>
		///     True while the enemy is being returned to the pool.
		/// </summary>
		public bool IsReleasingToPool { get; set; }

		private Enemy.Enemy _enemy;
		private IObjectPool<Enemy.Enemy> _pool;

		/// <summary>
		///     Binds this bridge to a pooled enemy instance.
		/// </summary>
		/// <param name="enemy">Enemy instance to reclaim.</param>
		/// <param name="pool">Pool that owns the instance.</param>
		public void Setup(Enemy.Enemy enemy, IObjectPool<Enemy.Enemy> pool)
		{
			_enemy = enemy;
			_pool = pool;
		}

		private void OnDisable()
		{
			if (IsReleasingToPool)
				return;

			if (!_enemy || _pool == null || !this.gameObject.scene.IsValid())
				return;

			_pool.Release(_enemy);
		}
	}
}
