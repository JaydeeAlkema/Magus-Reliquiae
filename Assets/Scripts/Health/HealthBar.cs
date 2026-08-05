using UnityEngine;

namespace Health
{
	/// <summary>
	///     World-space health bar controller.
	/// </summary>
	/// <remarks>
	///     Attach this to the health bar prefab, assign the fill and background sprite renderers, then call
	///     <see cref="Initialize" /> before using <see cref="SetHealth" /> or <see cref="SetHealthPercent" />.
	/// </remarks>
	public class HealthBar : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private SpriteRenderer FillSprite;
		[SerializeField] private SpriteRenderer BackgroundSprite;

		private float _fullWidth;
		private float _fullLocalPositionX;
		private bool _hasCache;

		private void Awake()
		{
			RebuildCache();
		}

		/// <summary>
		///     Assigns the bar sprites and rebuilds the cached width/position values used for scaling.
		/// </summary>
		/// <param name="fillSprite">The sprite renderer that visually represents remaining health.</param>
		/// <param name="backgroundSprite">The sprite renderer used as the bar backdrop.</param>
		public void Initialize(SpriteRenderer fillSprite, SpriteRenderer backgroundSprite)
		{
			FillSprite = fillSprite;
			BackgroundSprite = backgroundSprite;
			RebuildCache();
		}

		private void RebuildCache()
		{
			if (!FillSprite)
			{
				_hasCache = false;
				return;
			}

			_fullWidth = FillSprite.transform.localScale.x;
			_fullLocalPositionX = FillSprite.transform.localPosition.x;
			_hasCache = true;
		}

		/// <summary>
		///     Sets the bar from a current and max health pair.
		/// </summary>
		/// <param name="current">Current health value.</param>
		/// <param name="max">Maximum health value.</param>
		public void SetHealth(float current, float max)
		{
			float percent = max > 0f ? current / max : 0f;
			SetHealthPercent(percent);
		}

		/// <summary>
		///     Sets the bar using a 0-1 percentage.
		/// </summary>
		/// <param name="percent">Health percentage, clamped to the 0-1 range.</param>
		public void SetHealthPercent(float percent)
		{
			percent = Mathf.Clamp01(percent);

			if (!FillSprite || !_hasCache)
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
