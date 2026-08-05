using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	/// <summary>
	///     Pure movement controller owned by <see cref="Player" />.
	/// </summary>
	/// <remarks>
	///     Keep it constructed by the player prefab and configure collision settings through the owning component.
	/// </remarks>
	public class PlayerMovement
	{
		private readonly List<RaycastHit2D> _hits = new();

		private Rigidbody2D _rigidbody;
		private ContactFilter2D _contactFilter;
		private float _skinWidth;
		private int _maxSlideIterations;
		private Vector2 _pendingMovement;

		/// <summary>
		///     Configures movement collision and cast settings.
		/// </summary>
		/// <param name="rigidbody">Player rigidbody.</param>
		/// <param name="contactFilter">Contact filter used for casts.</param>
		/// <param name="skinWidth">Small padding kept away from walls.</param>
		/// <param name="maxSlideIterations">Maximum slide attempts per move.</param>
		/// <param name="castBufferCapacity">Initial cast hit buffer capacity.</param>
		public void Setup(
			Rigidbody2D rigidbody,
			ContactFilter2D contactFilter,
			float skinWidth,
			int maxSlideIterations,
			int castBufferCapacity)
		{
			_rigidbody = rigidbody;
			_contactFilter = contactFilter;
			_skinWidth = skinWidth;
			_maxSlideIterations = maxSlideIterations;

			int bufferCapacity = Mathf.Max(1, castBufferCapacity);
			if (_hits.Capacity < bufferCapacity)
				_hits.Capacity = bufferCapacity;
		}

		/// <summary>
		///     Queues desired movement for the next physics step.
		/// </summary>
		/// <param name="desiredDelta">Desired movement delta.</param>
		public void AddInput(Vector2 desiredDelta)
		{
			_pendingMovement += desiredDelta;
		}

		/// <summary>
		///     Returns and clears the queued input movement.
		/// </summary>
		/// <returns>Queued movement delta.</returns>
		public Vector2 ConsumePendingMovement()
		{
			Vector2 pending = _pendingMovement;
			_pendingMovement = Vector2.zero;
			return pending;
		}

		/// <summary>
		///     Applies collision-aware movement.
		/// </summary>
		/// <param name="movement">Movement to attempt.</param>
		/// <returns>The amount actually moved.</returns>
		public Vector2 Move(Vector2 movement)
		{
			Vector2 remaining = movement;
			Vector2 moved = Vector2.zero;

			for (int i = 0; i < _maxSlideIterations && remaining.sqrMagnitude > 1e-8f; i++)
			{
				float castDistance = remaining.magnitude + _skinWidth;
				int hitCount = _rigidbody.Cast(remaining.normalized, _contactFilter, _hits, castDistance);

				if (hitCount == 0)
				{
					_rigidbody.position += remaining;
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

				float safeDistance = Mathf.Max(closest.distance - _skinWidth, 0f);
				Vector2 safeMove = remaining.normalized * safeDistance;

				_rigidbody.position += safeMove;
				moved += safeMove;

				Vector2 leftover = remaining - safeMove;
				remaining = leftover - Vector2.Dot(leftover, closest.normal) * closest.normal;
			}

			return moved;
		}
	}
}
