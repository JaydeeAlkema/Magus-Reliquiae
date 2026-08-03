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

		[Header("Relic Board")]
		[SerializeField][Min(1)] private int BoardColumns = 4;
		[SerializeField][Min(1)] private int BoardRows = 5;

		[Header("XP & Leveling")]
		[SerializeField] private float[] LevelXpThresholds =
		{
			100f, 250f, 450f, 700f, 1000f, 1400f, 1900f, 2500f, 3200f, 4000f,
		};

		private PlayerMovement _movement;
		private PlayerEnemyPush _enemyPush;

		public PlayerStatsModel Stats { get; private set; }
		public PlayerRelicManager Relics { get; private set; }
		public PlayerXpService XP { get; private set; }

		private void Awake()
		{
			Stats = new PlayerStatsModel();
			Stats.SetBaseValue(PlayerStatType.MoveSpeed, MoveSpeed);
			Stats.SetBaseValue(PlayerStatType.PushRadius, PushRadius);
			Stats.SetBaseValue(PlayerStatType.MaxEnemyPushSpeed, MaxEnemyPushSpeed);

			_movement ??= new PlayerMovement();
			_movement.Setup(Rigidbody, ContactFilter, SkinWidth, MaxSlideIterations, CastBufferCapacity);

			_enemyPush ??= new PlayerEnemyPush();
			_enemyPush.Setup(PushRadius, MaxEnemyPushSpeed);

			Relics = new PlayerRelicManager(Stats, BoardColumns, BoardRows);
			XP = new PlayerXpService(LevelXpThresholds);

			if (ForceInterpolation && Rigidbody && Rigidbody.interpolation == RigidbodyInterpolation2D.None)
				Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
		}

		private void Update()
		{
			Relics.Tick(Time.deltaTime);
		}

		private void FixedUpdate()
		{
			_enemyPush.SetTuning(
				Stats.GetValue(PlayerStatType.PushRadius),
				Stats.GetValue(PlayerStatType.MaxEnemyPushSpeed));

			Vector2 movementDelta = _movement.ConsumePendingMovement();
			Vector2 enemyPush = _enemyPush.Compute(Rigidbody.position);
			_movement.Move(movementDelta + enemyPush);
		}

		public void Move(Vector2 desiredDelta)
		{
			float maxMove = Stats.GetValue(PlayerStatType.MoveSpeed) * Time.deltaTime;
			desiredDelta = Vector2.ClampMagnitude(desiredDelta, maxMove);
			_movement.AddInput(desiredDelta);
		}
	}
}
