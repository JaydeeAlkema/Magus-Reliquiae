using System.Collections.Generic;
using Enemy;
using UnityEngine;

namespace Player
{
	public class PlayerController : MonoBehaviour
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
		[SerializeField][Min(1)]
		private int CastBufferCapacity = 8;

		[Header("Enemy pushback")]
		[SerializeField] private float PushRadius = 0.4f;
		[SerializeField] private float MaxEnemyPushSpeed = 3f;

		private readonly List<RaycastHit2D> _hits = new();

		private Vector2 _pendingMovement;

		private void Awake()
		{
			int castBufferCapacity = Mathf.Max(1, CastBufferCapacity);
			if (_hits.Capacity < castBufferCapacity)
				_hits.Capacity = castBufferCapacity;

			if (ForceInterpolation && Rigidbody && Rigidbody.interpolation == RigidbodyInterpolation2D.None)
				Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
		}

		private void FixedUpdate()
		{
			Vector2 enemyPush = ComputeEnemyPush();
			SlideMove(_pendingMovement + enemyPush);
			_pendingMovement = Vector2.zero;
		}

		public void Move(Vector2 desiredDelta)
		{
			desiredDelta = Vector2.ClampMagnitude(desiredDelta, MoveSpeed * Time.deltaTime);
			_pendingMovement += desiredDelta;
		}

		private Vector2 ComputeEnemyPush()
		{
			Vector3 playerPos = Rigidbody.position;
			Vector2 push = Vector2.zero;

			List<EnemyContact> enemies = EnemyPushRegistry.Active;
			foreach (EnemyContact enemy in enemies)
			{
				Vector2 toPlayer = playerPos - enemy.Position;
				float combinedRadius = PushRadius + enemy.Radius;
				float sqrDist = toPlayer.sqrMagnitude;

				if (sqrDist >= combinedRadius * combinedRadius || sqrDist < 1e-6f)
					continue;

				float dist = Mathf.Sqrt(sqrDist);
				float overlap = combinedRadius - dist;
				push += toPlayer / dist * overlap;
			}

			float maxStep = MaxEnemyPushSpeed * Time.fixedDeltaTime;
			if (push.sqrMagnitude > maxStep * maxStep)
				push = push.normalized * maxStep;

			return push;
		}

		private Vector2 SlideMove(Vector2 movement)
		{
			Vector2 remaining = movement;
			Vector2 moved = Vector2.zero;

			for (int i = 0; i < MaxSlideIterations && remaining.sqrMagnitude > 1e-8f; i++)
			{
				float castDistance = remaining.magnitude + SkinWidth;
				int hitCount = Rigidbody.Cast(remaining.normalized, ContactFilter, _hits, castDistance);

				if (hitCount == 0)
				{
					Rigidbody.position += remaining;
					moved += remaining;
					remaining = Vector2.zero;
					break;
				}

				RaycastHit2D closest = _hits[0]; // Cast() results are sorted by distance
				if (closest.distance <= 1e-4f)
				{
					remaining = Vector2.zero;
					break;
				}

				float safeDistance = Mathf.Max(closest.distance - SkinWidth, 0f);
				Vector2 safeMove = remaining.normalized * safeDistance;

				Rigidbody.position += safeMove;
				moved += safeMove;

				Vector2 leftover = remaining - safeMove;
				remaining = leftover - Vector2.Dot(leftover, closest.normal) * closest.normal;
			}

			return moved;
		}
	}
}
