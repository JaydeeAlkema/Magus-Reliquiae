using UnityEngine;

namespace Enemy
{
	/// <summary>
	///     Pure movement helper owned by <see cref="Enemy" />.
	/// </summary>
	/// <remarks>
	///     Enemy creates and drives this at runtime; it is not a scene component.
	/// </remarks>
	public class EnemyMovement
	{
		/// <summary>
		///     Current transform position.
		/// </summary>
		public Vector2 CurrentPos => _transform.position;

		private float _moveSpeed;
		private Vector2 _targetPos;
		private Transform _transform;

		/// <summary>
		///     Binds the helper to an enemy instance and config.
		/// </summary>
		/// <param name="enemy">Owning enemy.</param>
		/// <param name="config">Movement config asset.</param>
		public void Setup(Enemy enemy, EnemyConfigSO config)
		{
			_transform = enemy.transform;
			_moveSpeed = config.MoveSpeed;
		}

		/// <summary>
		///     Updates the target position the enemy should move toward.
		/// </summary>
		/// <param name="targetPos">World-space target position.</param>
		public void SetTargetPos(Vector2 targetPos)
		{
			_targetPos = targetPos;
		}

		/// <summary>
		///     Moves the enemy a single step toward the cached target.
		/// </summary>
		public void MoveTowardsTarget()
		{
			float maxDistanceDelta = _moveSpeed * Time.fixedDeltaTime;
			_transform.position = Vector2.MoveTowards(
				_transform.position,
				_targetPos,
				maxDistanceDelta);
		}
	}
}
