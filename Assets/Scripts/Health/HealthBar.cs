using UnityEngine;

namespace Health
{
	public class HealthBar : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private SpriteRenderer FillSprite;
		[SerializeField] private SpriteRenderer BackgroundSprite;

		private Transform _owner;
		private float _fullWidth;
		private float _fullLocalPositionX;

		private void Awake()
		{
			if (!FillSprite)
				return;

			_fullWidth = FillSprite.transform.localScale.x;
			_fullLocalPositionX = FillSprite.transform.localPosition.x;
		}

		public void SetHealth(float current, float max)
		{
			float percent = max > 0f ? current / max : 0f;
			SetHealthPercent(percent);
		}

		public void SetHealthPercent(float percent)
		{
			percent = Mathf.Clamp01(percent);

			if (!FillSprite)
				return;

			Transform fillTransform = FillSprite.transform;

			Vector3 scale = fillTransform.localScale;
			scale.x = _fullWidth * percent;
			fillTransform.localScale = scale;

			Vector3 localPosition = fillTransform.localPosition;
			localPosition.x = _fullLocalPositionX - (_fullWidth - scale.x) * 0.5f;
			fillTransform.localPosition = localPosition;
		}
	}
}
