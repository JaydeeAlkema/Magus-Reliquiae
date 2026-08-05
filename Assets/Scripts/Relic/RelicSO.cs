using System;
using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	/// <summary>
	///     Relic definition asset.
	/// </summary>
	/// <remarks>
	///     Create one per relic, then assign icon, metadata, shape, and behavior assets in the inspector.
	/// </remarks>
	[CreateAssetMenu(fileName = "Relic", menuName = "ScriptableObjects/Relic/Relic", order = 0)]
	public class RelicSO : ScriptableObject
	{
		/// <summary>
		///     Unique relic identifier.
		/// </summary>
		public string Id;
		/// <summary>
		///     Display name shown in UI.
		/// </summary>
		public string DisplayName;
		/// <summary>
		///     Short description shown to the player.
		/// </summary>
		[TextArea(2, 6)] public string Description;
		/// <summary>
		///     Relic icon shown in UI.
		/// </summary>
		public Sprite Icon;
		/// <summary>
		///     Rarity used for offer weighting and visuals.
		/// </summary>
		public RelicRarity Rarity = RelicRarity.Common;
		/// <summary>
		///     Maximum relic level.
		/// </summary>
		[Min(1)] public int MaxLevel = 1;
		/// <summary>
		///     Asset that builds runtime relic behavior.
		/// </summary>
		[SerializeField] private RelicBehaviorSO Behavior;
		/// <summary>
		///     Optional text tags for filtering.
		/// </summary>
		public string[] Tags = Array.Empty<string>();
		/// <summary>
		///     Per-level data entries.
		/// </summary>
		public List<RelicLevelData> Levels = new();

		/// <summary>
		///     Optional shape per level.
		/// </summary>
		public readonly RelicShape[] ShapePerLevel = Array.Empty<RelicShape>();

		/// <summary>
		///     Gets the level data for the requested level.
		/// </summary>
		/// <param name="level">1-based level index.</param>
		/// <returns>The matching level data, or null.</returns>
		public RelicLevelData GetLevelData(int level)
		{
			if (Levels.Count == 0)
				return null;

			int index = Mathf.Clamp(level - 1, 0, Levels.Count - 1);
			return Levels[index];
		}

		/// <summary>
		///     Gets the placement shape for the requested level.
		/// </summary>
		/// <param name="level">1-based level index.</param>
		/// <returns>The matching shape, or the default shape.</returns>
		public RelicShape GetShape(int level)
		{
			if (ShapePerLevel == null || ShapePerLevel.Length == 0)
				return RelicShape.Default;

			int index = Mathf.Clamp(level - 1, 0, ShapePerLevel.Length - 1);
			RelicShape shape = ShapePerLevel[index];
			return shape is { IsValid: true } ? shape : RelicShape.Default;
		}

		/// <summary>
		///     Creates the runtime relic behavior.
		/// </summary>
		/// <returns>The created behavior, or null.</returns>
		public IRelicBehavior CreateBehavior()
		{
			return Behavior ? Behavior.CreateBehavior() : null;
		}
	}
}
