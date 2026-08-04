using UnityEngine;

namespace Camera
{
	/// <summary>
	/// Smoothly follows a target transform with a configurable offset.
	/// </summary>
	/// <remarks>
	/// Attach this to the camera object, assign a target, and tune <see cref="SmoothSpeed"/> and
	/// <see cref="Offset"/> in the inspector.
	/// </remarks>
	public class SmoothFollow : MonoBehaviour
	{
		[Header("Camera Follow Settings")]
		[SerializeField] private Transform Target;
		[SerializeField] private float SmoothSpeed = 0.125f;
		[SerializeField] private Vector3 Offset;

		private Vector3 _velocity;

		private void LateUpdate()
		{
			if (!Target)
				return;

			Vector3 desiredPosition = Target.position + Offset;
			this.transform.position = Vector3.SmoothDamp(
				this.transform.position,
				desiredPosition,
				ref _velocity,
				SmoothSpeed);
		}
	}
}
