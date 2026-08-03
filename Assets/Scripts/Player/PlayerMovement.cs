using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public class PlayerMovement
	{
		private readonly List<RaycastHit2D> _hits = new();

		private Rigidbody2D _rigidbody;
		private ContactFilter2D _contactFilter;
		private float _skinWidth;
		private int _maxSlideIterations;
		private Vector2 _pendingMovement;

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

		public void AddInput(Vector2 desiredDelta)
		{
			_pendingMovement += desiredDelta;
		}

		public Vector2 ConsumePendingMovement()
		{
			Vector2 pending = _pendingMovement;
			_pendingMovement = Vector2.zero;
			return pending;
		}

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
