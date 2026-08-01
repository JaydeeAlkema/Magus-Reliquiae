using UnityEngine;

namespace Enemy
{
	public class EnemyMovement
	{
		public Vector2 CurrentPos => _transform.position;

		private float _moveSpeed;
		private Vector2 _targetPos;
		private Transform _transform;

		public void Setup(Enemy enemy, EnemyConfigSO config)
		{
			_transform = enemy.transform;
			_moveSpeed = config.MoveSpeed;
		}

		public void SetTargetPos(Vector2 targetPos)
		{
			_targetPos = targetPos;
		}

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
