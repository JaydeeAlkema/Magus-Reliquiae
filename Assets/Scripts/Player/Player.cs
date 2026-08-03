using PlayerStats;
using Relic;
using UnityEngine;

namespace Player
{
	public class Player : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Rigidbody2D Rigidbody;
		[SerializeField] private ContactFilter2D ContactFilter;
		[SerializeField] private bool ForceInterpolation = true;

		[Header("Movement")]
		[SerializeField] private float MoveSpeed = 1f;

		[Header("Collision")]
		[SerializeField] private LayerMask CollisionMask;
		[SerializeField] private float SkinWidth = 0.02f;
		[SerializeField] private int MaxSlideIterations = 4;
		[SerializeField][Min(1)] private int CastBufferCapacity = 8;

		[Header("Enemy pushback")]
		[SerializeField] private float PushRadius = 0.4f;
		[SerializeField] private float MaxEnemyPushSpeed = 3f;

		private PlayerMovement _movement;
		private PlayerEnemyPush _enemyPush;
		private PlayerStatsModel _stats;
		private RelicInventory _relicInventory;

		public PlayerStatsModel Stats => _stats;
		public RelicInventory Relics => _relicInventory;

		private void Awake()
		{
			_stats = new PlayerStatsModel();
			_stats.SetBaseValue(PlayerStatType.MoveSpeed, MoveSpeed);
			_stats.SetBaseValue(PlayerStatType.PushRadius, PushRadius);
			_stats.SetBaseValue(PlayerStatType.MaxEnemyPushSpeed, MaxEnemyPushSpeed);

			_movement ??= new PlayerMovement();
			_movement.Setup(Rigidbody, ContactFilter, SkinWidth, MaxSlideIterations, CastBufferCapacity);

			_enemyPush ??= new PlayerEnemyPush();
			_enemyPush.Setup(PushRadius, MaxEnemyPushSpeed);
			_relicInventory = new RelicInventory(_stats);

			if (ForceInterpolation && Rigidbody && Rigidbody.interpolation == RigidbodyInterpolation2D.None)
				Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
		}

		private void FixedUpdate()
		{
			_enemyPush.SetTuning(
				_stats.GetValue(PlayerStatType.PushRadius),
				_stats.GetValue(PlayerStatType.MaxEnemyPushSpeed));

			Vector2 movementDelta = _movement.ConsumePendingMovement();
			Vector2 enemyPush = _enemyPush.Compute(Rigidbody.position);
			_movement.Move(movementDelta + enemyPush);
		}

		public void Move(Vector2 desiredDelta)
		{
			float maxMove = _stats.GetValue(PlayerStatType.MoveSpeed) * Time.deltaTime;
			desiredDelta = Vector2.ClampMagnitude(desiredDelta, maxMove);
			_movement.AddInput(desiredDelta);
		}
	}
}
