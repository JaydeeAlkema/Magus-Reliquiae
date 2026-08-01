using UnityEngine;

namespace Camera
{
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
