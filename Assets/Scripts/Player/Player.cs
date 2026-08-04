using PlayerStats;
using Relic;
using UnityEngine;

namespace Player
{
	/// <summary>
	/// Root runtime component for the player prefab.
	/// </summary>
	/// <remarks>
	/// Assign physics, relic, and tuning references in the inspector; the component builds movement, stats, and XP services at runtime.
	/// </remarks>
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
		[SerializeField] private RelicSO StartingRelic;
		[SerializeField] private Vector2Int StartingRelicAnchor = Vector2Int.zero;

		[Header("XP & Leveling")]
		[SerializeField] private float[] LevelXpThresholds =
		{
			100f, 250f, 450f, 700f, 1000f, 1400f, 1900f, 2500f, 3200f, 4000f,
		};

		private PlayerMovement _movement;
		private PlayerEnemyPush _enemyPush;

		/// <summary>
		/// Current player stats model.
		/// </summary>
		public PlayerStatsModel Stats { get; private set; }
		/// <summary>
		/// Current relic manager.
		/// </summary>
		public PlayerRelicManager Relics { get; private set; }
		/// <summary>
		/// Current XP progression service.
		/// </summary>
		public PlayerXpService XP { get; private set; }

		/// <summary>
		/// Adds XP to the player.
		/// </summary>
		/// <param name="amount">XP amount to add.</param>
		public void AddXp(float amount)
		{
			XP.AddXp(amount);
		}

		/// <summary>
		/// Builds runtime helpers and applies inspector tuning.
		/// </summary>
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

			Relics = new PlayerRelicManager(Stats, BoardColumns, BoardRows, this);
			XP = new PlayerXpService(LevelXpThresholds);
			SetupStartingRelic();

			if (ForceInterpolation && Rigidbody && Rigidbody.interpolation == RigidbodyInterpolation2D.None)
				Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
		}

		/// <summary>
		/// Advances relic timers each frame.
		/// </summary>
		private void Update()
		{
			Relics.Tick(Time.deltaTime);
		}

		/// <summary>
		/// Applies queued input and enemy pushback.
		/// </summary>
		private void FixedUpdate()
		{
			_enemyPush.SetTuning(
				Stats.GetValue(PlayerStatType.PushRadius),
				Stats.GetValue(PlayerStatType.MaxEnemyPushSpeed));

			Vector2 movementDelta = _movement.ConsumePendingMovement();
			Vector2 enemyPush = _enemyPush.Compute(Rigidbody.position);
			_movement.Move(movementDelta + enemyPush);
		}

		/// <summary>
		/// Queues movement for the current frame.
		/// </summary>
		/// <param name="desiredDelta">Requested movement delta.</param>
		public void Move(Vector2 desiredDelta)
		{
			float maxMove = Stats.GetValue(PlayerStatType.MoveSpeed) * Time.deltaTime;
			desiredDelta = Vector2.ClampMagnitude(desiredDelta, maxMove);
			_movement.AddInput(desiredDelta);
		}

		/// <summary>
		/// Spawns the configured starting relic and places it on the board.
		/// </summary>
		private void SetupStartingRelic()
		{
			RelicSO startingRelic = StartingRelic;
			if (!startingRelic)
				startingRelic = Resources.Load<RelicSO>("Relics/StartingRelic");

			if (!startingRelic)
			{
				Debug.LogWarning("[Player] No starting relic assigned or found in Resources/Relics/StartingRelic.");
				return;
			}

			RelicInstance instance = Relics.AcquireToBag(startingRelic);

			bool previousLockState = Relics.IsInteractionLocked;
			Relics.IsInteractionLocked = false;

			if (!Relics.PlaceOnBoard(instance, StartingRelicAnchor))
				Debug.LogWarning($"[Player] Failed to place starting relic '{startingRelic.DisplayName}' at {StartingRelicAnchor}.");

			Relics.IsInteractionLocked = previousLockState;
		}

	}
}
