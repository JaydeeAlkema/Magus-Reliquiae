using Enemy;
using UnityEngine;

namespace Player
{
	/// <summary>
	///     Enemy pushback helper owned by <see cref="Player" />.
	/// </summary>
	/// <remarks>
	///     Create it from the player prefab and keep its tuning synced with the player's stat values.
	/// </remarks>
	public class PlayerEnemyPush
	{
		private float _pushRadius;
		private float _maxEnemyPushSpeed;

		/// <summary>
		///     Sets the initial pushback tuning.
		/// </summary>
		/// <param name="pushRadius">Player push radius.</param>
		/// <param name="maxEnemyPushSpeed">Maximum enemy push speed.</param>
		public void Setup(float pushRadius, float maxEnemyPushSpeed)
		{
			_pushRadius = pushRadius;
			_maxEnemyPushSpeed = maxEnemyPushSpeed;
		}

		/// <summary>
		///     Updates the runtime tuning values.
		/// </summary>
		/// <param name="pushRadius">Player push radius.</param>
		/// <param name="maxEnemyPushSpeed">Maximum enemy push speed.</param>
		public void SetTuning(float pushRadius, float maxEnemyPushSpeed)
		{
			_pushRadius = pushRadius;
			_maxEnemyPushSpeed = maxEnemyPushSpeed;
		}

		/// <summary>
		///     Computes the push offset against nearby enemies.
		/// </summary>
		/// <param name="playerPos">Current player position.</param>
		/// <returns>Push vector to apply this physics step.</returns>
		public Vector2 Compute(Vector2 playerPos)
		{
			Vector2 push = Vector2.zero;

			foreach (EnemyContact enemy in EnemyPushRegistry.Active)
			{
				Vector2 toPlayer = playerPos - (Vector2)enemy.Position;
				float combinedRadius = _pushRadius + enemy.Radius;
				float sqrDist = toPlayer.sqrMagnitude;

				if (sqrDist >= combinedRadius * combinedRadius || sqrDist < 1e-6f)
					continue;

				float dist = Mathf.Sqrt(sqrDist);
				float overlap = combinedRadius - dist;
				push += toPlayer / dist * overlap;
			}

			float maxStep = _maxEnemyPushSpeed * Time.fixedDeltaTime;
			if (push.sqrMagnitude > maxStep * maxStep)
				push = push.normalized * maxStep;

			return push;
		}
	}
}
